using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    public class ProductoRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        public int PuntoReposicion { get; set; }

        public int StockMaximo { get; set; }

        public bool Activo { get; set; } = true;

        public int CategoriaProductoId { get; set; }
    }
}