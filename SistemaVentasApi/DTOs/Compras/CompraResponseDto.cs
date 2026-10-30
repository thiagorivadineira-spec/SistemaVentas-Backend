namespace SistemaVentasApi.DTOs
{
    public class CompraResponseDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public int ProveedorId { get; set; }

        public string ProveedorNombre { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public string UsuarioNombre { get; set; } = string.Empty;

        public List<DetalleCompraResponseDto> Detalles { get; set; } = new();
    }
}