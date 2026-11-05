using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;

public interface ISesionesProduccion
{
    SesionProduccion Crear(string propietario);
    SesionProduccion Obtener(Guid id);
    IReadOnlyList<SesionProduccion> Listar();
    SemaphoreSlim BloqueoNodo(int nodoId);
    void Retirar(Guid id);
}

public interface IControlMotor
{
    Task ConfigurarAsync(string conexion, ConfiguracionMotor configuracion, CancellationToken ct);
    Task CambiarHabilitacionAsync(string conexion, EstadoPanelMotor estado, CancellationToken ct);
    Task DetenerAsync(string conexion, Guid sesionId, CancellationToken ct);
}
