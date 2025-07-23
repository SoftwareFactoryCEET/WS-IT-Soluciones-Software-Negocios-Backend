using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionVoluntariadoEventosAPI.Models;

public partial class AvailabilitySlot
{
    [Key]
    public int Id { get; set; }

    public int VolunteerId { get; set; }

    [StringLength(20)]
    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [ForeignKey("VolunteerId")]
    [InverseProperty("AvailabilitySlots")]
    public virtual Volunteer Volunteer { get; set; } = null!;
}
