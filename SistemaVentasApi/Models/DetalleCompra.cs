using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentasApi.Models
{
    public class DetalleCompra
    {
        [Key]
        public int Id { get; set; }

        // Claves foráneas
        public int CompraId { get; set; }
        public Compra Compra { get; set; } = null!;

        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
