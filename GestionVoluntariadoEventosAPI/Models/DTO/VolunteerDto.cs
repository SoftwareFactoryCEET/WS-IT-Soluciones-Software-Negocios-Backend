using System.ComponentModel.DataAnnotations;

namespace GestionVoluntariadoEventosAPI.Models.DTO
{
    public class VolunteerDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder los 100 caracteres.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateOnly BirthDate { get; set; }

        [Required(ErrorMessage = "Las habilidades y experiencia son obligatorias.")]
        [StringLength(500, ErrorMessage = "Las habilidades y experiencia no pueden exceder los 500 caracteres.")]
        public string Skills { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "El número de teléfono debe tener entre 10 y 13 caracteres.")]
        // Se asume validación de formato colombiano más estricta en el frontend o con una expresión regular más avanzada si es necesario.
        // Ejemplo de RegEx para Colombia: ^(\+57)?(3\d{9})$
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Debe agregar al menos una franja horaria de disponibilidad.")]
        public List<AvailabilitySlotDto> AvailabilitySlots { get; set; } = new List<AvailabilitySlotDto>();
    }
}
