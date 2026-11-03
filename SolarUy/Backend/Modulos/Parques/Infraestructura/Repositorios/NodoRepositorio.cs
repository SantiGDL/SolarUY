using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Parques.Infraestructura.Repositorios;

public sealed class NodoRepositorio(
    IDbContextFactory<ParquesDbContext> contextFactory,
    ILogger<NodoRepositorio> logger) : INodoRepositorio
{
    public async Task<IReadOnlyList<Nodo>> ListarTodosAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Nodos
                .AsNoTracking()
                .OrderBy(nodo => nodo.ParqueId)
                .ThenBy(nodo => nodo.Nombre)
                .ToArrayAsync(cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta de todos los nodos.");
            throw new NodosPersistenciaException(
                "No se pudo consultar la lista de nodos.",
                exception);
        }
    }

    public async Task<IReadOnlyList<Nodo>> ListarPorParqueAsync(
        int parqueId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Nodos
                .AsNoTracking()
                .Where(nodo => nodo.ParqueId == parqueId)
                .OrderBy(nodo => nodo.Nombre)
                .ToArrayAsync(cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta de nodos del parque {ParqueId}.", parqueId);
            throw new NodosPersistenciaException(
                "No se pudo consultar la lista de nodos.",
                exception);
        }
    }

    public async Task<bool> ExisteParqueAsync(
        int parqueId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Parques
                .AnyAsync(parque => parque.Id == parqueId, cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la validación del parque {ParqueId}.", parqueId);
            throw new NodosPersistenciaException(
                "No se pudo validar el parque asociado.",
                exception);
        }
    }

    public async Task AgregarAsync(
        Nodo nodo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.Nodos.Add(nodo);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.GetBaseException() is MySqlException { Number: 1452 })
        {
            throw new ParqueNoEncontradoException(
                $"No existe el parque con identificador {nodo.ParqueId}.");
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(
                exception,
                "Falló el registro de un nodo para el parque {ParqueId}.",
                nodo.ParqueId);
            throw new NodosPersistenciaException("No se pudo guardar el nodo.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(
                exception,
                "Falló el registro de un nodo para el parque {ParqueId}.",
                nodo.ParqueId);
            throw new NodosPersistenciaException("No se pudo guardar el nodo.", exception);
        }
    }
}
