using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Tareas.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Tareas.Interfaz.Apis.Local.Implementaciones;

public sealed class TareasLocalService(ITareasService tareas) : ITareasLocales
{
    public async Task<IReadOnlyList<TareaConsultaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        var resultado = await tareas.ObtenerTodasAsync(cancellationToken);
        return resultado.Select(t => new TareaConsultaDto(t.Id, t.PanelId, t.TipoTarea.ToString(),
            t.FechaCreada, t.FechaFinalizado, t.Estado.ToString(), t.Observaciones, t.FechaRealizada)).ToArray();
    }

    public async Task CrearAutomaticaAsync(int panelId, string estadoPanel, CancellationToken cancellationToken = default) =>
        await tareas.CrearAutomaticaAsync(panelId, estadoPanel, cancellationToken);
}
