using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using SolarUy.Backend.Modulos.Parques.Dominio.Enums;
using Xunit;

namespace SolarUyParquesPruebas;

public class PanelHabilitacionTests
{
    [Fact]
    public void PanelNuevoGeneraIndependientementeDeSuEstadoTecnico()
    {
        foreach (var estado in Enum.GetValues<EstadoPanel>())
            Assert.False(Panel.Crear(1, estado, 50, "Norte").Deshabilitado);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualizarDatosConservaHabilitacion(bool deshabilitado)
    {
        var panel = Panel.Crear(1, EstadoPanel.Bueno, 50, "Norte");
        if (deshabilitado) panel.Deshabilitar();
        panel.Actualizar(2, EstadoPanel.Rotura, 25, "Sur");
        Assert.Equal(deshabilitado, panel.Deshabilitado);
    }

    [Fact]
    public void AlternarEsIdempotenteYNoModificaDatosTecnicos()
    {
        var panel = Panel.Crear(1, EstadoPanel.Intermedio, 50, "Norte");
        panel.Id = 7;
        for (var i = 0; i < 3; i++)
        {
            panel.Deshabilitar(); panel.Deshabilitar();
            Assert.True(panel.Deshabilitado);
            panel.Habilitar(); panel.Habilitar();
            Assert.False(panel.Deshabilitado);
        }
        Assert.Equal(7, panel.Id);
        Assert.Equal(1, panel.NodoId);
        Assert.Equal(50, panel.PotenciaActual);
        Assert.Equal("Norte", panel.Orientacion);
        Assert.Equal(EstadoPanel.Intermedio, panel.Estado);
    }
}
