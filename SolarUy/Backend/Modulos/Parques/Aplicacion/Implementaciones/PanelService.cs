using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;

public sealed class PanelService(
    IPanelRepositorio repositorio,
    IUsuarioActual usuarioActual,
    ITareasService tareasService) : IPanelesService
{
    public async Task<IReadOnlyList<PanelDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EstaAutenticadoAsync())
        {
            throw new AccesoNoAutorizadoException("Debes iniciar sesión para consultar los paneles.");
        }

        var paneles = await repositorio.ListarAsync(cancellationToken);
        return paneles.Select(Convertir).ToArray();
    }

    public async Task<PanelDto> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EstaAutenticadoAsync())
        {
            throw new AccesoNoAutorizadoException("Debes iniciar sesión para consultar los paneles.");
        }

        if (id <= 0)
        {
            throw new PanelNoEncontradoException($"No existe el panel con identificador {id}.");
        }

        var panel = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new PanelNoEncontradoException($"No existe el panel con identificador {id}.");

        return Convertir(panel);
    }

    public async Task<PanelDto> RegistrarAsync(
        CrearPanelDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EsAdministradorAsync())
        {
            throw new AccesoNoAutorizadoException("Solo un usuario ADMIN puede registrar paneles.");
        }

        ArgumentNullException.ThrowIfNull(solicitud);

        var panel = Panel.Crear(
            solicitud.NodoId,
            solicitud.Estado,
            solicitud.PotenciaActual,
            solicitud.Orientacion);

        if (!await repositorio.ExisteNodoAsync(panel.NodoId, cancellationToken))
        {
            throw new NodoNoEncontradoException(
                $"No existe el nodo con identificador {panel.NodoId}.");
        }

        await repositorio.AgregarAsync(panel, cancellationToken);
        if (panel.Estado != EstadoPanel.Bueno)
        {
            await tareasService.CrearAutomaticaAsync(
                panel.Id,
                panel.Estado,
                cancellationToken);
        }

        return Convertir(panel);
    }

    public async Task<PanelDto> ActualizarAsync(
        int id,
        CrearPanelDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EsAdministradorAsync())
        {
            throw new AccesoNoAutorizadoException("Solo un usuario ADMIN puede modificar paneles.");
        }

        ArgumentNullException.ThrowIfNull(solicitud);

        var panelActual = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new PanelNoEncontradoException($"No existe el panel con identificador {id}.");

        if (!await repositorio.ExisteNodoAsync(solicitud.NodoId, cancellationToken))
        {
            throw new NodoNoEncontradoException(
                $"No existe el nodo con identificador {solicitud.NodoId}.");
        }

        var estadoAnterior = panelActual.Estado;
        panelActual.Actualizar(
            solicitud.NodoId,
            solicitud.Estado,
            solicitud.PotenciaActual,
            solicitud.Orientacion);

        await repositorio.ActualizarAsync(panelActual, cancellationToken);
        if (estadoAnterior != panelActual.Estado
            && panelActual.Estado != EstadoPanel.Bueno)
        {
            await tareasService.CrearAutomaticaAsync(
                panelActual.Id,
                panelActual.Estado,
                cancellationToken);
        }

        return Convertir(panelActual);
    }

    public async Task EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EsAdministradorAsync())
        {
            throw new AccesoNoAutorizadoException("Solo un usuario ADMIN puede eliminar paneles.");
        }

        if (id <= 0)
        {
            throw new PanelNoEncontradoException($"No existe el panel con identificador {id}.");
        }

        var existe = await repositorio.ObtenerPorIdAsync(id, cancellationToken) is not null;
        if (!existe)
        {
            throw new PanelNoEncontradoException($"No existe el panel con identificador {id}.");
        }

        await repositorio.EliminarAsync(id, cancellationToken);
    }

    private static PanelDto Convertir(Panel panel) =>
        new(panel.Id, panel.NodoId, panel.Estado, panel.PotenciaActual, panel.Orientacion);
}
