using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Mediciones.Interfaz.Apis.Local.Contratos;

public interface IHistorialMediciones
{
    Task<IReadOnlyList<OpcionHistorialDto>> ObtenerOpcionesAsync(CancellationToken ct);
    Task<ResultadoHistorialLocalDto> ConsultarAsync(int parqueId, int? nodoId, int? panelId,
        string periodo, string modo, CancellationToken ct);
    Task<byte[]> ExportarAsync(ResultadoHistorialLocalDto resultado);
}

public sealed record ConsultaHistorialLocalDto(int ParqueId, int? NodoId, int? PanelId,
    string Periodo, string Modo, DateTimeOffset? Desde, DateTimeOffset Hasta);

public sealed record ResultadoHistorialLocalDto(ConsultaHistorialLocalDto Consulta,
    string Seleccion, string Periodo, IReadOnlyList<FilaHistorialDto> Filas);
