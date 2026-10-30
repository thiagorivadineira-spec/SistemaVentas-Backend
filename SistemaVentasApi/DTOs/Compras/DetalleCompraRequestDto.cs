namespace SistemaVentasApi.DTOs
{
    public class DetalleCompraRequestDto
    {
        public int ProductoId { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}