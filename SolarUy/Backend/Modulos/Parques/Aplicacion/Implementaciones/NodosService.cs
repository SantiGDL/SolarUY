using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;

public sealed class NodosService(
    INodoRepositorio repositorio,
    IUsuarioActual usuarioActual) : INodosService
{
    public async Task<IReadOnlyDictionary<int, IReadOnlyList<NodoDto>>>
        ObtenerTodosAgrupadosPorParqueAsync(CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EstaAutenticadoAsync())
        {
            throw new AccesoNoAutorizadoException(
                "Debes iniciar sesión para consultar los nodos.");
        }

        var nodos = await repositorio.ListarTodosAsync(cancellationToken);
        return nodos
            .Select(Convertir)
            .GroupBy(nodo => nodo.ParqueId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (IReadOnlyList<NodoDto>)grupo.ToArray());
    }

    public async Task<IReadOnlyList<NodoDto>> ObtenerPorParqueAsync(
        int parqueId,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EstaAutenticadoAsync())
        {
            throw new AccesoNoAutorizadoException(
                "Debes iniciar sesión para consultar los nodos.");
        }

        await ValidarParqueAsync(parqueId, cancellationToken);
        var nodos = await repositorio.ListarPorParqueAsync(parqueId, cancellationToken);
        return nodos.Select(Convertir).ToArray();
    }

    public async Task<NodoDto> RegistrarAsync(
        int parqueId,
        CrearNodoDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EsAdministradorAsync())
        {
            throw new AccesoNoAutorizadoException(
                "Solo un usuario ADMIN puede registrar nodos.");
        }

        ArgumentNullException.ThrowIfNull(solicitud);
        await ValidarParqueAsync(parqueId, cancellationToken);

        var nodo = Nodo.Crear(
            parqueId,
            solicitud.Nombre,
            solicitud.UbicacionDentroParque);
        await repositorio.AgregarAsync(nodo, cancellationToken);
        return Convertir(nodo);
    }

    private async Task ValidarParqueAsync(
        int parqueId,
        CancellationToken cancellationToken)
    {
        if (parqueId <= 0
            || !await repositorio.ExisteParqueAsync(parqueId, cancellationToken))
        {
            throw new ParqueNoEncontradoException(
                $"No existe el parque con identificador {parqueId}.");
        }
    }

    private static NodoDto Convertir(Nodo nodo) =>
        new(nodo.Id, nodo.ParqueId, nodo.Nombre, nodo.UbicacionDentroParque);
}
