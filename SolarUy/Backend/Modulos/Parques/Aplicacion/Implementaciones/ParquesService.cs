using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;

public sealed class ParquesService(
    IParqueRepositorio repositorio,
    IUsuarioActual usuarioActual) : IParquesService
{
    public async Task<IReadOnlyList<ParqueDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EstaAutenticadoAsync())
        {
            throw new AccesoNoAutorizadoException("Debes iniciar sesión para consultar los parques.");
        }

        var parques = await repositorio.ListarAsync(cancellationToken);
        return parques.Select(Convertir).ToArray();
    }

    public async Task<ParqueDto> RegistrarAsync(
        CrearParqueDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await usuarioActual.EsAdministradorAsync())
        {
            throw new AccesoNoAutorizadoException("Solo un usuario ADMIN puede registrar parques.");
        }

        ArgumentNullException.ThrowIfNull(solicitud);

        var parque = Parque.Crear(solicitud.Nombre, solicitud.Direccion);
        if (await repositorio.ExisteNombreAsync(parque.NombreNormalizado, cancellationToken))
        {
            throw new NombreParqueDuplicadoException(
                $"Ya existe un parque con el nombre \"{parque.Nombre}\".");
        }

        await repositorio.AgregarAsync(parque, cancellationToken);
        return Convertir(parque);
    }

    private static ParqueDto Convertir(Parque parque) =>
        new(parque.Id, parque.Nombre, parque.Direccion);
}
