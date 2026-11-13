using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

namespace SolarUy.Backend.Modulos.Consultas.Aplicacion.DTOs;

public sealed record ConsultaNaturalRequest(
    string Pregunta,
    int? ParqueId,
    int? NodoId,
    int? PanelId,
    PeriodoHistorial Periodo,
    ModoHistorial Modo,
    bool EsListado);
