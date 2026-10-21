namespace SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

public class Nodo
{
    public int Id { get; set; }

    //Relacion con Parque (Muchos a 1)
    public int ParqueId { get; set; }

    public string UbicacionDentroParque { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
}

