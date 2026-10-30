using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SistemaVentasApi.Data;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Models;
using SistemaVentasApi.Seguridad;
using SistemaVentasApi.Services;

namespace SistemaVentasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IAuditoriaService _auditoria;

        public AuthController(
            DataContext context,
            IConfiguration config,
            IPasswordHasher<Usuario> hasher,
            IAuditoriaService auditoria)
        {
            _context = context;
            _config = config;
            _hasher = hasher;
            _auditoria = auditoria;
        }

        // POST: api/Auth/Registro
        // Registro público: SIEMPRE crea un usuario con rol Cliente (y su ficha de cliente).
        // Los usuarios Cajero/Admin los crea un administrador desde api/Usuarios.
        [HttpPost("Registro")]
        public async Task<IActionResult> Registro(RegistroRequestDto registroDto)
        {
            var username = registroDto.Username.Trim();

            if (await _context.Usuarios.AnyAsync(u => u.Username == username))
                return BadRequest(new { mensaje = "El nombre de usuario ya está en uso." });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var cliente = new Cliente
            {
                Nombre = registroDto.Nombre.Trim(),
                Documento = registroDto.Documento,
                Telefono = registroDto.Telefono,
                Email = registroDto.Email,
                Direccion = registroDto.Direccion,
                Activo = true
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var nuevoUsuario = new Usuario
            {
                Nombre = registroDto.Nombre.Trim(),
                Username = username,
                Rol = Roles.Cliente,
                Activo = true,
                ClienteId = cliente.Id
            };
            nuevoUsuario.PasswordHash = _hasher.HashPassword(nuevoUsuario, registroDto.Password);

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditoria.RegistrarAsync(
                "REGISTRO_CLIENTE", "Usuario", nuevoUsuario.Id, $"Username: {username}", nuevoUsuario.Id);

            return Ok(new { mensaje = "Usuario registrado exitosamente." });
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginDto)
        {
            var username = loginDto.Username.Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username);

            var resultado = (usuario == null || !usuario.Activo)
                ? PasswordVerificationResult.Failed
                : VerificarPassword(usuario, loginDto.Password);

            if (usuario == null || resultado == PasswordVerificationResult.Failed)
            {
                await _auditoria.RegistrarAsync(
                    "LOGIN_FALLIDO", "Usuario", usuario?.Id, $"Username: {username}", usuario?.Id);

                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            }

            // Si el algoritmo de hash fue actualizado, se re-hashea con el nuevo
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = _hasher.HashPassword(usuario, loginDto.Password);
                await _context.SaveChangesAsync();
            }

            // --- FABRICACIÓN DEL TOKEN JWT ---
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Los "Claims" son los datos que viajan dentro del token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            if (usuario.ClienteId.HasValue)
            {
                claims.Add(new Claim(TokenClaimsExtensions.ClaimClienteId, usuario.ClienteId.Value.ToString()));
            }

            var expira = DateTime.UtcNow.AddHours(2); // El token dura 2 horas

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            // ---------------------------------

            await _auditoria.RegistrarAsync("LOGIN_OK", "Usuario", usuario.Id, null, usuario.Id);

            return Ok(new AuthResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.Rol,
                ClienteId = usuario.ClienteId,
                Token = tokenString,
                Expira = expira
            });
        }

        // POST: api/Auth/CambiarPassword
        [Authorize]
        [HttpPost("CambiarPassword")]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordRequestDto dto)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId.Value);
            if (usuario == null)
                return Unauthorized(new { mensaje = "Usuario no encontrado." });

            if (VerificarPassword(usuario, dto.PasswordActual) == PasswordVerificationResult.Failed)
                return BadRequest(new { mensaje = "La contraseña actual es incorrecta." });

            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.PasswordNueva);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("PASSWORD_CAMBIADA", "Usuario", usuario.Id);

            return Ok(new { mensaje = "Contraseña actualizada." });
        }

        private PasswordVerificationResult VerificarPassword(Usuario usuario, string password)
        {
            try
            {
                return _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            }
            catch (FormatException)
            {
                // El hash guardado no tiene formato válido
                return PasswordVerificationResult.Failed;
            }
        }
    }
}
