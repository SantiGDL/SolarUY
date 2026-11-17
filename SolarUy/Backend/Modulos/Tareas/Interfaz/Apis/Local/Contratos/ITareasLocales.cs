namespace SolarUy.Backend.Modulos.Tareas.Interfaz.Apis.Local.Contratos;

public interface ITareasLocales
{
    Task<IReadOnlyList<TareaConsultaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task CrearAutomaticaAsync(int panelId, string estadoPanel, CancellationToken cancellationToken = default);
}

public sealed record TareaConsultaDto(int Id, int PanelId, string TipoTarea,
    DateTime FechaCreada, DateTime? FechaFinalizado, string Estado,
    string Observaciones, DateTime? FechaRealizada);
