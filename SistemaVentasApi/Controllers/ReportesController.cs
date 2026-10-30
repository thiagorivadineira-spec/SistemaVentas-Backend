using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Seguridad;
using SistemaVentasApi.Services;

namespace SistemaVentasApi.Controllers
{
    // Reportes PDF: solo administradores
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reportes;
        private readonly IEmailService _email;
        private readonly IAuditoriaService _auditoria;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            IReporteService reportes,
            IEmailService email,
            IAuditoriaService auditoria,
            ILogger<ReportesController> logger)
        {
            _reportes = reportes;
            _email = email;
            _auditoria = auditoria;
            _logger = logger;
        }

        // GET: api/Reportes/stock
        // GET: api/Reportes/entradas?desde=2026-10-01&hasta=2026-10-31
        // GET: api/Reportes/salidas?desde=2026-10-01&hasta=2026-10-31
        [HttpGet("{tipo}")]
        public async Task<IActionResult> Descargar(
            string tipo,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            string tipoNormalizado;
            byte[] pdf;

            try
            {
                tipoNormalizado = _reportes.NormalizarTipo(tipo);
                pdf = await _reportes.GenerarAsync(tipoNormalizado, desde, hasta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

            await _auditoria.RegistrarAsync("REPORTE_GENERADO", "Reporte", null, $"Tipo: {tipoNormalizado}");

            return File(pdf, "application/pdf", NombreArchivo(tipoNormalizado));
        }

        // POST: api/Reportes/enviar   { "tipo": "stock", "email": "alguien@correo.com", "desde": null, "hasta": null }
        [HttpPost("enviar")]
        public async Task<IActionResult> Enviar(EnviarReporteRequestDto dto)
        {
            if (!_email.EstaConfigurado)
            {
                return StatusCode(503, new { mensaje = "El envío de correo no está configurado en el servidor." });
            }

            string tipoNormalizado;
            byte[] pdf;

            try
            {
                tipoNormalizado = _reportes.NormalizarTipo(dto.Tipo);
                pdf = await _reportes.GenerarAsync(tipoNormalizado, dto.Desde, dto.Hasta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

            var nombreArchivo = NombreArchivo(tipoNormalizado);

            try
            {
                await _email.EnviarConAdjuntoAsync(
                    dto.Email,
                    $"Reporte de {tipoNormalizado}",
                    $"Adjuntamos el reporte de {tipoNormalizado} generado el {DateTime.Now:dd/MM/yyyy HH:mm}.",
                    nombreArchivo,
                    pdf);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el reporte {Tipo} a {Email}", tipoNormalizado, dto.Email);

                await _auditoria.RegistrarAsync(
                    "REPORTE_ENVIO_FALLIDO", "Reporte", null, $"Tipo: {tipoNormalizado}; Destino: {dto.Email}");

                return StatusCode(502, new { mensaje = "No se pudo enviar el correo. Revisá la configuración SMTP." });
            }

            await _auditoria.RegistrarAsync(
                "REPORTE_ENVIADO", "Reporte", null, $"Tipo: {tipoNormalizado}; Destino: {dto.Email}");

            return Ok(new { mensaje = $"Reporte enviado a {dto.Email}." });
        }

        private static string NombreArchivo(string tipo) =>
            $"reporte-{tipo}-{DateTime.Now:yyyyMMdd-HHmm}.pdf";
    }
}
