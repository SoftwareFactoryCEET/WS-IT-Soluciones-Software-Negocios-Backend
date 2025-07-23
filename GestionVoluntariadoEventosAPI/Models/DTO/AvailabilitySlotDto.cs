using System.ComponentModel.DataAnnotations;

namespace GestionVoluntariadoEventosAPI.Models.DTO
{
    public class AvailabilitySlotDto
    {

        [Required(ErrorMessage = "El día de la semana es obligatorio.")]
        [StringLength(20, ErrorMessage = "El día de la semana no puede exceder los 20 caracteres.")]
        public string DayOfWeek { get; set; } = null!;

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "La hora de finalización es obligatoria.")]
        public TimeOnly EndTime { get; set; }
    }
}
