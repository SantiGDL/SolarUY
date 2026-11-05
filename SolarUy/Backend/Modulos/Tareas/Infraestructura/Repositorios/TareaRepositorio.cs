using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Tareas.Dominio.Entidades;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Infraestructura.Repositorios;

public sealed class TareaRepositorio(
    IDbContextFactory<ParquesDbContext> contextFactory,
    ILogger<TareaRepositorio> logger) : ITareaRepositorio
{
    public async Task<IReadOnlyList<TareaMantenimiento>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Tareas
                .AsNoTracking()
                .OrderBy(tarea => tarea.Estado)
                .ThenByDescending(tarea => tarea.FechaCreada)
                .ToArrayAsync(cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta de tareas de mantenimiento.");
            throw new TareasPersistenciaException("No se pudo consultar la lista de tareas.", exception);
        }
    }

    public async Task<TareaMantenimiento?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Tareas
                .AsNoTracking()
                .SingleOrDefaultAsync(tarea => tarea.Id == id, cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta de la tarea {TareaId}.", id);
            throw new TareasPersistenciaException("No se pudo consultar la tarea.", exception);
        }
    }

    public async Task<bool> ExistePanelAsync(
        int panelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Paneles
                .AsNoTracking()
                .AnyAsync(panel => panel.Id == panelId, cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la validación del panel {PanelId}.", panelId);
            throw new TareasPersistenciaException(
                "No se pudo validar el panel asociado.",
                exception);
        }
    }

    public async Task<bool> ExisteActivaAsync(
        int panelId,
        TipoMantenimiento tipoTarea,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Tareas.AnyAsync(
                tarea => tarea.PanelId == panelId
                    && tarea.TipoTarea == tipoTarea
                    && tarea.Estado != EstadoTarea.Completada,
                cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(
                exception,
                "Falló la comprobación de tareas activas para el panel {PanelId}.",
                panelId);
            throw new TareasPersistenciaException(
                "No se pudo verificar si el panel ya tiene una tarea activa.",
                exception);
        }
    }

    public async Task AgregarAsync(
        TareaMantenimiento tarea,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.Tareas.Add(tarea);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.GetBaseException() is MySqlException { Number: 1452 })
        {
            throw new TareasPersistenciaException(
                $"No existe el panel con identificador {tarea.PanelId}.",
                exception);
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Falló el registro de la tarea del panel {PanelId}.", tarea.PanelId);
            throw new TareasPersistenciaException("No se pudo guardar la tarea.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló el registro de la tarea del panel {PanelId}.", tarea.PanelId);
            throw new TareasPersistenciaException("No se pudo guardar la tarea.", exception);
        }
    }

    public async Task ActualizarAsync(
        TareaMantenimiento tarea,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.Attach(tarea);
            context.Entry(tarea).State = EntityState.Modified;
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Falló la actualización de la tarea {TareaId}.", tarea.Id);
            throw new TareasPersistenciaException("No se pudo actualizar la tarea.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la actualización de la tarea {TareaId}.", tarea.Id);
            throw new TareasPersistenciaException("No se pudo actualizar la tarea.", exception);
        }
    }
}
