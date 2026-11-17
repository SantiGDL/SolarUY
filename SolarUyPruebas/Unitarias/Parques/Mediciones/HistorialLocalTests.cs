using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Mediciones.Interfaz.Apis.Local.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUyPruebas.Unitarias.Mediciones;

public sealed class HistorialLocalTests
{
    [Fact]
    public async Task Contrato_ConservaFiltrosFilasYReferenciaAlExportar()
    {
        var repositorio = new Repositorio();
        var exportador = new Exportador();
        var acceso = new Acceso();
        var local = new HistorialMedicionesLocalService(new HistorialMedicionesService(
            repositorio, new Catalogo(), acceso, exportador));

        foreach (var periodo in Enum.GetValues<PeriodoHistorial>())
        foreach (var modo in Enum.GetValues<ModoHistorial>())
        {
            var resultado = await local.ConsultarAsync(1, 2, 3, periodo.ToString(), modo.ToString(), default);
            var original = repositorio.Ultima!;
            Assert.Equal(periodo, original.Periodo);
            Assert.Equal(modo, original.Modo);
            Assert.Equal(original.Desde, resultado.Consulta.Desde);
            Assert.Equal(original.Hasta, resultado.Consulta.Hasta);
            Assert.Equal(150, Assert.Single(resultado.Filas).Valor);
            var consultas = repositorio.Consultas;

            Assert.Equal(new byte[] { 1, 2, 3 }, await local.ExportarAsync(resultado));
            Assert.Equal(consultas, repositorio.Consultas);
            Assert.Equal(original.Hasta, exportador.Resultado!.Consulta.Hasta);
            Assert.Equal(original.Desde, exportador.Resultado.Consulta.Desde);
            Assert.Equal(original.Periodo, exportador.Resultado.Consulta.Periodo);
            Assert.Equal(original.Modo, exportador.Resultado.Consulta.Modo);
            Assert.Equal(resultado.Seleccion, exportador.Resultado.Seleccion);
            Assert.Equal(resultado.Filas.ToArray(), exportador.Resultado.Filas.ToArray());

            acceso.Permitido = false;
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => local.ExportarAsync(resultado));
            acceso.Permitido = true;
        }

        repositorio.SinFilas = true;
        Assert.Empty((await local.ConsultarAsync(1, 2, 3, "Todos", "Crudos", default)).Filas);
        acceso.Permitido = false;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => local.ObtenerOpcionesAsync(default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => local.ConsultarAsync(1, 2, 3, "Todos", "Crudos", default));
    }

    private sealed class Acceso : IAccesoHistorial
    {
        public bool Permitido { get; set; } = true;
        public Task<bool> PuedeConsultarAsync() => Task.FromResult(Permitido);
    }

    private sealed class Catalogo : ICatalogoHistorial
    {
        public Task<CatalogoHistorial> ObtenerAsync(CancellationToken ct) => Task.FromResult(
            new CatalogoHistorial([new(1, "Parque")], [new(2, 1, "Nodo")], [new(3, 2)]));
    }

    private sealed class Exportador : IExportadorHistorialPdf
    {
        public ResultadoHistorialDto? Resultado { get; private set; }
        public byte[] Generar(ResultadoHistorialDto resultado)
        {
            Resultado = resultado;
            return [1, 2, 3];
        }
    }

    private sealed class Repositorio : IMedicionRepository
    {
        public ConsultaHistorial? Ultima { get; private set; }
        public int Consultas { get; private set; }
        public bool SinFilas { get; set; }
        public Task AgregarEntregaAsync(IReadOnlyList<Medicion> mediciones, CancellationToken ct) =>
            throw new InvalidOperationException("El historial no debe escribir mediciones.");
        public Task<IReadOnlyList<AsociacionHistorial>> ListarAsociacionesAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AsociacionHistorial>>([new(1, 2, 3)]);
        public Task<IReadOnlyList<FilaHistorial>> ConsultarAsync(ConsultaHistorial consulta, CancellationToken ct)
        {
            Ultima = consulta;
            Consultas++;
            return Task.FromResult<IReadOnlyList<FilaHistorial>>(SinFilas ? [] :
                [new(1, 1, 2, 3, consulta.Hasta, consulta.Hasta, null, 150, 1)]);
        }
    }
}
