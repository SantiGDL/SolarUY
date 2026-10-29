using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Parques.Dominio.Enums;

namespace SolarUyPruebas.Unitarias.Parques;

public sealed class PanelServiceTests
{
    [Fact]
    public void CrearPanel_RecortaOrientacionYValidaDatos()
    {
        var panel = Panel.Crear(3, EstadoPanel.Bueno, 125.5, "  Norte  ");

        Assert.Equal(3, panel.NodoId);
        Assert.Equal(EstadoPanel.Bueno, panel.Estado);
        Assert.Equal(125.5, panel.PotenciaActual);
        Assert.Equal("Norte", panel.Orientacion);
    }

    [Theory]
    [InlineData(0, EstadoPanel.Bueno, 10, "Norte")]
    [InlineData(3, EstadoPanel.Bueno, -1, "Norte")]
    [InlineData(3, EstadoPanel.Bueno, double.NaN, "Norte")]
    [InlineData(3, EstadoPanel.Bueno, 10, "")]
    [InlineData(3, EstadoPanel.Bueno, 10, "   ")]
    public void CrearPanel_RechazaDatosInvalidos(
        int nodoId,
        EstadoPanel estado,
        double potenciaActual,
        string orientacion)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Panel.Crear(nodoId, estado, potenciaActual, orientacion));
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
            () => servicio.RegistrarAsync(new CrearPanelDto
            {
                NodoId = 3,
                Estado = EstadoPanel.Bueno,
                PotenciaActual = 10,
                Orientacion = "Norte"
            }));
        Assert.Empty(repositorio.Paneles);
    }

    [Fact]
    public async Task RegistrarAsync_RechazaNodoInexistente()
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        await Assert.ThrowsAsync<NodoNoEncontradoException>(
            () => servicio.RegistrarAsync(new CrearPanelDto
            {
                NodoId = 404,
                Estado = EstadoPanel.Bueno,
                PotenciaActual = 10,
                Orientacion = "Norte"
            }));
        Assert.Empty(repositorio.Paneles);
    }

    [Fact]
    public async Task ActualizarAsync_ActualizaLosCamposDelPanel()
    {
        var repositorio = new RepositorioFalso();
        var panel = Panel.Crear(3, EstadoPanel.Intermedio, 40, "Este");
        panel.Id = 1;
        repositorio.Paneles.Add(panel);
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        var resultado = await servicio.ActualizarAsync(
            1,
            new CrearPanelDto
            {
                NodoId = 3,
                Estado = EstadoPanel.Rotura,
                PotenciaActual = 85.5,
                Orientacion = "  Oeste  "
            });

        var panelGuardado = Assert.Single(repositorio.Paneles);
        Assert.Equal(EstadoPanel.Rotura, panelGuardado.Estado);
        Assert.Equal(85.5, panelGuardado.PotenciaActual);
        Assert.Equal("Oeste", panelGuardado.Orientacion);
        Assert.Equal("Oeste", resultado.Orientacion);
    }

    [Fact]
    public async Task EliminarAsync_EliminaUnPanelExistente()
    {
        var repositorio = new RepositorioFalso();
        var panel = Panel.Crear(3, EstadoPanel.Bueno, 30, "Norte");
        panel.Id = 1;
        repositorio.Paneles.Add(panel);
        var servicio = CrearServicio(repositorio, autenticado: true, administrador: true);

        await servicio.EliminarAsync(1);

        Assert.Empty(repositorio.Paneles);
    }

    private static IPanelesService CrearServicio(
        RepositorioFalso repositorio,
        bool autenticado,
        bool administrador) =>
        new PanelService(repositorio, new UsuarioFalso(autenticado, administrador));

    private sealed class UsuarioFalso(bool autenticado, bool administrador) : IUsuarioActual
    {
        public Task<bool> EstaAutenticadoAsync() => Task.FromResult(autenticado);

        public Task<bool> EsAdministradorAsync() => Task.FromResult(administrador);
    }

    private sealed class RepositorioFalso : IPanelRepositorio
    {
        public List<Panel> Paneles { get; } = [];
        public bool ConsultaRealizada { get; private set; }

        public Task<IReadOnlyList<Panel>> ListarAsync(CancellationToken cancellationToken = default)
        {
            ConsultaRealizada = true;
            return Task.FromResult<IReadOnlyList<Panel>>(Paneles.ToArray());
        }

        public Task<Panel?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            ConsultaRealizada = true;
            return Task.FromResult(Paneles.FirstOrDefault(panel => panel.Id == id));
        }

        public Task<bool> ExisteNodoAsync(int nodoId, CancellationToken cancellationToken = default) =>
            Task.FromResult(nodoId == 3);

        public Task AgregarAsync(Panel panel, CancellationToken cancellationToken = default)
        {
            panel.Id = Paneles.Count == 0 ? 1 : Paneles.Max(p => p.Id) + 1;
            Paneles.Add(panel);
            return Task.CompletedTask;
        }

        public Task ActualizarAsync(Panel panel, CancellationToken cancellationToken = default)
        {
            var existente = Paneles.Single(p => p.Id == panel.Id);
            existente.Estado = panel.Estado;
            existente.PotenciaActual = panel.PotenciaActual;
            existente.Orientacion = panel.Orientacion;
            existente.NodoId = panel.NodoId;
            return Task.CompletedTask;
        }

        public Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var panel = Paneles.Single(p => p.Id == id);
            Paneles.Remove(panel);
            return Task.CompletedTask;
        }
    }
}
