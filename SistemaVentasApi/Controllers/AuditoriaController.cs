using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasApi.Data;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Seguridad;

namespace SistemaVentasApi.Controllers
{
    // Consulta del registro de auditoría y errores: solo administradores
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly DataContext _context;

        public AuditoriaController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Auditoria?pagina=1&tamanoPagina=20&accion=VENTA_CREADA&desde=2026-10-01&hasta=2026-10-31&buscar=texto
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<AuditoriaResponseDto>>> GetAuditoria(
            [FromQuery] ParametrosPaginacionDto parametros,
            [FromQuery] string? accion,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var query = _context.RegistrosAuditoria
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(accion))
            {
                var accionNormalizada = accion.Trim().ToUpper();
                query = query.Where(a => a.Accion == accionNormalizada);
            }

            if (desde.HasValue)
            {
                var desdeFecha = desde.Value.Date;
                query = query.Where(a => a.Fecha >= desdeFecha);
            }

            if (hasta.HasValue)
            {
                var hastaFecha = hasta.Value.Date.AddDays(1);
                query = query.Where(a => a.Fecha < hastaFecha);
            }

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();
                query = query.Where(a => a.Detalle != null && a.Detalle.ToLower().Contains(termino));
            }

            var totalRegistros = await query.CountAsync();

            var registros = await query
                .OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.Id)
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(a => new AuditoriaResponseDto
                {
                    Id = a.Id,
                    Fecha = a.Fecha,
                    UsuarioId = a.UsuarioId,
                    Username = a.Usuario != null ? a.Usuario.Username : null,
                    Accion = a.Accion,
                    Entidad = a.Entidad,
                    EntidadId = a.EntidadId,
                    Detalle = a.Detalle,
                    DireccionIp = a.DireccionIp
                })
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<AuditoriaResponseDto>(
                registros,
                totalRegistros,
                parametros.Pagina,
                parametros.TamanoPagina));
        }
    }
}
