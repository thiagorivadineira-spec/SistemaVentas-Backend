
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasApi.Data;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Models;
using SistemaVentasApi.Seguridad;

namespace SistemaVentasApi.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Proveedores: solo administradores
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly DataContext _context;

        public ProveedoresController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Proveedores?pagina=1&tamanoPagina=10&buscar=tech&ordenarPor=nombre
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<ProveedorResponseDto>>> GetProveedores(
            [FromQuery] ParametrosPaginacionDto parametros)
        {
            var query = _context.Proveedores
                .AsNoTracking()
                .AsQueryable();

            // Búsqueda opcional
            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();

                query = query.Where(p =>
                    p.RazonSocial.ToLower().Contains(termino) ||
                    (p.Cuit != null && p.Cuit.ToLower().Contains(termino)) ||
                    (p.Email != null && p.Email.ToLower().Contains(termino)));
            }

            // Cantidad total antes de paginar
            var totalRegistros = await query.CountAsync();

            // Ordenamiento
            query = parametros.OrdenarPor?.ToLower() switch
            {
                "nombre" => query.OrderBy(p => p.RazonSocial),
                "nombre_desc" => query.OrderByDescending(p => p.RazonSocial),
                _ => query.OrderBy(p => p.Id)
            };

            // Paginación
            var proveedores = await query
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(p => new ProveedorResponseDto
                {
                    Id = p.Id,
                    RazonSocial = p.RazonSocial,
                    Cuit = p.Cuit,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Direccion = p.Direccion,
                    Activo = p.Activo
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<ProveedorResponseDto>(
                proveedores,
                totalRegistros,
                parametros.Pagina,
                parametros.TamanoPagina
            );

            return Ok(respuesta);
        }

        // GET: api/Proveedores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorResponseDto>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProveedorResponseDto
                {
                    Id = p.Id,
                    RazonSocial = p.RazonSocial,
                    Cuit = p.Cuit,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Direccion = p.Direccion,
                    Activo = p.Activo
                })
                .FirstOrDefaultAsync();

            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado."
                });
            }

            return Ok(proveedor);
        }

        // POST: api/Proveedores
        [HttpPost]
        public async Task<ActionResult<ProveedorResponseDto>> CrearProveedor(
            ProveedorRequestDto dto)
        {
            var proveedor = new Proveedor
            {
                RazonSocial = dto.RazonSocial,
                Cuit = dto.Cuit,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                Activo = dto.Activo
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            var response = new ProveedorResponseDto
            {
                Id = proveedor.Id,
                RazonSocial = proveedor.RazonSocial,
                Cuit = proveedor.Cuit,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Direccion = proveedor.Direccion,
                Activo = proveedor.Activo
            };

            return CreatedAtAction(
                nameof(GetProveedor),
                new { id = proveedor.Id },
                response
            );
        }

        // PUT: api/Proveedores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProveedor(
            int id,
            ProveedorRequestDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado."
                });
            }

            proveedor.RazonSocial = dto.RazonSocial;
            proveedor.Cuit = dto.Cuit;
            proveedor.Telefono = dto.Telefono;
            proveedor.Email = dto.Email;
            proveedor.Direccion = dto.Direccion;
            proveedor.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Proveedores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado."
                });
            }

            // Baja lógica
            proveedor.Activo = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}