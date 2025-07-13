using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionVoluntariadoEventosAPI.Datos;
using GestionVoluntariadoEventosAPI.Models;
using Microsoft.CodeAnalysis.Scripting;
using GestionVoluntariadoEventosAPI.Models.DTO;

namespace GestionVoluntariadoEventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest("El ID del usuario en la URL no coincide con el ID del usuario en el cuerpo de la solicitud.");
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound($"Usuario con ID {id} no encontrado.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            // Validación de unicidad de campos
            if (await _context.Users.AnyAsync(u => u.UserName == user.UserName))
            {
                return Conflict("El nombre de usuario ya está en uso."); // 
            }
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
            {
                return Conflict("El número de teléfono ya está registrado."); // 
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return Conflict("El correo electrónico ya está registrado."); // 
            }

            // Validación de la contraseña: minúscula, mayúscula y número.
            // Esta validación se hará principalmente en el frontend para feedback inmediato.
            // En el backend, solo nos aseguramos de que haya algo y que luego se hashee.
            // La complejidad (mayúscula, minúscula, número) es mejor validarla en el frontend
            // para una mejor experiencia de usuario.

            // Hashing de contraseña (¡CRÍTICO para la seguridad!)
            // Instalar paquete NuGet: BCrypt.Net-Next
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Evita devolver la contraseña hasheada en la respuesta
            user.Password = null;
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"Usuario con ID {id} no encontrado.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // Nuevo: Endpoint para Login
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Se requiere nombre de usuario y contraseña.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return Unauthorized("Credenciales inválidas.");
            }

            //Verificar la contraseña hasheada
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            // Evita devolver la contraseña hasheada en la respuesta
            user.Password = null;
            return Ok(user);
        }
    }
}
