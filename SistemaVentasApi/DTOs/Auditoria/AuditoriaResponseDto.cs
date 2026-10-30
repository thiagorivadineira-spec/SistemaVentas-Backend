namespace SistemaVentasApi.DTOs
{
    public class AuditoriaResponseDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string? Username { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public int? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? DireccionIp { get; set; }
    }
}
