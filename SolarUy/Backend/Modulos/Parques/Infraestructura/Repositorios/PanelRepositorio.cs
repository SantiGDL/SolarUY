using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Parques.Infraestructura.Repositorios;

public sealed class PanelRepositorio(
    IDbContextFactory<ParquesDbContext> contextFactory,
    ILogger<PanelRepositorio> logger) : IPanelRepositorio
{
    public async Task<IReadOnlyList<Panel>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Paneles
                .AsNoTracking()
                .OrderBy(panel => panel.NodoId)
                .ThenBy(panel => panel.Orientacion)
                .ToArrayAsync(cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta de paneles.");
            throw new PanelesPersistenciaException(
                "No se pudo consultar la lista de paneles.",
                exception);
        }
    }

    public async Task<Panel?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Paneles
                .AsNoTracking()
                .SingleOrDefaultAsync(panel => panel.Id == id, cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta del panel {PanelId}.", id);
            throw new PanelesPersistenciaException(
                "No se pudo consultar el detalle del panel.",
                exception);
        }
    }

    public async Task<bool> ExisteNodoAsync(
        int nodoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Nodos
                .AsNoTracking()
                .AnyAsync(nodo => nodo.Id == nodoId, cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la validación del nodo {NodoId}.", nodoId);
            throw new PanelesPersistenciaException(
                "No se pudo validar el nodo asociado.",
                exception);
        }
    }

    public async Task AgregarAsync(
        Panel panel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.Paneles.Add(panel);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.GetBaseException() is MySqlException { Number: 1452 })
        {
            throw new NodoNoEncontradoException(
                $"No existe el nodo con identificador {panel.NodoId}.");
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(
                exception,
                "Falló el registro del panel del nodo {NodoId}.",
                panel.NodoId);
            throw new PanelesPersistenciaException("No se pudo guardar el panel.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(
                exception,
                "Falló el registro del panel del nodo {NodoId}.",
                panel.NodoId);
            throw new PanelesPersistenciaException("No se pudo guardar el panel.", exception);
        }
    }

    public async Task ActualizarAsync(
        Panel panel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.Entry(panel).State = EntityState.Modified;
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.GetBaseException() is MySqlException { Number: 1452 })
        {
            throw new NodoNoEncontradoException(
                $"No existe el nodo con identificador {panel.NodoId}.");
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(
                exception,
                "Falló la actualización del panel {PanelId}.",
                panel.Id);
            throw new PanelesPersistenciaException("No se pudo actualizar el panel.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(
                exception,
                "Falló la actualización del panel {PanelId}.",
                panel.Id);
            throw new PanelesPersistenciaException("No se pudo actualizar el panel.", exception);
        }
    }

    public async Task EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var panel = await context.Paneles
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (panel is null)
            {
                throw new PanelNoEncontradoException($"No existe el panel con identificador {id}.");
            }

            context.Paneles.Remove(panel);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (PanelNoEncontradoException)
        {
            throw;
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Falló la eliminación del panel {PanelId}.", id);
            throw new PanelesPersistenciaException("No se pudo eliminar el panel.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la eliminación del panel {PanelId}.", id);
            throw new PanelesPersistenciaException("No se pudo eliminar el panel.", exception);
        }
    }
}
