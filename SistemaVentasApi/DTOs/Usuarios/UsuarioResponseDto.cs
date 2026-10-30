namespace SistemaVentasApi.DTOs
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int? ClienteId { get; set; }
    }
}
