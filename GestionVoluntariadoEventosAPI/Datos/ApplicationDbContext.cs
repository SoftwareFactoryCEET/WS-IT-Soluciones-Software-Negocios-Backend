using System;
using System.Collections.Generic;
using GestionVoluntariadoEventosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionVoluntariadoEventosAPI.Datos;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventVolunteer> EventVolunteers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AvailabilitySlot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Availabi__3214EC07275ABF93");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.AvailabilitySlots)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Availabil__Volun__3A81B327");
        });

        modelBuilder.Entity<EventVolunteer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EventVol__3214EC07F17CE1AE");

            entity.HasOne(d => d.Event).WithMany(p => p.EventVolunteers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventVolu__Event__3F466844");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.EventVolunteers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventVolu__Volun__403A8C7D");
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voluntee__3214EC079440398D");

            entity.Property(e => e.Age).HasComputedColumnSql("(datediff(year,[BirthDate],getdate()))", false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
