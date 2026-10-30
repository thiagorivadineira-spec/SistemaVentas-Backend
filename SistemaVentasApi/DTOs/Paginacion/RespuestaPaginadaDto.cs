namespace SistemaVentasApi.DTOs
{
    public class RespuestaPaginadaDto<T>
    {
        public int PaginaActual { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }

        public bool TienePaginaAnterior =>
            PaginaActual > 1;

        public bool TienePaginaSiguiente =>
            PaginaActual < TotalPaginas;

        public IEnumerable<T> Datos { get; set; } = new List<T>();

        public RespuestaPaginadaDto(
            IEnumerable<T> datos,
            int totalRegistros,
            int paginaActual,
            int tamanoPagina)
        {
            Datos = datos;
            TotalRegistros = totalRegistros;
            PaginaActual = paginaActual;
            TamanoPagina = tamanoPagina;

            TotalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)tamanoPagina
            );
        }
    }
}