using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Dominio.Entidades;

public sealed class TareaMantenimiento
{
    private TareaMantenimiento()
    {
    }

    public int Id { get; private set; }

    public TipoMantenimiento TipoTarea { get; private set; }

    public DateTime FechaCreada { get; private set; }

    public DateTime? FechaFinalizado { get; private set; }

    public EstadoTarea Estado { get; private set; }

    public string Observaciones { get; private set; } = string.Empty;

    public DateTime? FechaRealizada { get; private set; }

    public int PanelId { get; private set; }

    public static TareaMantenimiento Crear(
        int panelId,
        TipoMantenimiento tipoTarea,
        string? observaciones,
        DateTime fechaCreada)
    {
        var observacionesNormalizadas = (observaciones ?? string.Empty).Trim();

        if (panelId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(panelId),
                "El panel asociado debe ser válido.");
        }

        if (!Enum.IsDefined(tipoTarea))
        {
            throw new ArgumentOutOfRangeException(
                nameof(tipoTarea),
                "El tipo de mantenimiento no es válido.");
        }

        if (observacionesNormalizadas.Length > 1000)
        {
            throw new ArgumentException(
                "Las observaciones no pueden superar los 1000 caracteres.",
                nameof(observaciones));
        }

        return new TareaMantenimiento
        {
            PanelId = panelId,
            TipoTarea = tipoTarea,
            Observaciones = observacionesNormalizadas,
            FechaCreada = AsegurarUtc(fechaCreada),
            Estado = EstadoTarea.Pendiente
        };
    }

    public void Completar(string? observaciones, DateTime fechaRealizada)
    {
        if (Estado == EstadoTarea.Completada)
        {
            throw new InvalidOperationException("La tarea ya está completada.");
        }

        var observacionesNormalizadas = (observaciones ?? string.Empty).Trim();
        if (observacionesNormalizadas.Length > 1000)
        {
            throw new ArgumentException(
                "Las observaciones no pueden superar los 1000 caracteres.",
                nameof(observaciones));
        }

        var fechaUtc = AsegurarUtc(fechaRealizada);
        FechaRealizada = fechaUtc;
        FechaFinalizado = fechaUtc;
        Estado = EstadoTarea.Completada;
        if (observacionesNormalizadas.Length > 0)
        {
            Observaciones = observacionesNormalizadas;
        }
    }

    private static DateTime AsegurarUtc(DateTime fecha) =>
        fecha.Kind switch
        {
            DateTimeKind.Utc => fecha,
            DateTimeKind.Local => fecha.ToUniversalTime(),
            _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
        };
}
