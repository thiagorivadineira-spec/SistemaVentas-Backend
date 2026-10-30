namespace SistemaVentasApi.DTOs
{
    public class CategoriaResponseDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}