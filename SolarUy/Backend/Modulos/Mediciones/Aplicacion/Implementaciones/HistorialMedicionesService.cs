using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;

public sealed class HistorialMedicionesService(IMedicionRepository mediciones,
    ICatalogoHistorial catalogo, IAccesoHistorial acceso, IExportadorHistorialPdf exportador)
{
    private async Task AutorizarAsync()
    {
        if (!await acceso.PuedeConsultarAsync())
            throw new UnauthorizedAccessException("El histórico requiere un administrador autenticado.");
    }

    public async Task<IReadOnlyList<OpcionHistorialDto>> ObtenerOpcionesAsync(CancellationToken ct)
    {
        await AutorizarAsync();
        var actual = await catalogo.ObtenerAsync(ct);
        var asociaciones = (await mediciones.ListarAsociacionesAsync(ct)).ToHashSet();
        foreach (var panel in actual.Paneles)
        {
            var nodo = actual.Nodos.FirstOrDefault(n => n.Id == panel.NodoId);
            if (nodo is not null) asociaciones.Add(new(nodo.ParqueId, nodo.Id, panel.Id));
        }
        return asociaciones.OrderBy(a => a.ParqueId).ThenBy(a => a.NodoId).ThenBy(a => a.PanelId)
            .Select(a => new OpcionHistorialDto(a.ParqueId,
                actual.Parques.FirstOrDefault(p => p.Id == a.ParqueId)?.Nombre ?? $"Parque #{a.ParqueId} (histórico)",
                a.NodoId, actual.Nodos.FirstOrDefault(n => n.Id == a.NodoId)?.Nombre ?? $"Nodo #{a.NodoId} (histórico)",
                a.PanelId)).ToArray();
    }

    public async Task<ResultadoHistorialDto> ConsultarAsync(int parqueId, int? nodoId, int? panelId,
        PeriodoHistorial periodo, ModoHistorial modo, CancellationToken ct)
    {
        await AutorizarAsync();
        var consulta = new ConsultaHistorial(parqueId, nodoId, panelId, periodo, modo, DateTimeOffset.UtcNow);
        var opciones = await ObtenerOpcionesAsync(ct);
        var seleccion = opciones.FirstOrDefault(o => o.ParqueId == parqueId &&
            (!nodoId.HasValue || o.NodoId == nodoId) && (!panelId.HasValue || o.PanelId == panelId))
            ?? throw new ArgumentException("La selección no existe o ya no está disponible. Actualizá el catálogo.");
        var filas = await mediciones.ConsultarAsync(consulta, ct);
        var descripcion = $"{seleccion.Parque} (#{parqueId}) / " +
            (nodoId.HasValue ? $"{seleccion.Nodo} (#{nodoId})" : "Todos los nodos") + " / " +
            (panelId.HasValue ? $"Panel #{panelId}" : "Todos los paneles");
        return new(consulta, descripcion, filas.Select(f => new FilaHistorialDto(f.Id, f.ParqueId,
            f.NodoId, f.PanelId, f.PrimeraFecha, f.UltimaFecha, f.Dia, f.Valor, f.Cantidad)));
    }

    public async Task<byte[]> ExportarAsync(ResultadoHistorialDto resultado)
    {
        await AutorizarAsync();
        return exportador.Generar(resultado);
    }
}
