using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;

namespace SolarUy.Backend.Modulos.Parques.Infraestructura.Repositorios;

public sealed class ParqueRepositorio(
    IDbContextFactory<ParquesDbContext> contextFactory,
    ILogger<ParqueRepositorio> logger) : IParqueRepositorio
{
    public async Task<IReadOnlyList<Parque>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Parques
                .AsNoTracking()
                .OrderBy(parque => parque.Nombre)
                .ToArrayAsync(cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la consulta de parques en MySQL.");
            throw new ParquesPersistenciaException("No se pudo consultar la lista de parques.", exception);
        }
    }

    public async Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Parques
                .AnyAsync(
                    parque => parque.NombreNormalizado == nombreNormalizado,
                    cancellationToken);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló la búsqueda de parques en MySQL.");
            throw new ParquesPersistenciaException("No se pudo validar el nombre del parque.", exception);
        }
    }

    public async Task AgregarAsync(
        Parque parque,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Parques.Add(parque);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.GetBaseException() is MySqlException { Number: 1062 })
        {
            throw new NombreParqueDuplicadoException(
                $"Ya existe un parque con el nombre \"{parque.Nombre}\".");
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Falló el registro de un parque en MySQL.");
            throw new ParquesPersistenciaException("No se pudo guardar el parque.", exception);
        }
        catch (MySqlException exception)
        {
            logger.LogError(exception, "Falló el registro de un parque en MySQL.");
            throw new ParquesPersistenciaException("No se pudo guardar el parque.", exception);
        }
    }
}
