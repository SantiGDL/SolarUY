using SolarUy.Backend.Modulos.Parques.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.DTOs;

public sealed record PanelDto(
    int Id,
    int NodoId,
    EstadoPanel Estado,
    double PotenciaActual,
    string Orientacion);
