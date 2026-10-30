namespace SistemaVentasApi.DTOs
{
    public class ParametrosPaginacionDto
    {
        private const int MaxPageSize = 50;

        private int _pagina = 1;
        private int _tamanoPagina = 10;

        public int Pagina
        {
            get => _pagina;
            set => _pagina = value < 1 ? 1 : value;
        }

        public int TamanoPagina
        {
            get => _tamanoPagina;
            set => _tamanoPagina =
                value > MaxPageSize
                    ? MaxPageSize
                    : (value < 1 ? 1 : value);
        }

        public string? Buscar { get; set; }

        public string? OrdenarPor { get; set; } = "id";
    }
}