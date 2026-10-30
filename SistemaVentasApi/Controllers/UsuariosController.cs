using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasApi.Data;
using SistemaVentasApi.DTOs;
using SistemaVentasApi.Models;
using SistemaVentasApi.Seguridad;
using SistemaVentasApi.Services;

namespace SistemaVentasApi.Controllers
{
    // Gestión de usuarios del personal: solo administradores
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IAuditoriaService _auditoria;

        public UsuariosController(
            DataContext context,
            IPasswordHasher<Usuario> hasher,
            IAuditoriaService auditoria)
        {
            _context = context;
            _hasher = hasher;
            _auditoria = auditoria;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Username = u.Username,
                    Rol = u.Rol,
                    Activo = u.Activo,
                    ClienteId = u.ClienteId
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // POST: api/Usuarios  (crea Admin o Cajero)
        [HttpPost]
        public async Task<ActionResult<UsuarioResponseDto>> CrearUsuario(UsuarioRequestDto dto)
        {
            if (dto.Rol != Roles.Admin && dto.Rol != Roles.Cajero)
            {
                return BadRequest(new
                {
                    mensaje = $"Rol inválido. Permitidos: {Roles.Admin}, {Roles.Cajero}. Los clientes se registran por api/Auth/Registro."
                });
            }

            var username = dto.Username.Trim();

            if (await _context.Usuarios.AnyAsync(u => u.Username == username))
                return BadRequest(new { mensaje = "El nombre de usuario ya está en uso." });

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Username = username,
                Rol = dto.Rol,
                Activo = true
            };
            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                "USUARIO_CREADO", "Usuario", usuario.Id, $"Username: {username}; Rol: {usuario.Rol}");

            return StatusCode(201, new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                ClienteId = usuario.ClienteId
            });
        }

        // PUT: api/Usuarios/5/activo?activo=false
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            if (User.ObtenerUsuarioId() == id)
                return BadRequest(new { mensaje = "No podés cambiar el estado de tu propio usuario." });

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            usuario.Activo = activo;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                activo ? "USUARIO_ACTIVADO" : "USUARIO_DESACTIVADO", "Usuario", usuario.Id, $"Username: {usuario.Username}");

            return NoContent();
        }
    }
}
