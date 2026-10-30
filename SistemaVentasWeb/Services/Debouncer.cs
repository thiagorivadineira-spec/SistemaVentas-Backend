namespace SistemaVentasWeb.Services
{
    /// <summary>
    /// Espera unos milisegundos desde la última tecla antes de ejecutar la búsqueda,
    /// para no llamar a la API por cada letra que se escribe.
    /// </summary>
    public class Debouncer
    {
        private CancellationTokenSource? _cts;

        public async Task EjecutarAsync(Func<Task> accion, int milisegundos = 300)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Task.Delay(milisegundos, token);
            }
            catch (TaskCanceledException)
            {
                return; // llegó otra tecla: se descarta esta ejecución
            }

            await accion();
        }
    }
}
