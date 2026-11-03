namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

public sealed record LecturaMedicion(int ParqueId, int NodoId, int PanelId,
    double ValorGenerado, DateTimeOffset FechaHora);
public sealed record EntregaMediciones(int ParqueId, int NodoId, IReadOnlyList<LecturaMedicion> Lecturas);
