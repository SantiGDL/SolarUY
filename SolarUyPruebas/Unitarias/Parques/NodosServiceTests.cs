using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUyPruebas.Unitarias.Parques;

public sealed class NodosServiceTests
{
    [Fact]
    public void CrearNodo_RecortaNombreYUbicacion()
    {
        var nodo = Nodo.Crear(3, "  Inversor norte  ", "  Sector A  ");

        Assert.Equal(3, nodo.ParqueId);
        Assert.Equal("Inversor norte", nodo.Nombre);
        Assert.Equal("Sector A", nodo.UbicacionDentroParque);
    }

    [Theory]
    [InlineData(0, "Nodo", "Sector A")]
    [InlineData(3, "", "Sector A")]
    [InlineData(3, "   ", "Sector A")]
    [InlineData(3, "Nodo", "")]
    [InlineData(3, "Nodo", "   ")]
    public void CrearNodo_RechazaIdentificadorOCamposInvalidos(
        int parqueId,
        string nombre,
        string ubicacion)
    {
        Assert.ThrowsAny<ArgumentException>(() => Nodo.Crear(parqueId, nombre, ubicacion));
    }

    [Fact]
    public void CrearNodo_RechazaCamposFueraDeLimite()
    {
        Assert.Throws<ArgumentException>(
            () => Nodo.Crear(3, new string('n', 101), "Sector A"));
        Assert.Throws<ArgumentException>(
            () => Nodo.Crear(3, "Nodo", new string('u', 251)));
    }

    [Fact]
    public async Task ObtenerPorParqueAsync_RequiereAutenticacion()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: false, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(
            () => servicio.ObtenerPorParqueAsync(3));
        Assert.False(repositorio.ConsultaRealizada);
    }

    [Fact]
    public async Task ObtenerPorParqueAsync_SoloDevuelveNodosDelParqueSolicitado()
    {
        var repositorio = new RepositorioFalso();
        repositorio.Nodos.Add(Nodo.Crear(3, "Nodo B", "Sector B"));
        repositorio.Nodos.Add(Nodo.Crear(4, "Otro parque", "Sector C"));
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: false);

        var resultado = await servicio.ObtenerPorParqueAsync(3);

        var nodo = Assert.Single(resultado);
        Assert.Equal(3, nodo.ParqueId);
        Assert.Equal("Nodo B", nodo.Nombre);
    }

    [Fact]
    public async Task ObtenerTodosAgrupadosPorParqueAsync_AgrupaNodosPorParque()
    {
        var repositorio = new RepositorioFalso();
        repositorio.Nodos.Add(Nodo.Crear(3, "Nodo B", "Sector B"));
        repositorio.Nodos.Add(Nodo.Crear(3, "Nodo A", "Sector A"));
        repositorio.Nodos.Add(Nodo.Crear(4, "Otro parque", "Sector C"));
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: false);

        var resultado = await servicio.ObtenerTodosAgrupadosPorParqueAsync();

        Assert.Equal(2, resultado.Count);
        Assert.Equal(2, resultado[3].Count);
        Assert.Single(resultado[4]);
    }

    [Fact]
    public async Task RegistrarAsync_RequiereRolAdmin()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(
            () => servicio.RegistrarAsync(3, new CrearNodoDto
            {
                Nombre = "Nodo",
                UbicacionDentroParque = "Sector A"
            }));
        Assert.Empty(repositorio.Nodos);
    }

    [Fact]
    public async Task RegistrarAsync_AsociaElNodoAlParqueSeleccionado()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        var resultado = await servicio.RegistrarAsync(3, new CrearNodoDto
        {
            Nombre = "  Nodo norte ",
            UbicacionDentroParque = " Sector A "
        });

        var nodoGuardado = Assert.Single(repositorio.Nodos);
        Assert.Equal(3, nodoGuardado.ParqueId);
        Assert.Equal(3, resultado.ParqueId);
        Assert.Equal("Nodo norte", resultado.Nombre);
        Assert.Equal("Sector A", resultado.UbicacionDentroParque);
    }

    [Fact]
    public async Task RegistrarAsync_RechazaUnParqueInexistente()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        await Assert.ThrowsAsync<ParqueNoEncontradoException>(
            () => servicio.RegistrarAsync(404, new CrearNodoDto
            {
                Nombre = "Nodo",
                UbicacionDentroParque = "Sector A"
            }));
        Assert.Empty(repositorio.Nodos);
    }

    private static INodosService CrearServicio(
        RepositorioFalso repositorio,
        bool autenticado,
        bool administrador) =>
        new NodosService(repositorio, new UsuarioFalso(autenticado, administrador));

    private sealed class UsuarioFalso(bool autenticado, bool administrador) : IUsuarioActual
    {
        public Task<bool> EstaAutenticadoAsync() => Task.FromResult(autenticado);

        public Task<bool> EsAdministradorAsync() => Task.FromResult(administrador);
    }

    private sealed class RepositorioFalso : INodoRepositorio
    {
        public List<Nodo> Nodos { get; } = [];
        public bool ConsultaRealizada { get; private set; }

        public Task<IReadOnlyList<Nodo>> ListarTodosAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Nodo>>(Nodos.ToArray());

        public Task<IReadOnlyList<Nodo>> ListarPorParqueAsync(
            int parqueId,
            CancellationToken cancellationToken = default)
        {
            ConsultaRealizada = true;
            return Task.FromResult<IReadOnlyList<Nodo>>(
                Nodos.Where(nodo => nodo.ParqueId == parqueId).ToArray());
        }

        public Task<bool> ExisteParqueAsync(
            int parqueId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(parqueId is 3 or 4);

        public Task AgregarAsync(
            Nodo nodo,
            CancellationToken cancellationToken = default)
        {
            Nodos.Add(nodo);
            return Task.CompletedTask;
        }
    }
}
