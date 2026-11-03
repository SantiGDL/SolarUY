using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sensor.Contratos.Mediciones;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Mediciones.Interfaz.SignalR;

[Authorize(Roles = "ADMIN")]
public sealed class MedicionesHub(MedicionesService servicio, ILogger<MedicionesHub> logger) : Hub
{
    private bool EsAdministrador => Context.User?.Identity?.IsAuthenticated == true && Context.User.IsInRole("ADMIN");

    public Task<ParqueMedicionesDto[]> ObtenerCatalogo() => EjecutarAsync(async () =>
        (await servicio.ObtenerCatalogoAsync(EsAdministrador, Context.ConnectionAborted))
            .Select(p => new ParqueMedicionesDto(p.Id, p.Nombre, p.Nodos.Select(Convertir).ToArray())).ToArray());

    public Task<NodoMedicionesDto> ValidarSeleccion(SeleccionMedicionesDto seleccion) => EjecutarAsync(async () =>
        Convertir(await servicio.ValidarSeleccionAsync(EsAdministrador, seleccion.ParqueId,
            seleccion.NodoId, seleccion.PanelId, Context.ConnectionAborted)));

    public Task<bool> RegistrarEntrega(EntregaMedicionesDto entrega) => EjecutarAsync(async () =>
    {
        if (entrega?.Lecturas is null || entrega.Lecturas.Any(l => l is null))
            throw new ArgumentException("La entrega de mediciones no es válida.");
        await servicio.RegistrarEntregaAsync(EsAdministrador, new EntregaMediciones(entrega.ParqueId, entrega.NodoId,
            entrega.Lecturas.Select(l => new LecturaMedicion(l.ParqueId, l.NodoId, l.PanelId,
                l.ValorGenerado, l.FechaHora)).ToArray()), Context.ConnectionAborted);
        return true;
    });

    private async Task<T> EjecutarAsync<T>(Func<Task<T>> operacion)
    {
        try { return await operacion(); }
        catch (ArgumentException ex) { throw new HubException(ex.Message); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló una operación de mediciones.");
            throw new HubException("No se pudo completar la operación. Verificá la conexión y volvé a intentar.");
        }
    }

    private static NodoMedicionesDto Convertir(NodoProduccion n) => new(n.Id, n.Nombre, n.Ubicacion,
        n.Paneles.Select(p => new PanelMedicionesDto(p.Id, p.PotenciaActual, p.Orientacion, p.Estado)).ToArray());
}
