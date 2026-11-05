using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

public sealed record OpcionHistorialDto(int ParqueId, string Parque, int NodoId, string Nodo, int PanelId);
public sealed record FilaHistorialDto(int? Id, int ParqueId, int NodoId, int PanelId,
    DateTimeOffset PrimeraFecha, DateTimeOffset UltimaFecha, DateTime? Dia, double Valor, long Cantidad);

public sealed class ResultadoHistorialDto
{
    public ConsultaHistorial Consulta { get; }
    public string Seleccion { get; }
    public IReadOnlyList<FilaHistorialDto> Filas { get; }
    public string Periodo => Consulta.Periodo switch
    {
        PeriodoHistorial.Todos => "Todos los datos",
        PeriodoHistorial.Ultimas24Horas => "Últimas 24 horas",
        PeriodoHistorial.Ultimos30Dias => "Últimos 30 días",
        _ => "Últimos 365 días"
    };
    public string Modo => Consulta.Modo switch
    {
        ModoHistorial.Diario => "Promedios diarios por panel",
        ModoHistorial.Promedio => "Promedio del período por panel",
        _ => "Datos crudos"
    };
    public const string Aviso = "Solo mediciones registradas del simulador. Los períodos sin datos no representan cero. " +
        "Las sesiones simultáneas aportan muestras independientes. Los promedios son por panel, no la potencia total del parque.";

    internal ResultadoHistorialDto(ConsultaHistorial consulta, string seleccion, IEnumerable<FilaHistorialDto> filas)
    {
        Consulta = consulta;
        Seleccion = seleccion;
        Filas = Array.AsReadOnly(filas.ToArray());
    }

    public static string FechaLocal(DateTimeOffset fecha) =>
        fecha.ToOffset(ResumenHistorial.DesfaseUruguay).ToString("dd/MM/yyyy HH:mm:ss");
}
