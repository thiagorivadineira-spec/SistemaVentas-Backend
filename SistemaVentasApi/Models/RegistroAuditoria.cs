using System.ComponentModel.DataAnnotations;

namespace SistemaVentasApi.Models
{
    /// <summary>
    /// Registro de auditoría: quién hizo qué y cuándo (ventas, ingresos de mercadería,
    /// inicios de sesión, reportes, errores del sistema).
    /// </summary>
    public class RegistroAuditoria
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Nulo cuando no hay usuario identificado (login fallido, error del sistema)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Entidad { get; set; }

        public int? EntidadId { get; set; }

        [MaxLength(500)]
        public string? Detalle { get; set; }

        [MaxLength(45)]
        public string? DireccionIp { get; set; }
    }
}
