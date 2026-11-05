using SolarUy.Backend.Modulos.Tareas.Dominio.Entidades;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;

public interface ITareaRepositorio
{
    Task<IReadOnlyList<TareaMantenimiento>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<TareaMantenimiento?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistePanelAsync(int panelId, CancellationToken cancellationToken = default);

    Task<bool> ExisteActivaAsync(
        int panelId,
        TipoMantenimiento tipoTarea,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        TareaMantenimiento tarea,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        TareaMantenimiento tarea,
        CancellationToken cancellationToken = default);
}
