using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;

public interface INodosService
{
    Task<IReadOnlyDictionary<int, IReadOnlyList<NodoDto>>> ObtenerTodosAgrupadosPorParqueAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NodoDto>> ObtenerPorParqueAsync(
        int parqueId,
        CancellationToken cancellationToken = default);

    Task<NodoDto> RegistrarAsync(
        int parqueId,
        CrearNodoDto solicitud,
        CancellationToken cancellationToken = default);
}
