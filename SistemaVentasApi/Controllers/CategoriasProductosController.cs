
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasApi.Data;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Models;
using SistemaVentasApi.Seguridad;

namespace SistemaVentasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasProductosController : ControllerBase
    {
        private readonly DataContext _context;

        public CategoriasProductosController(DataContext context)
        {
            _context = context;
        }

        // GET: api/CategoriasProductos
        [AllowAnonymous] // El catálogo puede consultarse sin iniciar sesión
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> GetCategorias()
        {
            var categorias = await _context.CategoriasProductos
                .AsNoTracking()
                .Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/CategoriasProductos/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponseDto>> GetCategoria(int id)
        {
            var categoria = await _context.CategoriasProductos
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });
            }

            return Ok(categoria);
        }

        // POST: api/CategoriasProductos
        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<ActionResult<CategoriaResponseDto>> CrearCategoria(
            CategoriaRequestDto dto)
        {
            var categoria = new CategoriaProducto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = dto.Activo
            };

            _context.CategoriasProductos.Add(categoria);
            await _context.SaveChangesAsync();

            var response = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo
            };

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoria.Id },
                response
            );
        }

        // PUT: api/CategoriasProductos/5
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(
            int id,
            CategoriaRequestDto dto)
        {
            var categoria = await _context.CategoriasProductos.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });
            }

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            categoria.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CategoriasProductos/5
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.CategoriasProductos.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });
            }

            categoria.Activo = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}