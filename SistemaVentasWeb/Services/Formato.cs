using System.Globalization;

namespace SistemaVentasWeb.Services
{
    /// <summary>Formatos de dinero y fechas en español de Argentina.</summary>
    public static class Formato
    {
        private static readonly CultureInfo Cultura = Obtener();

        public static string Dinero(decimal valor) => "$ " + valor.ToString("N2", Cultura);

        public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Cultura);

        private static CultureInfo Obtener()
        {
            try
            {
                return new CultureInfo("es-AR");
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.InvariantCulture;
            }
        }
    }
}
