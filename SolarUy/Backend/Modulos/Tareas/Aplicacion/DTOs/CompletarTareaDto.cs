using System.ComponentModel.DataAnnotations;

namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;

public sealed class CompletarTareaDto
{
    [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar 1000 caracteres.")]
    public string Observaciones { get; set; } = string.Empty;
}
