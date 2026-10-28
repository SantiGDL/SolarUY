namespace SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

public sealed record NodoDto(
    int Id,
    int ParqueId,
    string Nombre,
    string UbicacionDentroParque);
