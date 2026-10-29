using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;

public interface IPanelRepositorio
{
    Task<IReadOnlyList<Panel>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Panel?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNodoAsync(
        int nodoId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(Panel panel, CancellationToken cancellationToken = default);

    Task ActualizarAsync(Panel panel, CancellationToken cancellationToken = default);

    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
