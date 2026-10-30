namespace SistemaVentasApi.DTOs
{
    public class CompraRequestDto
    {
        public int ProveedorId { get; set; }

        // El usuario que registra el ingreso se toma del token JWT.

        public List<DetalleCompraRequestDto> Detalles { get; set; } = new();
    }
}
