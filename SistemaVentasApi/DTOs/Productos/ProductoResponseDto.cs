namespace SistemaVentasApi.DTOs
{
    public class ProductoResponseDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        public int PuntoReposicion { get; set; }

        public int StockMaximo { get; set; }

        public bool Activo { get; set; }

        public int CategoriaProductoId { get; set; }

        public string? CategoriaNombre { get; set; }
        public List<ImagenResponseDto> Imagenes { get; set; } = new();

    }
}