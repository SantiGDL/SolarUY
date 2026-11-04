using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sensor.Contratos.Mediciones;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Components.Servicios;

namespace SolarUy.Backend.Modulos.Mediciones.Interfaz.SignalR;

[Authorize(Roles = "ADMIN")]
public sealed class MedicionesHub(MedicionesService servicio, ILogger<MedicionesHub> logger) : Hub
{
    public Task Latido(Guid sesionId) => EjecutarAsync(() => servicio.LatidoAsync(
        AccesoProduccionActual.Desde(Context.User!), sesionId, Context.ConnectionId, Context.ConnectionAborted));
    public Task RegistrarSensor(Guid sesionId) => EjecutarAsync(() => servicio.RegistrarSensorAsync(
        AccesoProduccionActual.Desde(Context.User!), sesionId, Context.ConnectionId, Context.ConnectionAborted));
    public Task ConfirmarConfiguracion(Guid sesionId, long version, PanelMotorDto[] paneles) => EjecutarAsync(() =>
        servicio.ConfirmarAsync(AccesoProduccionActual.Desde(Context.User!), sesionId, Context.ConnectionId,
            version, paneles.Select(p => new PanelMotor(p.Id, p.PotenciaActual, p.Deshabilitado, p.RevisionEstado)).ToArray(),
            Context.ConnectionAborted));
    public Task RegistrarEntrega(EntregaMotorDto entrega) => EjecutarAsync(() => servicio.RegistrarEntregaAsync(
        AccesoProduccionActual.Desde(Context.User!), Context.ConnectionId,
        new(entrega.SesionId, entrega.EntregaId, entrega.Version, entrega.ParqueId, entrega.NodoId,
            entrega.Lecturas.Select(l => new LecturaMotor(l.PanelId, l.RevisionEstado, l.ValorGenerado, l.FechaHora)).ToArray()),
        Context.ConnectionAborted));
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await servicio.DesconectarAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    private async Task EjecutarAsync(Func<Task> operacion)
    {
        try { await operacion(); }
        catch (ArgumentException ex) { throw new HubException(ex.Message); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló una operación de mediciones.");
            throw new HubException("No se pudo completar la operación de Producción.");
        }
    }
}
