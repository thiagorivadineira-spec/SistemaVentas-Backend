using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    public class VentaRequestDto
    {
        // Opcional: para Cajero/Admin, si no se indica se usa "Consumidor Final".
        // Para el rol Cliente se ignora: siempre se usa el cliente del propio usuario.
        public int? ClienteId { get; set; }

        [Required]
        [MinLength(1)]
        public List<DetalleVentaRequestDto> Detalles { get; set; } = new List<DetalleVentaRequestDto>();
    }
}
