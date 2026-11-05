namespace SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

public enum PeriodoHistorial { Todos, Ultimas24Horas, Ultimos30Dias, Ultimos365Dias }
public enum ModoHistorial { Diario, Promedio, Crudos }

public sealed class ConsultaHistorial
{
    public int ParqueId { get; }
    public int? NodoId { get; }
    public int? PanelId { get; }
    public PeriodoHistorial Periodo { get; }
    public ModoHistorial Modo { get; }
    public DateTimeOffset? Desde { get; }
    public DateTimeOffset Hasta { get; }

    public ConsultaHistorial(int parqueId, int? nodoId, int? panelId,
        PeriodoHistorial periodo, ModoHistorial modo, DateTimeOffset referencia)
    {
        if (parqueId <= 0 || nodoId is <= 0 || panelId is <= 0 || (panelId.HasValue && !nodoId.HasValue))
            throw new ArgumentException("Seleccioná un parque, nodo y panel compatibles.");
        if (!Enum.IsDefined(periodo) || !Enum.IsDefined(modo) || referencia == default)
            throw new ArgumentException("Período, modo o fecha de consulta inválidos.");
        ParqueId = parqueId;
        NodoId = nodoId;
        PanelId = panelId;
        Periodo = periodo;
        Modo = modo;
        Hasta = referencia.ToUniversalTime();
        Desde = periodo switch
        {
            PeriodoHistorial.Ultimas24Horas => Hasta.AddHours(-24),
            PeriodoHistorial.Ultimos30Dias => Hasta.AddDays(-30),
            PeriodoHistorial.Ultimos365Dias => Hasta.AddDays(-365),
            _ => null
        };
    }
}

public sealed record AsociacionHistorial(int ParqueId, int NodoId, int PanelId);

public sealed record FilaHistorial(int? Id, int ParqueId, int NodoId, int PanelId,
    DateTimeOffset PrimeraFecha, DateTimeOffset UltimaFecha, DateTime? Dia,
    double Valor, long Cantidad);

public static class ResumenHistorial
{
    // Uruguay utiliza UTC-03:00. La misma definición alimenta el agrupamiento SQL.
    public static TimeSpan DesfaseUruguay => TimeSpan.FromHours(-3);
    public static DateTime DiaUruguay(DateTimeOffset fecha) => fecha.ToOffset(DesfaseUruguay).Date;

    public static double Promedio(double suma, long cantidad)
    {
        if (cantidad <= 0 || !double.IsFinite(suma) || suma < 0)
            throw new ArgumentException("El promedio requiere mediciones válidas.");
        return suma / cantidad;
    }
}
