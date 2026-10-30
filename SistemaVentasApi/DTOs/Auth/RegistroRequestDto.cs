using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    /// <summary>Registro público: siempre crea un usuario con rol Cliente.</summary>
    public class RegistroRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Documento { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }
    }
}
