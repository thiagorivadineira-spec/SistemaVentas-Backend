using Microsoft.EntityFrameworkCore;
using SistemaVentasApi.Models;

namespace SistemaVentasApi.Data
{
    public class DataContext : DbContext
    {
        // Constructor necesario
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // DbSets (Las tablas que se crearán en SQL Server)
        public DbSet<CategoriaProducto> CategoriasProductos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompras { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVentas { get; set; }
        public DbSet<Imagen> Imagenes { get; set; }
        public DbSet<RegistroAuditoria> RegistrosAuditoria { get; set; }

        // Seed Data para prueba de Endpoints Compras, Productos, Proveedores y Categorias

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario <-> Cliente: un usuario con rol Cliente tiene su ficha de cliente (como máximo una por usuario)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Cliente)
                .WithMany()
                .HasForeignKey(u => u.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.ClienteId)
                .IsUnique();

            // Auditoría: el historial se conserva aunque el usuario sea dado de baja (baja lógica)
            modelBuilder.Entity<RegistroAuditoria>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegistroAuditoria>()
                .HasIndex(a => a.Fecha);

            // Relación Producto -> Imagenes
            modelBuilder.Entity<Imagen>()
                .HasOne(i => i.Producto)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // CATEGORÍAS
            modelBuilder.Entity<CategoriaProducto>().HasData(
                new
                {
                    Id = 1,
                    Nombre = "Periféricos",
                    Descripcion = "Mouse, teclados y accesorios",
                    Activo = true
                },
                new
                {
                    Id = 2,
                    Nombre = "Almacenamiento",
                    Descripcion = "Discos SSD y dispositivos de almacenamiento",
                    Activo = true
                },
                new
                {
                    Id = 3,
                    Nombre = "Monitores",
                    Descripcion = "Monitores y pantallas",
                    Activo = true
                },
                new
                {
                    Id = 4,
                    Nombre = "Redes",
                    Descripcion = "Equipos y accesorios de red",
                    Activo = true
                }
            );

            // PROVEEDORES
            modelBuilder.Entity<Proveedor>().HasData(
                new
                {
                    Id = 1,
                    RazonSocial = "Distribuidora Tech SA",
                    Cuit = "30-11111111-1",
                    Telefono = "2954-123456",
                    Email = "ventas@distribuidoratech.com",
                    Direccion = "Av. Principal 123",
                    Activo = true
                },
                new
                {
                    Id = 2,
                    RazonSocial = "Informática Mayorista SRL",
                    Cuit = "30-22222222-2",
                    Telefono = "2954-654321",
                    Email = "contacto@informaticamayorista.com",
                    Direccion = "Calle Comercio 456",
                    Activo = true
                }
            );

            // PRODUCTOS
            modelBuilder.Entity<Producto>().HasData(
                new
                {
                    Id = 1,
                    Nombre = "Mouse Logitech M90",
                    Descripcion = "Mouse óptico USB",
                    PrecioVenta = 15000m,
                    StockActual = 20,
                    PuntoReposicion = 5,
                    StockMaximo = 30,
                    Activo = true,
                    CategoriaProductoId = 1
                },
                new
                {
                    Id = 2,
                    Nombre = "Teclado Redragon Kumara",
                    Descripcion = "Teclado mecánico RGB",
                    PrecioVenta = 65000m,
                    StockActual = 12,
                    PuntoReposicion = 4,
                    StockMaximo = 20,
                    Activo = true,
                    CategoriaProductoId = 1
                },
                new
                {
                    Id = 3,
                    Nombre = "SSD Kingston 480 GB",
                    Descripcion = "Disco sólido SATA 480 GB",
                    PrecioVenta = 48000m,
                    StockActual = 8,
                    PuntoReposicion = 3,
                    StockMaximo = 15,
                    Activo = true,
                    CategoriaProductoId = 2
                },
                new
                {
                    Id = 4,
                    Nombre = "Router TP-Link Archer C6",
                    Descripcion = "Router Wi-Fi doble banda",
                    PrecioVenta = 70000m,
                    StockActual = 6,
                    PuntoReposicion = 2,
                    StockMaximo = 10,
                    Activo = true,
                    CategoriaProductoId = 4
                }
            );

            // CLIENTES 
            modelBuilder.Entity<Cliente>().HasData(
                new
                {
                    Id = 1,
                    Nombre = "Consumidor Final",
                    Documento = "00000000",
                    Telefono = "S/N",
                    Email = "cf@sistema.com",
                    Direccion = "S/D",
                    Activo = true
                },
                new
                {
                    Id = 2,
                    Nombre = "Thiago Rivadineira",
                    Documento = "12345678",
                    Telefono = "2954-112233",
                    Email = "thiago@correo.com",
                    Direccion = "Av. San Martín 123",
                    Activo = true
                }
            );

            // USUARIOS (Para probar tu AuthController)
            modelBuilder.Entity<Usuario>().HasData(
                new
                {
                    Id = 1,
                    Nombre = "Administrador del Sistema",
                    Username = "admin",
                    PasswordHash = "123", 
                    Rol = "Admin",
                    Activo = true
                }
            );
        }

    }
}
