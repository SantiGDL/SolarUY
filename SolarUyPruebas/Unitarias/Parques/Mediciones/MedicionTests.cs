using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;

namespace SolarUyPruebas.Unitarias.Mediciones;

public sealed class MedicionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(125.5)]
    public void Crear_ConservaAsociacionYNormalizaFecha(double valor)
    {
        var fecha = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(-3));
        var medicion = Medicion.Crear(1, 2, 3, valor, fecha);
        Assert.Equal((1, 2, 3), (medicion.ParqueId, medicion.NodoId, medicion.PanelId));
        Assert.Equal(valor, medicion.ValorGenerado);
        Assert.Equal(fecha.UtcDateTime, medicion.FechaHora.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, medicion.FechaHora.Offset);
    }

    [Theory]
    [InlineData(0, 2, 3)]
    [InlineData(1, -2, 3)]
    [InlineData(1, 2, 0)]
    public void Crear_RechazaAsociacionesInvalidas(int parque, int nodo, int panel) =>
        Assert.Throws<ArgumentException>(() => Medicion.Crear(parque, nodo, panel, 0, DateTimeOffset.UtcNow));

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Crear_RechazaGeneracionInvalida(double valor) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Medicion.Crear(1, 2, 3, valor, DateTimeOffset.UtcNow));

    [Fact]
    public void Crear_RechazaFechaAusente() =>
        Assert.Throws<ArgumentException>(() => Medicion.Crear(1, 2, 3, 0, default));
}
