using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionVoluntariadoEventosAPI.Datos;
using GestionVoluntariadoEventosAPI.Models;

namespace GestionVoluntariadoEventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        /// <summary>
        /// Obtiene una lista de eventos, filtrando aquellos que aún requieren voluntarios.
        /// </summary>
        /// <returns>Una lista de objetos Event.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            // Solo se deben mostrar eventos cuyo número de voluntarios requeridos sea superior a cero. [cite: 91]
            return await _context.Events
                                 .Where(e => e.VolunteersRequired > 0)
                                 .ToListAsync();
        }

        // GET: api/Events/5
        /// Obtiene los detalles de un evento específico por su ID.
        /// </summary>
        /// <param name="id">El ID del evento.</param>
        /// <returns>El objeto Event si se encuentra, de lo contrario NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound($"Evento con ID {id} no encontrado.");
            }

            return @event;
        }

        // PUT: api/Events/5
        /// <summary>
        /// Actualiza la información de un evento existente.
        /// </summary>
        /// <param name="id">El ID del evento a actualizar.</param>
        /// <param name="event">Los datos del evento actualizados.</param>
        /// <returns>NoContent si la actualización es exitosa, BadRequest o NotFound en caso de error.</returns>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, Event @event)
        {
            if (id != @event.Id)
            {
                return BadRequest("El ID del evento en la URL no coincide con el ID del evento en el cuerpo de la solicitud.");
            }

            // Validaciones adicionales de negocio antes de actualizar
            if (@event.DateTime < DateTime.Now)
            {
                return BadRequest("No se puede registrar un evento en una fecha pasada.");
            }
            if (@event.DurationMinutes < 5) // Mínimo de duración
            {
                return BadRequest("La duración mínima del evento es de 5 minutos.");
            }
            if (@event.VolunteersRequired < 0) // No puede ser negativo
            {
                return BadRequest("El número de voluntarios requeridos no puede ser negativo.");
            }


            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound($"Evento con ID {id} no encontrado.");
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // POST: api/Events

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            // Validaciones adicionales de negocio
            // No se pueden registrar eventos en fechas pasadas.
            if (@event.DateTime < DateTime.Now)
            {
                return BadRequest("No se puede registrar un evento en una fecha pasada.");
            }
            if (@event.DurationMinutes < 5)
            {
                return BadRequest("La duración mínima del evento es de 5 minutos.");
            }
            if (@event.VolunteersRequired < 1) // Debe requerir al menos un voluntario.
            {
                return BadRequest("El evento debe requerir al menos un voluntario.");
            }

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
        }

        // DELETE: api/Events/5
        /// <summary>
        /// Elimina un evento por su ID.
        /// </summary>
        /// <param name="id">El ID del evento a eliminar.</param>
        /// <returns>NoContent si la eliminación es exitosa, o NotFound en caso de no encontrar el evento.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound($"Evento con ID {id} no encontrado.");
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }


        // NUEVO: Endpoint para Asignar Voluntario a Evento
        /// <summary>
        /// Asigna un voluntario a un evento si se cumplen las condiciones de disponibilidad.
        /// </summary>
        /// <param name="eventId">El ID del evento.</param>
        /// <param name="volunteerId">El ID del voluntario.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpPost("{eventId}/assign-volunteer/{volunteerId}")] //Padres e hijos
        public async Task<IActionResult> AssignVolunteerToEvent(int eventId, int volunteerId)
        {
            // Validar si se ha seleccionado un evento y un voluntario.
            if (eventId <= 0 || volunteerId <= 0)
            {
                return BadRequest("Se debe seleccionar un evento y un voluntario para la asignación.");
            }
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null)
            {
                return NotFound($"Evento con ID {eventId} no encontrado.");
            }
            var volunteer = await _context.Volunteers
                                         .Include(v => v.AvailabilitySlots)
                                         .FirstOrDefaultAsync(v => v.Id == volunteerId);
            if (volunteer == null)
            {
                return NotFound($"Voluntario con ID {volunteerId} no encontrado.");
            }
            // El evento tiene un número de voluntarios requeridos mayor a cero.
            if (@event.VolunteersRequired <= 0)
            {
                return BadRequest($"El evento '{@event.Name}' ya no requiere más voluntarios.");
            }
            // Verificar si el voluntario ya está asignado a este evento para evitar duplicados
            if (await _context.EventVolunteers.AnyAsync(ev => ev.EventId == eventId && ev.VolunteerId == volunteerId))
            {
                return Conflict($"El voluntario '{volunteer.FullName}' ya está asignado al evento '{@event.Name}'.");
            }
            // Las fechas y horas disponibles del voluntario coinciden con las del evento.
            // Calcular el día de la semana correspondiente a la fecha del evento.
            var eventDayOfWeek = @event.DateTime.DayOfWeek.ToString(); // Obtener el día de la semana del evento (ej. "Saturday")
            var eventStartTime = TimeOnly.FromDateTime(@event.DateTime); // Obtener la hora de inicio del evento
            var eventEndTime = TimeOnly.FromDateTime(@event.DateTime.AddMinutes(@event.DurationMinutes)); // Calcular la hora de fin del evento

            var isVolunteerAvailable = volunteer.AvailabilitySlots.Any(slot =>
                string.Equals(slot.DayOfWeek, eventDayOfWeek, StringComparison.OrdinalIgnoreCase) && // Comparar el día de la semana ignorando mayúsculas/minúsculas
                eventStartTime >= slot.StartTime && // La hora de inicio del evento debe estar dentro de la franja del voluntario
                eventEndTime <= slot.EndTime
            );

            if (!isVolunteerAvailable)
            {
                return BadRequest($"El voluntario '{volunteer.FullName}' no está disponible para el evento '{@event.Name}' en la fecha y hora especificadas.");
            }

            // Si todas las condiciones se cumplen, asignar voluntario
            var eventVolunteer = new EventVolunteer
            {
                EventId = eventId,
                VolunteerId = volunteerId
            };

            _context.EventVolunteers.Add(eventVolunteer);
            @event.VolunteersRequired--; // Disminuir en uno el número de voluntarios requeridos para el evento.

            try
            {
                await _context.SaveChangesAsync();
                return Ok($"Voluntario '{volunteer.FullName}' asignado exitosamente al evento '{@event.Name}'."); // Registro exitoso.
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al asignar el voluntario al evento: {ex.Message}");
            }

        }

    }
}
