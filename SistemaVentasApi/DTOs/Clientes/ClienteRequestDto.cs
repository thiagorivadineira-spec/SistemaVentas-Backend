using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    public class ClienteRequestDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        public string? Documento { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
    }
}