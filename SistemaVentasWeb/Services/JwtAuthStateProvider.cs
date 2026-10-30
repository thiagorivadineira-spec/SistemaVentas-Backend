using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaVentasWeb.Services
{
    /// <summary>
    /// Le dice a Blazor quién es el usuario actual y cuál es su rol, a partir de la sesión guardada.
    /// Esto sirve para mostrar u ocultar pantallas; la seguridad real la aplica la API con el token.
    /// </summary>
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly SesionService _sesion;

        public JwtAuthStateProvider(SesionService sesion)
        {
            _sesion = sesion;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var sesion = await _sesion.ObtenerAsync();

            if (sesion == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, sesion.Id.ToString()),
                new Claim(ClaimTypes.Name, sesion.Nombre),
                new Claim(ClaimTypes.Role, sesion.Rol)
            };

            var identidad = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identidad));
        }

        public async Task IniciarSesionAsync(SesionUsuario sesion)
        {
            await _sesion.GuardarAsync(sesion);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task CerrarSesionAsync()
        {
            await _sesion.CerrarAsync();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
