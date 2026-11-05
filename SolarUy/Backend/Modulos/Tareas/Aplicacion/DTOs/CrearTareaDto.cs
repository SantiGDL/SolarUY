using System.ComponentModel.DataAnnotations;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;

public sealed class CrearTareaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un panel válido.")]
    public int PanelId { get; set; }

    [EnumDataType(typeof(TipoMantenimiento), ErrorMessage = "Seleccioná un tipo de tarea válido.")]
    public TipoMantenimiento TipoTarea { get; set; }

    [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar 1000 caracteres.")]
    public string Observaciones { get; set; } = string.Empty;
}
