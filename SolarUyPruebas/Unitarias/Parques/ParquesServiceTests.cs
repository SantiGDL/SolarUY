using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;

namespace SolarUyPruebas.Unitarias.Parques;

public sealed class ParquesServiceTests
{
    [Fact]
    public void CrearParque_RecortaNombreYDireccionYNormalizaNombre()
    {
        var parque = Parque.Crear("  Parque Sur  ", "  Ruta 1  ");

        Assert.Equal("Parque Sur", parque.Nombre);
        Assert.Equal("Ruta 1", parque.Direccion);
        Assert.Equal("PARQUE SUR", parque.NombreNormalizado);
    }

    [Theory]
    [InlineData("", "Ruta 1")]
    [InlineData("   ", "Ruta 1")]
    [InlineData("Parque", "   ")]
    [InlineData("Parque", "")]
    public void CrearParque_RechazaDatosVacios(string nombre, string direccion)
    {
        Assert.Throws<ArgumentException>(() => Parque.Crear(nombre, direccion));
    }

    [Fact]
    public void CrearParque_RechazaNombreConMasDeCienCaracteres()
    {
        Assert.Throws<ArgumentException>(
            () => Parque.Crear(new string('a', 101), "Ruta 1"));
    }

    [Fact]
    public void CrearParque_RechazaDireccionConMasDeDoscientosCincuentaCaracteres()
    {
        Assert.Throws<ArgumentException>(
            () => Parque.Crear("Parque", new string('a', 251)));
    }

    [Fact]
    public async Task ObtenerTodosAsync_RequiereUsuarioAutenticado()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: false, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(
            () => servicio.ObtenerTodosAsync());
        Assert.False(repositorio.ConsultaRealizada);
    }

    [Fact]
    public async Task RegistrarAsync_RequiereRolAdmin()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(
            () => servicio.RegistrarAsync(new CrearParqueDto
            {
                Nombre = "Parque Sur",
                Direccion = "Ruta 1"
            }));
        Assert.Empty(repositorio.Parques);
    }

    [Fact]
    public async Task RegistrarAsync_AdminGuardaElParqueNormalizado()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        var resultado = await servicio.RegistrarAsync(new CrearParqueDto
        {
            Nombre = "  Parque Sur ",
            Direccion = " Ruta 1 "
        });

        var guardado = Assert.Single(repositorio.Parques);
        Assert.Equal("Parque Sur", resultado.Nombre);
        Assert.Equal("Ruta 1", resultado.Direccion);
        Assert.Equal("PARQUE SUR", guardado.NombreNormalizado);
    }

    [Fact]
    public async Task RegistrarAsync_RechazaNombreDuplicadoSinDistinguirMayusculas()
    {
        var repositorio = new RepositorioFalso();
        repositorio.Parques.Add(Parque.Crear("Parque Sur", "Ruta 1"));
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        await Assert.ThrowsAsync<NombreParqueDuplicadoException>(
            () => servicio.RegistrarAsync(new CrearParqueDto
            {
                Nombre = " parque sur ",
                Direccion = "Ruta 2"
            }));
        Assert.Single(repositorio.Parques);
    }

    private static IParquesService CrearServicio(
        RepositorioFalso repositorio,
        bool autenticado,
        bool administrador) =>
        new ParquesService(repositorio, new UsuarioFalso(autenticado, administrador));

    private sealed class UsuarioFalso(bool autenticado, bool administrador) : IUsuarioActual
    {
        public Task<bool> EstaAutenticadoAsync() => Task.FromResult(autenticado);

        public Task<bool> EsAdministradorAsync() => Task.FromResult(administrador);
    }

    private sealed class RepositorioFalso : IParqueRepositorio
    {
        public List<Parque> Parques { get; } = [];
        public bool ConsultaRealizada { get; private set; }

        public Task<IReadOnlyList<Parque>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            ConsultaRealizada = true;
            return Task.FromResult<IReadOnlyList<Parque>>(Parques);
        }

        public Task<bool> ExisteNombreAsync(
            string nombreNormalizado,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Parques.Any(parque =>
                parque.NombreNormalizado == nombreNormalizado));

        public Task AgregarAsync(
            Parque parque,
            CancellationToken cancellationToken = default)
        {
            Parques.Add(parque);
            return Task.CompletedTask;
        }
    }
}
