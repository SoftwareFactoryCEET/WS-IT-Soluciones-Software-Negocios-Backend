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

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime DateTime { get; set; }

    [StringLength(200)]
    public string Location { get; set; } = null!;

    [StringLength(250)]
    public string Description { get; set; } = null!;

    public int DurationMinutes { get; set; }

    [StringLength(500)]
    public string? SpecialRequirements { get; set; }

    public int VolunteersRequired { get; set; }

    [StringLength(100)]
    public string OrganizerContact { get; set; } = null!;
}
