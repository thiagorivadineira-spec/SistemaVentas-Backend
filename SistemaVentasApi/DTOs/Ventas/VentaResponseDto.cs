namespace SistemaVentasApi.DTOs
{
    public class VentaResponseDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty; // Quien registró la venta
        public List<DetalleVentaResponseDto> Detalles { get; set; } = new List<DetalleVentaResponseDto>();
    }
}