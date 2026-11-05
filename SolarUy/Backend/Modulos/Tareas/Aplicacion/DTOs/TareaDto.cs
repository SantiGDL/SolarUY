using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;

public sealed record TareaDto(
    int Id,
    int PanelId,
    TipoMantenimiento TipoTarea,
    DateTime FechaCreada,
    DateTime? FechaFinalizado,
    EstadoTarea Estado,
    string Observaciones,
    DateTime? FechaRealizada);
