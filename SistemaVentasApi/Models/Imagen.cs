using System.Text.Json.Serialization;

namespace SistemaVentasApi.Models
{
    public class Imagen
    {
        public int Id { get; set; }

        public string NombreOriginal { get; set; } = string.Empty;

        public string NombreArchivo { get; set; } = string.Empty;

        // Ejemplo: uploads/archivo-guid.png
        public string RutaRelativa { get; set; } = string.Empty;

        // image/png o image/jpeg
        public string TipoContenido { get; set; } = string.Empty;

        public long TamanoBytes { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // FK
        public int ProductoId { get; set; }

        // Navegación
        [JsonIgnore]
        public Producto? Producto { get; set; }
    }
}