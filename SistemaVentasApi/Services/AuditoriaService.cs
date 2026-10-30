using SistemaVentasApi.Data;
using SistemaVentasApi.Models;
using SistemaVentasApi.Seguridad;

namespace SistemaVentasApi.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(
            string accion,
            string? entidad = null,
            int? entidadId = null,
            string? detalle = null,
            int? usuarioId = null);
    }

    public class AuditoriaService : IAuditoriaService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(
            DataContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditoriaService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task RegistrarAsync(
            string accion,
            string? entidad = null,
            int? entidadId = null,
            string? detalle = null,
            int? usuarioId = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                var registro = new RegistroAuditoria
                {
                    Fecha = DateTime.Now,
                    UsuarioId = usuarioId ?? httpContext?.User.ObtenerUsuarioId(),
                    Accion = accion,
                    Entidad = entidad,
                    EntidadId = entidadId,
                    Detalle = detalle != null && detalle.Length > 500 ? detalle[..500] : detalle,
                    DireccionIp = httpContext?.Connection.RemoteIpAddress?.ToString()
                };

                _context.RegistrosAuditoria.Add(registro);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // La auditoría nunca debe romper la operación principal
                _logger.LogError(ex, "No se pudo registrar la auditoría de la acción {Accion}", accion);
            }
        }
    }
}
