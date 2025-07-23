using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionVoluntariadoEventosAPI.Models;

public partial class Event
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La fecha y hora del evento son obligatorias.")]
    [Column(TypeName = "datetime")]
    public DateTime DateTime { get; set; }

    [Required(ErrorMessage = "La ubicación del evento es obligatoria.")]
    [StringLength(200)]
    public string Location { get; set; } = null!;

    [Required(ErrorMessage = "La descripción del evento es obligatoria.")]
    [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
    public string Description { get; set; } = null!;

    [Range(5, 480, ErrorMessage = "Duración Must be between 5 to 480")]
    public int DurationMinutes { get; set; }

    [StringLength(500)]
    public string? SpecialRequirements { get; set; }

    [Required(ErrorMessage = "Los voluntarios requeridos son obligatorios.")]
    [Range(1, int.MaxValue, ErrorMessage = "Se requiere al menos un voluntario.")]
    public int VolunteersRequired { get; set; }

    [Required(ErrorMessage = "El contacto del organizador es obligatorio.")]
    [StringLength(100)]
    public string OrganizerContact { get; set; } = null!;

    [InverseProperty("Event")]
    public virtual ICollection<EventVolunteer> EventVolunteers { get; set; } = new List<EventVolunteer>();
}
