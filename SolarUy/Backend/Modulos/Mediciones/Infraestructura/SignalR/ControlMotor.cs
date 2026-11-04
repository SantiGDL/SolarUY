using Microsoft.AspNetCore.SignalR;
using Sensor.Contratos.Mediciones;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Interfaz.SignalR;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.SignalR;

public sealed class ControlMotor(IHubContext<MedicionesHub> hub) : IControlMotor
{
    public Task ConfigurarAsync(string conexion, ConfiguracionMotor c, CancellationToken ct) =>
        hub.Clients.Client(conexion).SendAsync("Configurar", new ConfiguracionMotorDto(c.SesionId,
            c.Version, c.ParqueId, c.NodoId, c.Paneles.Select(p => new PanelMotorDto(p.Id, p.PotenciaActual,
                p.Deshabilitado, p.RevisionEstado)).ToArray()), ct);
    public Task CambiarHabilitacionAsync(string conexion, EstadoPanelMotor e, CancellationToken ct) =>
        hub.Clients.Client(conexion).SendAsync("CambiarHabilitacionPanel", new EstadoPanelMotorDto(e.SesionId,
            e.Version, e.PanelId, e.Deshabilitado, e.RevisionEstado), ct);
    public Task DetenerAsync(string conexion, Guid sesionId, CancellationToken ct) =>
        hub.Clients.Client(conexion).SendAsync("Detener", sesionId, ct);
}
