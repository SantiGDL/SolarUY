using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
namespace SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;

public interface IParqueRepository
{
    Task<IReadOnlyList<Parque>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default);
    Task AgregarAsync(Parque parque, CancellationToken cancellationToken = default);
}
