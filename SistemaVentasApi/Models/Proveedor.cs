using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Cuit { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    }
}