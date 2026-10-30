using System.Text.Json;
using Microsoft.JSInterop;

namespace SistemaVentasWeb.Services
{
    public class SesionUsuario
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    /// <summary>
    /// Guarda y recupera la sesión (token JWT + datos del usuario) en el localStorage del navegador.
    /// Si el token venció, la sesión se descarta sola.
    /// </summary>
    public class SesionService
    {
        private const string Clave = "sistemaventas.sesion";

        private readonly IJSRuntime _js;
        private SesionUsuario? _sesion;
        private bool _cargada;

        public SesionService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<SesionUsuario?> ObtenerAsync()
        {
            if (!_cargada)
            {
                _cargada = true;

                try
                {
                    var json = await _js.InvokeAsync<string?>("localStorage.getItem", Clave);
                    _sesion = string.IsNullOrWhiteSpace(json)
                        ? null
                        : JsonSerializer.Deserialize<SesionUsuario>(json);
                }
                catch (JsonException)
                {
                    _sesion = null;
                }
            }

            if (_sesion != null && _sesion.Expira.ToUniversalTime() <= DateTime.UtcNow)
            {
                await CerrarAsync();
            }

            return _sesion;
        }

        public async Task GuardarAsync(SesionUsuario sesion)
        {
            _sesion = sesion;
            _cargada = true;
            await _js.InvokeVoidAsync("localStorage.setItem", Clave, JsonSerializer.Serialize(sesion));
        }

        public async Task CerrarAsync()
        {
            _sesion = null;
            _cargada = true;
            await _js.InvokeVoidAsync("localStorage.removeItem", Clave);
        }
    }
}
