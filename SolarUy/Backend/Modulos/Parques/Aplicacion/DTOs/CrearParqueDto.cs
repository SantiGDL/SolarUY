using System.ComponentModel.DataAnnotations;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

public sealed class CrearParqueDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres.")]
    public string Direccion { get; set; } = string.Empty;
}
