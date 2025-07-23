using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionVoluntariadoEventosAPI.Datos;
using GestionVoluntariadoEventosAPI.Models;
using GestionVoluntariadoEventosAPI.Models.DTO;


namespace GestionVoluntariadoEventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VolunteersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Volunteers
        /// <summary>
        /// Obtiene una lista de todos los voluntarios registrados.
        /// </summary>
        /// <returns>Una lista de objetos Volunteer.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Volunteer>>> GetVolunteers()
        {
            return await _context.Volunteers
                                 .Include(v => v.AvailabilitySlots) // Incluir las franjas de disponibilidad
                                 .ToListAsync();
        }

        // GET: api/Volunteers/5
        /// <summary>
        /// Obtiene los detalles de un voluntario específico por su ID.
        /// </summary>
        /// <param name="id">El ID del voluntario.</param>
        /// <returns>El objeto Volunteer si se encuentra, de lo contrario NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Volunteer>> GetVolunteer(int id)
        {
            var volunteer = await _context.Volunteers
                              .Include(v => v.AvailabilitySlots)
                              .FirstOrDefaultAsync(v => v.Id == id);

            if (volunteer == null)
            {
                return NotFound($"Voluntario con ID {id} no encontrado.");
            }

            return volunteer;
        }

        // PUT: api/Volunteers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Actualiza la información de un voluntario existente.
        /// </summary>
        /// <param name="id">El ID del voluntario a actualizar.</param>
        /// <param name="volunteerDto">Los datos del voluntario actualizados.</param>
        /// <returns>NoContent si la actualización es exitosa, BadRequest o NotFound en caso de error.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVolunteer(int id, VolunteerDto volunteerDto)
        {
            var volunteerToUpdate = await _context.Volunteers
                                                   .Include(v => v.AvailabilitySlots)
                                                   .FirstOrDefaultAsync(v => v.Id == id);

            if (volunteerToUpdate == null)
            {
                return NotFound($"Voluntario con ID {id} no encontrado.");
            }

            // Validaciones de unicidad para Email y PhoneNumber al actualizar
            if (await _context.Volunteers.AnyAsync(v => v.Email == volunteerDto.Email && v.Id != id))
            {
                return Conflict("El correo electrónico ya está registrado por otro voluntario.");
            }
            if (await _context.Volunteers.AnyAsync(v => v.PhoneNumber == volunteerDto.PhoneNumber && v.Id != id))
            {
                return Conflict("El número de teléfono ya está registrado por otro voluntario.");
            }

            // Validar disponibilidad
            if (volunteerDto.AvailabilitySlots == null || !volunteerDto.AvailabilitySlots.Any())
            {
                return BadRequest("Se debe agregar al menos una franja horaria de disponibilidad.");
            }

            foreach (var slotDto in volunteerDto.AvailabilitySlots)
            {
                if (slotDto.StartTime >= slotDto.EndTime)
                {
                    return BadRequest("La hora de inicio debe ser menor que la hora de finalización en una franja horaria.");
                }
                if ((slotDto.EndTime - slotDto.StartTime).TotalMinutes < 60)
                {
                    return BadRequest("Las franjas horarias deben tener una duración mínima de una hora.");
                }
                if (slotDto.StartTime.Minute != 0 || slotDto.EndTime.Minute != 0)
                {
                    return BadRequest("Solo se considerarán horas exactas (ej. 12:00, 13:00, etc.).");
                }
            }
            // Validar franjas horarias duplicadas para el mismo voluntario
            if (volunteerDto.AvailabilitySlots.GroupBy(s => new { s.DayOfWeek, s.StartTime, s.EndTime }).Any(g => g.Count() > 1))
            {
                return BadRequest("No se pueden registrar franjas horarias duplicadas para el mismo voluntario.");
            }


            // Mapear los datos del DTO al modelo existente. Es más eficiente con Autommaper
            volunteerToUpdate.FullName = volunteerDto.FullName;
            volunteerToUpdate.BirthDate = volunteerDto.BirthDate;
            volunteerToUpdate.Skills = volunteerDto.Skills;
            volunteerToUpdate.Email = volunteerDto.Email;
            volunteerToUpdate.PhoneNumber = volunteerDto.PhoneNumber;

            // Actualizar AvailabilitySlots: eliminar las viejas y agregar las nuevas
            _context.AvailabilitySlots.RemoveRange(volunteerToUpdate.AvailabilitySlots);
            foreach (var slotDto in volunteerDto.AvailabilitySlots)
            {
                volunteerToUpdate.AvailabilitySlots.Add(new AvailabilitySlot
                {
                    DayOfWeek = slotDto.DayOfWeek,
                    StartTime = slotDto.StartTime,
                    EndTime = slotDto.EndTime
                });
            }

            _context.Entry(volunteerToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // Registro exitoso.
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VolunteerExists(id))
                {
                    return NotFound($"Voluntario con ID {id} no encontrado.");
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                // Manejo de errores específicos de la base de datos (ej. violación de restricciones)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar el voluntario: {ex.Message}");
            }
        }

        // POST: api/Volunteers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Registra un nuevo voluntario en el sistema.
        /// </summary>
        /// <param name="volunteerDto">Los datos del voluntario a registrar.</param>
        /// <returns>El voluntario creado si es exitoso, o BadRequest/Conflict en caso de error.</returns>
        [HttpPost]
        public async Task<ActionResult<Volunteer>> PostVolunteer(VolunteerDto volunteerDto)
        {
            // Validaciones de unicidad
            if (await _context.Volunteers.AnyAsync(v => v.Email == volunteerDto.Email))
            {
                return Conflict("El correo electrónico ya está registrado.");
            }
            if (await _context.Volunteers.AnyAsync(v => v.PhoneNumber == volunteerDto.PhoneNumber))
            {
                return Conflict("El número de teléfono ya está registrado.");
            }

            // Validar disponibilidad
            if (volunteerDto.AvailabilitySlots == null || !volunteerDto.AvailabilitySlots.Any())
            {
                return BadRequest("Se debe agregar al menos una franja horaria de disponibilidad.");
            }

            foreach (var slotDto in volunteerDto.AvailabilitySlots)
            {
                if (slotDto.StartTime >= slotDto.EndTime)
                {
                    return BadRequest("La hora de inicio debe ser menor que la hora de finalización en una franja horaria.");
                }
                if ((slotDto.EndTime - slotDto.StartTime).TotalMinutes < 60)
                {
                    return BadRequest("Las franjas horarias deben tener una duración mínima de una hora.");
                }
                if (slotDto.StartTime.Minute != 0 || slotDto.EndTime.Minute != 0)
                {
                    return BadRequest("Solo se considerarán horas exactas (ej. 12:00, 13:00, etc.).");
                }
            }

            // Validar franjas horarias duplicadas
            if (volunteerDto.AvailabilitySlots.GroupBy(s => new { s.DayOfWeek, s.StartTime, s.EndTime }).Any(g => g.Count() > 1))
            {
                return BadRequest("No se pueden registrar franjas horarias duplicadas.");
            }

            var volunteer = new Volunteer
            {
                FullName = volunteerDto.FullName,
                BirthDate = volunteerDto.BirthDate,
                Skills = volunteerDto.Skills,
                Email = volunteerDto.Email,
                PhoneNumber = volunteerDto.PhoneNumber
            };

            // Mapear y añadir franjas de disponibilidad
            foreach (var slotDto in volunteerDto.AvailabilitySlots)
            {
                volunteer.AvailabilitySlots.Add(new AvailabilitySlot
                {
                    DayOfWeek = slotDto.DayOfWeek,
                    StartTime = slotDto.StartTime,
                    EndTime = slotDto.EndTime
                });
            }

            _context.Volunteers.Add(volunteer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Manejo de errores específicos de la base de datos (ej. violación de restricciones)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al guardar el voluntario: {ex.Message}");
            }

            // Notificación de registro exitoso.
            return CreatedAtAction("GetVolunteer", new { id = volunteer.Id }, volunteer);
        }

        // DELETE: api/Volunteers/5
        /// <summary>
        /// Elimina un voluntario por su ID.
        /// </summary>
        /// <param name="id">El ID del voluntario a eliminar.</param>
        /// <returns>NoContent si la eliminación es exitosa, o NotFound en caso de no encontrar el voluntario.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVolunteer(int id)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer == null)
            {
                return NotFound($"Voluntario con ID {id} no encontrado.");
            }

            _context.Volunteers.Remove(volunteer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VolunteerExists(int id)
        {
            return _context.Volunteers.Any(e => e.Id == id);
        }
    }
}
