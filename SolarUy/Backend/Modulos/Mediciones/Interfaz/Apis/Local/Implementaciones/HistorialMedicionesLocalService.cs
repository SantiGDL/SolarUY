using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Mediciones.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Mediciones.Interfaz.Apis.Local.Implementaciones;

public sealed class HistorialMedicionesLocalService(HistorialMedicionesService historial) : IHistorialMediciones
{
    public Task<IReadOnlyList<OpcionHistorialDto>> ObtenerOpcionesAsync(CancellationToken ct) =>
        historial.ObtenerOpcionesAsync(ct);

    public async Task<ResultadoHistorialLocalDto> ConsultarAsync(int parqueId, int? nodoId, int? panelId,
        string periodo, string modo, CancellationToken ct)
    {
        var resultado = await historial.ConsultarAsync(parqueId, nodoId, panelId,
            Enum.Parse<PeriodoHistorial>(periodo), Enum.Parse<ModoHistorial>(modo), ct);
        var consulta = resultado.Consulta;
        return new(new(consulta.ParqueId, consulta.NodoId, consulta.PanelId,
            consulta.Periodo.ToString(), consulta.Modo.ToString(), consulta.Desde, consulta.Hasta),
            resultado.Seleccion, resultado.Periodo, resultado.Filas);
    }

    public Task<byte[]> ExportarAsync(ResultadoHistorialLocalDto resultado)
    {
        var consulta = resultado.Consulta;
        // Reconstruir con el instante original conserva la ventana móvil del resultado.
        var original = new ConsultaHistorial(consulta.ParqueId, consulta.NodoId, consulta.PanelId,
            Enum.Parse<PeriodoHistorial>(consulta.Periodo), Enum.Parse<ModoHistorial>(consulta.Modo),
            consulta.Hasta);
        return historial.ExportarAsync(new ResultadoHistorialDto(original, resultado.Seleccion, resultado.Filas));
    }
}
