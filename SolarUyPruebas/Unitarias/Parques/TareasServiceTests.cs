using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Enums;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Tareas.Dominio.Entidades;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUyPruebas.Unitarias.Parques;

public sealed class TareasServiceTests
{
    [Fact]
    public void Crear_RecortaObservacionesYComienzaPendiente()
    {
        var tarea = TareaMantenimiento.Crear(
            4,
            TipoMantenimiento.Limpieza,
            "  Revisar suciedad  ",
            new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc));

        Assert.Equal(4, tarea.PanelId);
        Assert.Equal("Revisar suciedad", tarea.Observaciones);
        Assert.Equal(EstadoTarea.Pendiente, tarea.Estado);
        Assert.Null(tarea.FechaFinalizado);
    }

    [Fact]
    public void Crear_RechazaPanelTipoYObservacionesInvalidos()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TareaMantenimiento.Crear(0, TipoMantenimiento.Limpieza, "", DateTime.UtcNow));
        Assert.ThrowsAny<ArgumentException>(() =>
            TareaMantenimiento.Crear(
                1,
                (TipoMantenimiento)99,
                "",
                DateTime.UtcNow));
        Assert.Throws<ArgumentException>(() =>
            TareaMantenimiento.Crear(
                1,
                TipoMantenimiento.Limpieza,
                new string('x', 1001),
                DateTime.UtcNow));
    }

    [Fact]
    public void Completar_RegistraObservacionesYFechas()
    {
        var tarea = CrearTarea();
        var realizada = new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

        tarea.Completar("  Limpieza finalizada  ", realizada);

        Assert.Equal(EstadoTarea.Completada, tarea.Estado);
        Assert.Equal("Limpieza finalizada", tarea.Observaciones);
        Assert.Equal(realizada, tarea.FechaFinalizado);
        Assert.Equal(realizada, tarea.FechaRealizada);
    }

    [Fact]
    public void Completar_RechazaCompletarTareaDosVeces()
    {
        var tarea = CrearTarea();
        tarea.Completar("", DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(
            () => tarea.Completar("", DateTime.UtcNow));
    }

    [Fact]
    public async Task ObtenerTodasAsync_ExigeAdmin()
    {
        var repositorio = new RepositorioFalso();
        repositorio.AgregarParaPrueba(CrearTarea(), 1);
        var servicio = CrearServicio(repositorio, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() => servicio.ObtenerTodasAsync());
        Assert.False(repositorio.ListadoConsultado);
    }

    [Fact]
    public async Task CrearAsync_ExigeAdminYPanelExistente()
    {
        var repositorio = new RepositorioFalso();
        var sinPermiso = CrearServicio(repositorio, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() =>
            sinPermiso.CrearAsync(new CrearTareaDto
            {
                PanelId = 4,
                TipoTarea = TipoMantenimiento.Limpieza
            }));

        var admin = CrearServicio(repositorio, administrador: true);
        await Assert.ThrowsAsync<TareaNoEncontradaException>(() =>
            admin.CrearAsync(new CrearTareaDto
            {
                PanelId = 404,
                TipoTarea = TipoMantenimiento.Limpieza
            }));
        Assert.Empty(repositorio.Tareas);
    }

    [Theory]
    [InlineData(EstadoPanel.Intermedio, TipoMantenimiento.Limpieza)]
    [InlineData(EstadoPanel.Rotura, TipoMantenimiento.Reparacion)]
    public async Task CrearAutomaticaAsync_EligeTipoPorEstado(
        EstadoPanel estado,
        TipoMantenimiento tipoEsperado)
    {
        var repositorio = new RepositorioFalso();
        var servicio = CrearServicio(repositorio, administrador: true);

        var resultado = await servicio.CrearAutomaticaAsync(4, estado);

        var tarea = Assert.Single(repositorio.Tareas.Values);
        Assert.Equal(tipoEsperado, tarea.TipoTarea);
        Assert.Equal(EstadoTarea.Pendiente, resultado!.Estado);
    }

    [Fact]
    public async Task CrearAutomaticaAsync_NoDuplicaMantenimientoActivoDelMismoTipo()
    {
        var repositorio = new RepositorioFalso();
        repositorio.AgregarParaPrueba(CrearTarea(), 1);
        var servicio = CrearServicio(repositorio, administrador: true);

        var resultado = await servicio.CrearAutomaticaAsync(4, EstadoPanel.Intermedio);

        Assert.Null(resultado);
        Assert.Single(repositorio.Tareas);
    }

    [Fact]
    public async Task CompletarAsync_ExigeAdminYGuardaTarea()
    {
        var repositorio = new RepositorioFalso();
        repositorio.AgregarParaPrueba(CrearTarea(), 1);
        var sinPermiso = CrearServicio(repositorio, administrador: false);

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() =>
            sinPermiso.CompletarAsync(1, new CompletarTareaDto()));
        Assert.Equal(EstadoTarea.Pendiente, repositorio.Tareas[1].Estado);

        var admin = CrearServicio(repositorio, administrador: true);
        var resultado = await admin.CompletarAsync(
            1,
            new CompletarTareaDto { Observaciones = "Reparación realizada" });

        Assert.Equal(EstadoTarea.Completada, resultado.Estado);
        Assert.NotNull(resultado.FechaRealizada);
    }

    private static TareaMantenimiento CrearTarea() =>
        TareaMantenimiento.Crear(
            4,
            TipoMantenimiento.Limpieza,
            "",
            DateTime.UtcNow);

    private static TareasService CrearServicio(
        RepositorioFalso repositorio,
        bool administrador) =>
        new(repositorio, new UsuarioFalso(administrador));

    private sealed class UsuarioFalso(bool administrador) : IUsuarioActual
    {
        public Task<bool> EstaAutenticadoAsync() => Task.FromResult(administrador);

        public Task<bool> EsAdministradorAsync() => Task.FromResult(administrador);
    }

    private sealed class RepositorioFalso : ITareaRepositorio
    {
        public Dictionary<int, TareaMantenimiento> Tareas { get; } = [];
        public int? UltimoIdConsultado { get; private set; }
        public bool ListadoConsultado { get; private set; }

        public void AgregarParaPrueba(TareaMantenimiento tarea, int id) => Tareas.Add(id, tarea);

        public Task<IReadOnlyList<TareaMantenimiento>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            ListadoConsultado = true;
            return Task.FromResult<IReadOnlyList<TareaMantenimiento>>(Tareas.Values.ToArray());
        }

        public Task<TareaMantenimiento?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            UltimoIdConsultado = id;
            return Task.FromResult(Tareas.GetValueOrDefault(id));
        }

        public Task<bool> ExistePanelAsync(
            int panelId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(panelId == 4);

        public Task<bool> ExisteActivaAsync(
            int panelId,
            TipoMantenimiento tipoTarea,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Tareas.Values.Any(tarea =>
                tarea.PanelId == panelId
                && tarea.TipoTarea == tipoTarea
                && tarea.Estado != EstadoTarea.Completada));

        public Task AgregarAsync(
            TareaMantenimiento tarea,
            CancellationToken cancellationToken = default)
        {
            Tareas.Add(Tareas.Count + 1, tarea);
            return Task.CompletedTask;
        }

        public Task ActualizarAsync(
            TareaMantenimiento tarea,
            CancellationToken cancellationToken = default)
        {
            var existente = Tareas.FirstOrDefault(par => ReferenceEquals(par.Value, tarea));
            if (existente.Value is null)
            {
                throw new TareaNoEncontradaException("No se encontró la tarea en el repositorio de prueba.");
            }

            Tareas[existente.Key] = tarea;
            return Task.CompletedTask;
        }
    }
}
