using SolarUy.Backend.Modulos.Parques.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

public sealed class CrearPanelDto
{
    public int NodoId { get; set; }

    public EstadoPanel Estado { get; set; }

    public double PotenciaActual { get; set; }

    public string Orientacion { get; set; } = string.Empty;
}
