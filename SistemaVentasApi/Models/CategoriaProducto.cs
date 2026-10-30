using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.Models
{
    public class CategoriaProducto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Una categoría puede tener muchos productos
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}