using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionVoluntariadoEventosAPI.Models;

public partial class EventVolunteer
{
    [Key]
    public int Id { get; set; }

    public int EventId { get; set; }

    public int VolunteerId { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventVolunteers")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("VolunteerId")]
    [InverseProperty("EventVolunteers")]
    public virtual Volunteer Volunteer { get; set; } = null!;
}
