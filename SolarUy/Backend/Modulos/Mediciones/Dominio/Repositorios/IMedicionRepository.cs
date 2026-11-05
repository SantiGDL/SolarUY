using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;

public interface IMedicionRepository
{
    Task AgregarEntregaAsync(IReadOnlyList<Medicion> mediciones, CancellationToken cancellationToken);
}
