using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;

public interface IPanelesService
{
    Task<IReadOnlyList<PanelDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<PanelDto> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PanelDto> RegistrarAsync(
        CrearPanelDto solicitud,
        CancellationToken cancellationToken = default);

    Task<PanelDto> ActualizarAsync(
        int id,
        CrearPanelDto solicitud,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        int id,
        CancellationToken cancellationToken = default);
}
