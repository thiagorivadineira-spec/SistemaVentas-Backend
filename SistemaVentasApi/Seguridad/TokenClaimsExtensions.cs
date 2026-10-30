using System.Security.Claims;

namespace SistemaVentasApi.Seguridad
{
    public static class TokenClaimsExtensions
    {
        public const string ClaimClienteId = "clienteId";

        /// <summary>Id del usuario autenticado (claim del token JWT).</summary>
        public static int? ObtenerUsuarioId(this ClaimsPrincipal user)
        {
            var valor = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue("nameid")
                        ?? user.FindFirstValue("sub");

            return int.TryParse(valor, out var id) ? id : null;
        }

        /// <summary>Id de la ficha de cliente asociada al usuario (solo rol Cliente).</summary>
        public static int? ObtenerClienteId(this ClaimsPrincipal user)
        {
            var valor = user.FindFirstValue(ClaimClienteId);

            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
