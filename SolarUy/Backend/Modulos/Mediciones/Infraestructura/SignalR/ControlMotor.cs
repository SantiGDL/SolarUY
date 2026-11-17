using Microsoft.AspNetCore.SignalR;
using Sensor.Contratos.Mediciones;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Interfaz.SignalR;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.SignalR;

// Envía cada orden al sensor vinculado a la sesión por su ID de conexión.
public sealed class ControlMotor(IHubContext<MedicionesHub> hub) : IControlMotor
{
    public Task ConfigurarAsync(string conexion, ConfiguracionMotor configuracion, CancellationToken cancelacion)
    {
        var paneles = configuracion.Paneles.Select(panel => new PanelMotorDto(panel.Id, panel.PotenciaActual,
            panel.Deshabilitado, panel.RevisionEstado)).ToArray();
        var mensaje = new ConfiguracionMotorDto(configuracion.SesionId, configuracion.Version,
            configuracion.ParqueId, configuracion.NodoId, paneles);
        return hub.Clients.Client(conexion).SendAsync("Configurar", mensaje, cancelacion);
    }

    public Task CambiarHabilitacionAsync(string conexion, EstadoPanelMotor estado, CancellationToken cancelacion)
    {
        var mensaje = new EstadoPanelMotorDto(estado.SesionId, estado.Version,
            estado.PanelId, estado.Deshabilitado, estado.RevisionEstado);
        return hub.Clients.Client(conexion).SendAsync("CambiarHabilitacionPanel", mensaje, cancelacion);
    }

    public Task DetenerAsync(string conexion, Guid sesionId, CancellationToken ct) =>
        hub.Clients.Client(conexion).SendAsync("Detener", sesionId, ct);
}
