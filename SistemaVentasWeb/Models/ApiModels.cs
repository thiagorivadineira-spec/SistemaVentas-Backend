namespace SistemaVentasWeb.Models
{
    // Clases que reflejan el JSON que devuelve / recibe la API (SistemaVentasApi)

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }

    public class Paginado<T>
    {
        public int PaginaActual { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public List<T> Datos { get; set; } = new();
    }

    public class ImagenProducto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int PuntoReposicion { get; set; }
        public int StockMaximo { get; set; }
        public bool Activo { get; set; }
        public int CategoriaProductoId { get; set; }
        public string? CategoriaNombre { get; set; }
        public List<ImagenProducto> Imagenes { get; set; } = new();
    }

    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Documento { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }

    public class Proveedor
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? Cuit { get; set; }
        public bool Activo { get; set; }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int? ClienteId { get; set; }
    }

    public class UsuarioNuevo
    {
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cajero";
    }

    // ---- Ventas ----
    public class DetalleVentaRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaRequest
    {
        public int? ClienteId { get; set; }
        public List<DetalleVentaRequest> Detalles { get; set; } = new();
    }

    public class VentaCreada
    {
        public string? Mensaje { get; set; }
        public int VentaId { get; set; }
        public decimal Total { get; set; }
    }

    // ---- Compras (ingreso de mercadería) ----
    public class DetalleCompraRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CompraRequest
    {
        public int ProveedorId { get; set; }
        public List<DetalleCompraRequest> Detalles { get; set; } = new();
    }

    public class CompraCreada
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public string? ProveedorNombre { get; set; }
    }

    // ---- Auditoría, reportes, cuenta ----
    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string? Username { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public int? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? DireccionIp { get; set; }
    }

    public class EnviarReporteRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }

    public class CambiarPasswordRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }

    public class MensajeRespuesta
    {
        public string? Mensaje { get; set; }
    }

    // ---- Modelos solo de pantalla (no viajan a la API) ----
    public class LineaTicket
    {
        public Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => Cantidad * Producto.PrecioVenta;
    }

    public class LineaIngreso
    {
        public Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
