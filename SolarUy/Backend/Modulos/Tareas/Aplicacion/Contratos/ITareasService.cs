using SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;

public interface ITareasService
{
    Task<IReadOnlyList<TareaDto>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default);

    Task<TareaDto> CrearAsync(
        CrearTareaDto solicitud,
        CancellationToken cancellationToken = default);

    Task<TareaDto?> CrearAutomaticaAsync(
        int panelId,
        string estadoPanel,
        CancellationToken cancellationToken = default);

    Task<TareaDto> CompletarAsync(
        int id,
        CompletarTareaDto solicitud,
        CancellationToken cancellationToken = default);
}
