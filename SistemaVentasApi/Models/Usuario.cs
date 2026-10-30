using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Rol { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Solo para usuarios con rol Cliente: su ficha de cliente (las compras se registran a ese cliente)
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public ICollection<Compra> Compras { get; set; } = new List<Compra>();

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
