using System.ComponentModel.DataAnnotations;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

public sealed class CrearNodoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación dentro del parque es obligatoria.")]
    [StringLength(
        250,
        ErrorMessage = "La ubicación dentro del parque no puede superar los 250 caracteres.")]
    public string UbicacionDentroParque { get; set; } = string.Empty;
}
