using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;

public interface INodoRepositorio
{
    Task<IReadOnlyList<Nodo>> ListarTodosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Nodo>> ListarPorParqueAsync(
        int parqueId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteParqueAsync(
        int parqueId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(Nodo nodo, CancellationToken cancellationToken = default);
}
