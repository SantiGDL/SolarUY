using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

namespace SolarUyPruebas.Unitarias.Mediciones;

public sealed class HistorialTests
{
    private static readonly DateTimeOffset Referencia = new(2026, 11, 5, 13, 27, 15, TimeSpan.FromHours(-3));

    [Theory]
    [InlineData(PeriodoHistorial.Ultimas24Horas, 1)]
    [InlineData(PeriodoHistorial.Ultimos30Dias, 30)]
    [InlineData(PeriodoHistorial.Ultimos365Dias, 365)]
    public void Periodos_ConservanInstanteYDuracionMovil(PeriodoHistorial periodo, int dias)
    {
        var consulta = new ConsultaHistorial(1, null, null, periodo, ModoHistorial.Crudos, Referencia);
        Assert.Equal(Referencia.ToUniversalTime(), consulta.Hasta);
        Assert.Equal(TimeSpan.FromDays(dias), consulta.Hasta - consulta.Desde!.Value);
        Assert.Equal(TimeSpan.Zero, consulta.Hasta.Offset);
    }

    [Fact]
    public void Todos_NoImponeFechaInicial()
    {
        var consulta = new ConsultaHistorial(1, 2, null, PeriodoHistorial.Todos, ModoHistorial.Promedio, Referencia);
        Assert.Null(consulta.Desde);
        Assert.Equal(Referencia, consulta.Hasta);
    }

    [Theory]
    [InlineData(0, null, null)]
    [InlineData(1, 0, null)]
    [InlineData(1, null, 3)]
    [InlineData(1, 2, -1)]
    public void Seleccion_RechazaIdentificadoresInvalidos(int parque, int? nodo, int? panel) =>
        Assert.Throws<ArgumentException>(() => new ConsultaHistorial(parque, nodo, panel,
            PeriodoHistorial.Todos, ModoHistorial.Crudos, Referencia));

    [Fact]
    public void Consulta_RechazaOpcionesYFechaInvalidas()
    {
        Assert.Throws<ArgumentException>(() => new ConsultaHistorial(1, null, null, (PeriodoHistorial)99, ModoHistorial.Crudos, Referencia));
        Assert.Throws<ArgumentException>(() => new ConsultaHistorial(1, null, null, PeriodoHistorial.Todos, (ModoHistorial)99, Referencia));
        Assert.Throws<ArgumentException>(() => new ConsultaHistorial(1, null, null, PeriodoHistorial.Todos, ModoHistorial.Crudos, default));
    }

    [Fact]
    public void DiaUruguay_CambiaExactamenteEnMedianocheLocal()
    {
        var limite = new DateTimeOffset(2026, 11, 5, 3, 0, 0, TimeSpan.Zero);
        Assert.Equal(new DateTime(2026, 11, 4), ResumenHistorial.DiaUruguay(limite.AddTicks(-1)));
        Assert.Equal(new DateTime(2026, 11, 5), ResumenHistorial.DiaUruguay(limite));
    }

    [Theory]
    [InlineData(600, 3, 200)]
    [InlineData(0, 2, 0)]
    [InlineData(10, 4, 2.5)]
    public void Promedio_UsaCantidadRealSinCompletarHuecos(double suma, long cantidad, double esperado) =>
        Assert.Equal(esperado, ResumenHistorial.Promedio(suma, cantidad));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, -1)]
    [InlineData(-2, 1)]
    [InlineData(double.NaN, 1)]
    [InlineData(double.PositiveInfinity, 1)]
    public void Promedio_NoInventaCeroSinMuestras(double suma, long cantidad) =>
        Assert.Throws<ArgumentException>(() => ResumenHistorial.Promedio(suma, cantidad));
}
