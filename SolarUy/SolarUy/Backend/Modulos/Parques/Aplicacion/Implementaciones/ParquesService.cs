using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;
namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;

public sealed class ParquesService(IParqueRepository repositorio, IUsuarioActual usuario) : IParquesService
{
    public async Task<IReadOnlyList<ParqueDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        await ExigirAutenticacionAsync();
        var parques = await repositorio.ListarAsync(cancellationToken);
        return parques.Select(Mapear).ToArray();
    }
    public async Task<ParqueDto> RegistrarAsync(CrearParqueDto entrada, CancellationToken cancellationToken = default)
    {
        await ExigirAutenticacionAsync();
        if (!await usuario.EsAdministradorAsync())
            throw new AccesoNoAutorizadoException("Solo un administrador puede registrar parques.");
        ArgumentNullException.ThrowIfNull(entrada);
        var parque = Parque.Crear(entrada.Nombre, entrada.Direccion);
        if (await repositorio.ExisteNombreAsync(parque.NombreNormalizado, cancellationToken))
            throw new NombreParqueDuplicadoException("Ya existe un parque con ese nombre.");
        await repositorio.AgregarAsync(parque, cancellationToken);
        return Mapear(parque);
    }
    private async Task ExigirAutenticacionAsync()
    {
        if (!await usuario.EstaAutenticadoAsync())
            throw new AccesoNoAutorizadoException("Iniciá sesión para consultar parques.");
    }
    private static ParqueDto Mapear(Parque parque) => new(parque.Id, parque.Nombre, parque.Direccion);
}
