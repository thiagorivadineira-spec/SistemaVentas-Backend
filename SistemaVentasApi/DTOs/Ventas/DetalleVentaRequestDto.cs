using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    public class DetalleVentaRequestDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}