using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;

public interface IParquesService
{
    Task<IReadOnlyList<ParqueDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<ParqueDto> RegistrarAsync(
        CrearParqueDto solicitud,
        CancellationToken cancellationToken = default);
}
