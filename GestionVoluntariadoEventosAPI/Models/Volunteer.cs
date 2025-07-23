using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionVoluntariadoEventosAPI.Models;

[Index("PhoneNumber", Name = "UQ__Voluntee__85FB4E380ED1EC68", IsUnique = true)]
[Index("Email", Name = "UQ__Voluntee__A9D10534021288D7", IsUnique = true)]
public partial class Volunteer
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public int? Age { get; set; }

    [StringLength(500)]
    public string Skills { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(13)]
    public string PhoneNumber { get; set; } = null!;

    [InverseProperty("Volunteer")]
    public virtual ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();

    [InverseProperty("Volunteer")]
    public virtual ICollection<EventVolunteer> EventVolunteers { get; set; } = new List<EventVolunteer>();
}
