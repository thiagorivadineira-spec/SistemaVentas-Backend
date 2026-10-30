
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasApi.Data;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Models;
using SistemaVentasApi.Seguridad;
using SistemaVentasApi.Services;

namespace SistemaVentasApi.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Ingreso de mercadería: solo administradores
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public ComprasController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Compras?pagina=1&tamanoPagina=10&buscar=proveedor&ordenarPor=fecha_desc
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<CompraResponseDto>>> GetCompras(
            [FromQuery] ParametrosPaginacionDto parametros)
        {
            var query = _context.Compras
                .AsNoTracking()
                .AsQueryable();

            // Búsqueda opcional por proveedor o usuario
            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();

                query = query.Where(c =>
                    c.Proveedor.RazonSocial.ToLower().Contains(termino) ||
                    c.Usuario.Nombre.ToLower().Contains(termino));
            }

            // Total antes de paginar
            var totalRegistros = await query.CountAsync();

            // Ordenamiento
            query = parametros.OrdenarPor?.ToLower() switch
            {
                "fecha" => query.OrderBy(c => c.Fecha),
                "fecha_desc" => query.OrderByDescending(c => c.Fecha),
                "total_asc" => query.OrderBy(c => c.Total),
                "total_desc" => query.OrderByDescending(c => c.Total),
                _ => query.OrderBy(c => c.Id)
            };

            // Paginación
            var compras = await query
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(c => new CompraResponseDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,

                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor.RazonSocial,

                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre,

                    Detalles = c.Detalles.Select(d => new DetalleCompraResponseDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<CompraResponseDto>(
                compras,
                totalRegistros,
                parametros.Pagina,
                parametros.TamanoPagina
            );

            return Ok(respuesta);
        }

        // GET: api/Compras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompraResponseDto>> GetCompra(int id)
        {
            var compra = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(c => c.Id == id)
                .Select(c => new CompraResponseDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,

                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor.RazonSocial,

                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre,

                    Detalles = c.Detalles.Select(d => new DetalleCompraResponseDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                return NotFound(new
                {
                    mensaje = "Compra no encontrada."
                });
            }

            return Ok(compra);
        }

        // POST: api/Compras
        [HttpPost]
        public async Task<ActionResult<CompraResponseDto>> CrearCompra(
            CompraRequestDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                return BadRequest(new
                {
                    mensaje = "La compra debe contener al menos un producto."
                });
            }

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == dto.ProveedorId);

            if (proveedor == null)
            {
                return BadRequest(new
                {
                    mensaje = "El proveedor indicado no existe."
                });
            }

            // El usuario que registra el ingreso sale del token JWT
            var usuarioId = User.ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Token inválido o usuario no identificado."
                });
            }

            // Transacción: compra + detalles + stock deben guardarse juntos
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = new Compra
                {
                    ProveedorId = dto.ProveedorId,
                    UsuarioId = usuarioId.Value,
                    Fecha = DateTime.Now,
                    Total = 0
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var detalleDto in dto.Detalles)
                {
                    if (detalleDto.Cantidad <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "La cantidad debe ser mayor a cero."
                        });
                    }

                    if (detalleDto.PrecioUnitario <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "El precio unitario debe ser mayor a cero."
                        });
                    }

                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Id == detalleDto.ProductoId);

                    if (producto == null)
                    {
                        return BadRequest(new
                        {
                            mensaje = $"El producto con ID {detalleDto.ProductoId} no existe."
                        });
                    }

                    var detalle = new DetalleCompra
                    {
                        CompraId = compra.Id,
                        ProductoId = producto.Id,
                        Cantidad = detalleDto.Cantidad,
                        PrecioUnitario = detalleDto.PrecioUnitario
                    };

                    _context.DetallesCompras.Add(detalle);

                    // Aumentar stock
                    producto.StockActual += detalleDto.Cantidad;

                    total += detalleDto.Cantidad * detalleDto.PrecioUnitario;
                }

                compra.Total = total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditoria.RegistrarAsync(
                    "COMPRA_CREADA",
                    "Compra",
                    compra.Id,
                    $"Proveedor: {proveedor.RazonSocial}; Ítems: {dto.Detalles.Count}; Total: {total:N2}");

                return CreatedAtAction(
                    nameof(GetCompra),
                    new { id = compra.Id },
                    new
                    {
                        mensaje = "Compra registrada correctamente.",
                        compraId = compra.Id,
                        total = compra.Total
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}