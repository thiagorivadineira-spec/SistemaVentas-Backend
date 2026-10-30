namespace SistemaVentasApi.DTOs
{
    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }
}
