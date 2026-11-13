namespace SolarUy.Backend.Modulos.Consultas.Aplicacion.DTOs;

public sealed record RespuestaConsultaNaturalDto(
    string Mensaje,
    bool EsListado,
    bool HayDatos,
    bool HayRecorte = false,
    int FilasMostradas = 0,
    int TotalFilas = 0,
    byte[]? Pdf = null,
    string? NombrePdf = null);
