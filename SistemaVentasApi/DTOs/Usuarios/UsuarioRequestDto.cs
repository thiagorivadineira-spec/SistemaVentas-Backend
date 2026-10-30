using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    /// <summary>Alta de usuarios de personal (Admin o Cajero) por parte de un administrador.</summary>
    public class UsuarioRequestDto
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

        [Required]
        public string Rol { get; set; } = string.Empty;
    }
}
