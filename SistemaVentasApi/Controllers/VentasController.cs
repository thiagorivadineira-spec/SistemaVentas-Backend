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
    [Authorize] // Requiere autenticación: no se puede vender ni consultar ventas sin iniciar sesión
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        // Id del cliente "Consumidor Final" (dato semilla de la base)
        private const int ClienteConsumidorFinalId = 1;

        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public VentasController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Ventas?pagina=1&tamanoPagina=10&buscar=cliente&ordenarPor=fecha_desc
        // Admin: todas las ventas | Cajero: las que registró él | Cliente: solo sus compras
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<VentaResponseDto>>> GetVentas(
            [FromQuery] ParametrosPaginacionDto parametros)
        {
            var query = _context.Ventas
                .AsNoTracking()
                .AsQueryable();

            if (User.IsInRole(Roles.Cliente))
            {
                var clienteIdToken = User.ObtenerClienteId();
                if (clienteIdToken == null)
                    return Forbid();

                var clienteIdFiltro = clienteIdToken.Value;
                query = query.Where(v => v.ClienteId == clienteIdFiltro);
            }
            else if (User.IsInRole(Roles.Cajero))
            {
                var usuarioIdToken = User.ObtenerUsuarioId();
                if (usuarioIdToken == null)
                    return Forbid();

                var usuarioIdFiltro = usuarioIdToken.Value;
                query = query.Where(v => v.UsuarioId == usuarioIdFiltro);
            }

            // Búsqueda opcional por nombre del cliente
            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();

                query = query.Where(v =>
                    v.Cliente.Nombre.ToLower().Contains(termino));
            }

            // Cantidad total antes de paginar
            var totalRegistros = await query.CountAsync();

            // Ordenamiento
            query = parametros.OrdenarPor?.ToLower() switch
            {
                "fecha" => query.OrderBy(v => v.Fecha),
                "fecha_desc" => query.OrderByDescending(v => v.Fecha),
                "total_asc" => query.OrderBy(v => v.Total),
                "total_desc" => query.OrderByDescending(v => v.Total),
                _ => query.OrderBy(v => v.Id)
            };

            // Paginación
            var ventas = await query
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(v => new VentaResponseDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    Total = v.Total,
                    NombreCliente = v.Cliente.Nombre,
                    NombreUsuario = v.Usuario.Nombre,

                    Detalles = v.Detalles.Select(d => new DetalleVentaResponseDto
                    {
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<VentaResponseDto>(
                ventas,
                totalRegistros,
                parametros.Pagina,
                parametros.TamanoPagina
            );

            return Ok(respuesta);
        }

        // POST: api/Ventas
        [HttpPost]
        public async Task<ActionResult> PostVenta(VentaRequestDto ventaDto)
        {
            // El usuario sale SIEMPRE del token JWT, nunca del cuerpo de la petición
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

            if (ventaDto.Detalles == null || ventaDto.Detalles.Count == 0)
                return BadRequest(new { mensaje = "La venta debe contener al menos un producto." });

            // Cliente de la venta: un usuario Cliente compra a su propio nombre
            int clienteId;
            if (User.IsInRole(Roles.Cliente))
            {
                var clienteIdToken = User.ObtenerClienteId();
                if (clienteIdToken == null)
                    return Forbid();

                clienteId = clienteIdToken.Value;
            }
            else
            {
                clienteId = ventaDto.ClienteId ?? ClienteConsumidorFinalId;
            }

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clienteId);

            if (cliente == null || !cliente.Activo)
                return BadRequest(new { mensaje = "Cliente no válido o inactivo." });

            // Transacción: si algo falla o falta stock, no se descuenta nada
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var nuevaVenta = new Venta
            {
                ClienteId = clienteId,
                UsuarioId = usuarioId.Value,
                Fecha = DateTime.Now,
                Total = 0,
                Detalles = new List<DetalleVenta>()
            };

            foreach (var detalleDto in ventaDto.Detalles)
            {
                if (detalleDto.Cantidad <= 0)
                    return BadRequest(new { mensaje = "La cantidad debe ser mayor a cero." });

                var producto = await _context.Productos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == detalleDto.ProductoId);

                if (producto == null || !producto.Activo)
                    return BadRequest(new { mensaje = $"El producto ID {detalleDto.ProductoId} no existe o no está disponible." });

                var cantidad = detalleDto.Cantidad;

                // Descuento atómico: solo se aplica si todavía hay stock suficiente
                // (evita vender de más cuando hay ventas simultáneas)
                var filasAfectadas = await _context.Productos
                    .Where(p => p.Id == producto.Id && p.StockActual >= cantidad)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockActual, p => p.StockActual - cantidad));

                if (filasAfectadas == 0)
                    return BadRequest(new { mensaje = $"Stock insuficiente para {producto.Nombre}." });

                nuevaVenta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta
                });

                nuevaVenta.Total += cantidad * producto.PrecioVenta;
            }

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditoria.RegistrarAsync(
                "VENTA_CREADA",
                "Venta",
                nuevaVenta.Id,
                $"Cliente: {cliente.Nombre}; Ítems: {nuevaVenta.Detalles.Count}; Total: {nuevaVenta.Total:N2}");

            return StatusCode(201, new { Mensaje = "Venta registrada con éxito", VentaId = nuevaVenta.Id, Total = nuevaVenta.Total });
        }
    }
}
