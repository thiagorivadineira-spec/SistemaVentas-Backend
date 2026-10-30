using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentasApi.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;


        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Claves foráneas
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        // Una compra puede tener varios productos
        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}