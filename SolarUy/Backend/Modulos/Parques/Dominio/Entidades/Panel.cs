using SolarUy.Backend.Modulos.Parques.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

public class Panel
{
    public int Id { get; set; }

    public EstadoPanel Estado { get; set; }

    public double PotenciaActual { get; set; }

    public string Orientacion { get; set; } = string.Empty;

    //Relacion con Nodo (Muchos a 1)
    public int NodoId { get; set; }
}

