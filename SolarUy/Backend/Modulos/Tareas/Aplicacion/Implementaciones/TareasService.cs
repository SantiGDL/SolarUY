using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Enums;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Tareas.Dominio.Entidades;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Implementaciones;

public sealed class TareasService(
    ITareaRepositorio repositorio,
    IUsuarioActual usuarioActual) : ITareasService
{
    public async Task<IReadOnlyList<TareaDto>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default)
    {
        await ExigirAdministradorAsync();
        var todas = await repositorio.ListarAsync(cancellationToken);
        return todas.Select(Convertir).ToArray();
    }

    public async Task<TareaDto> CrearAsync(
        CrearTareaDto solicitud,
        CancellationToken cancellationToken = default)
    {
        await ExigirAdministradorAsync();
        ArgumentNullException.ThrowIfNull(solicitud);

        if (!await repositorio.ExistePanelAsync(solicitud.PanelId, cancellationToken))
        {
            throw new TareaNoEncontradaException(
                $"No existe el panel con identificador {solicitud.PanelId}.");
        }

        var tarea = TareaMantenimiento.Crear(
            solicitud.PanelId,
            solicitud.TipoTarea,
            solicitud.Observaciones,
            DateTime.UtcNow);
        await repositorio.AgregarAsync(tarea, cancellationToken);
        return Convertir(tarea);
    }

    public async Task<TareaDto?> CrearAutomaticaAsync(
        int panelId,
        EstadoPanel estadoPanel,
        CancellationToken cancellationToken = default)
    {
        await ExigirAdministradorAsync();
        var tipoTarea = estadoPanel switch
        {
            EstadoPanel.Intermedio => TipoMantenimiento.Limpieza,
            EstadoPanel.Rotura => TipoMantenimiento.Reparacion,
            _ => (TipoMantenimiento?)null
        };

        if (tipoTarea is null)
        {
            return null;
        }

        if (!await repositorio.ExistePanelAsync(panelId, cancellationToken))
        {
            throw new TareaNoEncontradaException(
                $"No existe el panel con identificador {panelId}.");
        }

        if (await repositorio.ExisteActivaAsync(panelId, tipoTarea.Value, cancellationToken))
        {
            return null;
        }

        var tarea = TareaMantenimiento.Crear(
            panelId,
            tipoTarea.Value,
            $"Generada automáticamente por el estado {estadoPanel}.",
            DateTime.UtcNow);
        try
        {
            await repositorio.AgregarAsync(tarea, cancellationToken);
        }
        catch (TareasPersistenciaException exception)
        {
            throw new TareaAutomaticaException(
                "El panel se guardó, pero no se pudo generar su tarea automática. " +
                "Verificá el panel y creá la tarea manualmente.",
                exception);
        }

        return Convertir(tarea);
    }

    public async Task<TareaDto> CompletarAsync(
        int id,
        CompletarTareaDto solicitud,
        CancellationToken cancellationToken = default)
    {
        await ExigirAdministradorAsync();
        ArgumentNullException.ThrowIfNull(solicitud);
        var tarea = await ObtenerTareaAsync(id, cancellationToken);

        try
        {
            tarea.Completar(solicitud.Observaciones, DateTime.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new TareaNoDisponibleException(exception.Message);
        }

        await repositorio.ActualizarAsync(tarea, cancellationToken);
        return Convertir(tarea);
    }

    private async Task ExigirAdministradorAsync()
    {
        if (!await usuarioActual.EsAdministradorAsync())
        {
            throw new AccesoNoAutorizadoException(
                "Solo un usuario ADMIN puede administrar tareas.");
        }
    }

    private async Task<TareaMantenimiento> ObtenerTareaAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new TareaNoEncontradaException(
                $"No existe la tarea con identificador {id}.");
        }

        return await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new TareaNoEncontradaException(
                $"No existe la tarea con identificador {id}.");
    }

    private static TareaDto Convertir(TareaMantenimiento tarea) =>
        new(
            tarea.Id,
            tarea.PanelId,
            tarea.TipoTarea,
            tarea.FechaCreada,
            tarea.FechaFinalizado,
            tarea.Estado,
            tarea.Observaciones,
            tarea.FechaRealizada);
}
