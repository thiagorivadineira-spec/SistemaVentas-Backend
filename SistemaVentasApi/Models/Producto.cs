using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentasApi.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        public int PuntoReposicion { get; set; }

        public int StockMaximo { get; set; }

        public bool Activo { get; set; } = true;

        // Clave foránea
        public int CategoriaProductoId { get; set; }
        public CategoriaProducto CategoriaProducto { get; set; } = null!;

        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

        // Imágenes
        public ICollection<Imagen> Imagenes { get; set; } = new List<Imagen>();
    }
}