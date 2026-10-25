using System.ComponentModel.DataAnnotations;
namespace SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
public sealed class CrearParqueDto
{
    [Required(ErrorMessage = "Ingresá el nombre del parque.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Ingresá la dirección del parque.")]
    [StringLength(250, ErrorMessage = "La dirección admite hasta 250 caracteres.")]
    public string Direccion { get; set; } = "";
}
