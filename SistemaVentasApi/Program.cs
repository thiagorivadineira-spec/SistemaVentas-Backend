using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using SistemaVentasApi.Data;
using SistemaVentasApi.Models;
using SistemaVentasApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Validar que la clave JWT fue reemplazada por una propia
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || jwtKey.StartsWith("CAMBIAR"))
{
    throw new InvalidOperationException(
        "Configurá Jwt:Key con una clave propia de al menos 32 caracteres (user-secrets o variable de entorno Jwt__Key).");
}

// 0. Logging con Serilog: consola + archivos diarios en la carpeta "logs"
//    logs/log-AAAAMMDD.txt     -> todo lo importante (Information o superior)
//    logs/errores-AAAAMMDD.txt -> solo errores (se conservan 90 días)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .WriteTo.File("logs/errores-.txt", restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90));

// 1. Configuración de JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Obtener la cadena de conexión de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar el DataContext para usar SQL Server
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

// Servicios propios: hash de contraseñas y auditoría
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// Reportes PDF (QuestPDF, licencia Community) y envío de correo (MailKit)
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Controladores
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese el token JWT."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// CORS permisivo
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPermisivo", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();

// Registra cada petición HTTP en el log
app.UseSerilogRequestLogging();

// Swagger en entorno de produccion
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("CorsPermisivo");

// Manejo global de errores: se registra en el log (archivo) y en la tabla de auditoría,
// y el cliente recibe un 500 con un identificador para rastrear el error.
// Va después de CORS para que la respuesta de error conserve las cabeceras CORS.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error no controlado en {Metodo} {Ruta} (TraceId {TraceId})",
            context.Request.Method, context.Request.Path, context.TraceIdentifier);

        try
        {
            // Scope nuevo: el DataContext de la petición puede haber quedado en mal estado
            using var scope = context.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var auditoria = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

            await auditoria.RegistrarAsync(
                "ERROR",
                "Sistema",
                null,
                $"{context.Request.Method} {context.Request.Path}: {ex.GetType().Name} - {ex.Message} (TraceId {context.TraceIdentifier})");
        }
        catch
        {
            // El registro de auditoría nunca debe impedir responder
        }

        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "Ocurrió un error inesperado. Intente nuevamente.",
                traceId = context.TraceIdentifier
            });
        }
    }
});

// Habilitar archivos estáticos de wwwroot
app.UseStaticFiles();

// Crear la carpeta wwwroot/uploads si no existe
var uploadsPath = Path.Combine(
    app.Environment.WebRootPath
        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
    "uploads"
);

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Migración de datos al iniciar: las contraseñas guardadas en texto plano (por ejemplo la del
// usuario semilla "admin") se convierten a hash. Los hashes de ASP.NET Identity empiezan con "AQAAAA".
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

        var heredados = await db.Usuarios
            .Where(u => !u.PasswordHash.StartsWith("AQAAAA"))
            .ToListAsync();

        foreach (var usuario in heredados)
        {
            usuario.PasswordHash = hasher.HashPassword(usuario, usuario.PasswordHash);
        }

        if (heredados.Count > 0)
        {
            await db.SaveChangesAsync();
            Log.Information("Se convirtieron {Cantidad} contraseñas heredadas a hash.", heredados.Count);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "No se pudo revisar las contraseñas de usuarios al iniciar. ¿Aplicaste las migraciones (Update-Database)?");
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
