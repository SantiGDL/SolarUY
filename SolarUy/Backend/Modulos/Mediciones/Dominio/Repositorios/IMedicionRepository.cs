using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;

public interface IMedicionRepository
{
    Task AgregarEntregaAsync(IReadOnlyList<Medicion> mediciones, CancellationToken cancellationToken);
    Task<IReadOnlyList<SolarUy.Backend.Modulos.Mediciones.Dominio.Historial.AsociacionHistorial>> ListarAsociacionesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SolarUy.Backend.Modulos.Mediciones.Dominio.Historial.FilaHistorial>> ConsultarAsync(
        SolarUy.Backend.Modulos.Mediciones.Dominio.Historial.ConsultaHistorial consulta, CancellationToken cancellationToken);
}
