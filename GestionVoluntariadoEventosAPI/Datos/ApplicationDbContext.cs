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
    // Agrega tus DbSet aquí (tablas), es decir las clases modelos
    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }


    // Configuraciones adicionales si las necesitas
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
        // Ejemplo de configuración personalizada con Fluent API


        // Puedes agregar relaciones, restricciones, índices, etc.
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
