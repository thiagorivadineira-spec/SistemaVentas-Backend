using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Documento { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        // Un cliente puede tener muchas ventas
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}