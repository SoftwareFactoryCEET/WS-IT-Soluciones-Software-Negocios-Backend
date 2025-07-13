using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionVoluntariadoEventosAPI.Models;

public partial class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    //[RegularExpression(@"^\+57[3][0-9]{9}$", ErrorMessage = "El número debe tener el formato +57XXXXXXXXXX.")]
    [StringLength(13)]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50)]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100)]
    //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$", ErrorMessage = "La contraseña debe tener al menos una letra mayúscula, una minúscula, un número y un carácter especial.")]
    public string Password { get; set; } = null!;
    public bool TermsAccepted { get; set; }
}
