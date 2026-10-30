namespace SistemaVentasApi.DTOs
{
    public class ImagenResponseDto
    {
        public int Id { get; set; }

        public string NombreOriginal { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string TipoContenido { get; set; } = string.Empty;

        public long TamanoBytes { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}