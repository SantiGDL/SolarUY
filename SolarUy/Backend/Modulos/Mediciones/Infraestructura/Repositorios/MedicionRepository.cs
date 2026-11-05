using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Mediciones.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Excepciones;
using System.Data.Common;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Repositorios;

public sealed class MedicionRepository(IDbContextFactory<MedicionesDbContext> factory) : IMedicionRepository
{
    public async Task<IReadOnlyList<AsociacionHistorial>> ListarAsociacionesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync(cancellationToken);
            return await context.Mediciones.AsNoTracking()
                .Select(m => new AsociacionHistorial(m.ParqueId, m.NodoId, m.PanelId))
                .Distinct().ToArrayAsync(cancellationToken);
        }
        catch (DbException ex)
        {
            throw new HistorialPersistenciaException("No se pudo cargar el catálogo histórico.", ex);
        }
    }

    public async Task<IReadOnlyList<FilaHistorial>> ConsultarAsync(ConsultaHistorial consulta, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync(cancellationToken);
            if (consulta.Modo == ModoHistorial.Crudos)
            {
                var query = context.Mediciones.AsNoTracking().Where(m => m.ParqueId == consulta.ParqueId && m.FechaHora <= consulta.Hasta);
                if (consulta.Desde.HasValue) query = query.Where(m => m.FechaHora >= consulta.Desde.Value);
                if (consulta.NodoId.HasValue) query = query.Where(m => m.NodoId == consulta.NodoId.Value);
                if (consulta.PanelId.HasValue) query = query.Where(m => m.PanelId == consulta.PanelId.Value);
                var filas = await query.OrderBy(m => m.FechaHora).ThenBy(m => m.Id).ToArrayAsync(cancellationToken);
                return filas.Select(m => new FilaHistorial(m.Id, m.ParqueId, m.NodoId, m.PanelId,
                    m.FechaHora, m.FechaHora, null, m.ValorGenerado, 1)).ToArray();
            }

            // SQL parametrizado: MySQL agrupa antes de transferir los resultados al servidor.
            // No se depende de la traducción de DateTimeOffset del proveedor para las fechas diarias.
            await context.Database.OpenConnectionAsync(cancellationToken);
            await using var command = context.Database.GetDbConnection().CreateCommand();
            var diario = consulta.Modo == ModoHistorial.Diario;
            var diaSql = "DATE(TIMESTAMPADD(MINUTE, @desfase, FechaHora))";
            command.CommandText = "SELECT ParqueId, NodoId, PanelId, MIN(FechaHora), MAX(FechaHora), " +
                "SUM(ValorGenerado), COUNT(*), " + (diario ? diaSql : "NULL") +
                " FROM Mediciones WHERE ParqueId = @parque AND FechaHora <= @hasta" +
                (consulta.Desde.HasValue ? " AND FechaHora >= @desde" : "") +
                (consulta.NodoId.HasValue ? " AND NodoId = @nodo" : "") +
                (consulta.PanelId.HasValue ? " AND PanelId = @panel" : "") +
                " GROUP BY ParqueId, NodoId, PanelId" + (diario ? ", " + diaSql : "") +
                (diario ? " ORDER BY 8, ParqueId, NodoId, PanelId" : " ORDER BY ParqueId, NodoId, PanelId");
            AgregarParametro(command, "@parque", consulta.ParqueId);
            AgregarParametro(command, "@hasta", consulta.Hasta.UtcDateTime);
            if (diario) AgregarParametro(command, "@desfase", (int)ResumenHistorial.DesfaseUruguay.TotalMinutes);
            if (consulta.Desde.HasValue) AgregarParametro(command, "@desde", consulta.Desde.Value.UtcDateTime);
            if (consulta.NodoId.HasValue) AgregarParametro(command, "@nodo", consulta.NodoId.Value);
            if (consulta.PanelId.HasValue) AgregarParametro(command, "@panel", consulta.PanelId.Value);
            var resumen = new List<FilaHistorial>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var cantidad = reader.GetInt64(6);
                resumen.Add(new(null, reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2),
                    FechaUtc(reader.GetDateTime(3)), FechaUtc(reader.GetDateTime(4)),
                    diario ? reader.GetDateTime(7) : null, ResumenHistorial.Promedio(reader.GetDouble(5), cantidad), cantidad));
            }
            return resumen;
        }
        catch (DbException ex)
        {
            throw new HistorialPersistenciaException("No se pudieron consultar las mediciones. Intentá nuevamente.", ex);
        }
    }

    private static DateTimeOffset FechaUtc(DateTime fecha) => new(DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

    private static void AgregarParametro(DbCommand command, string nombre, object valor)
    {
        var parametro = command.CreateParameter();
        parametro.ParameterName = nombre;
        parametro.Value = valor;
        command.Parameters.Add(parametro);
    }

    public async Task AgregarEntregaAsync(IReadOnlyList<Medicion> mediciones, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        context.Mediciones.AddRange(mediciones);
        await context.SaveChangesAsync(cancellationToken);
    }
}
