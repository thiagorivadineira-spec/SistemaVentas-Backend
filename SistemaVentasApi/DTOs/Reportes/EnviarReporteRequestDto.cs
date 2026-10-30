using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.DTOs
{
    public class EnviarReporteRequestDto
    {
        // "stock", "entradas" o "salidas"
        [Required]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Solo aplican a "entradas" y "salidas". Por defecto: últimos 30 días.
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }
}
