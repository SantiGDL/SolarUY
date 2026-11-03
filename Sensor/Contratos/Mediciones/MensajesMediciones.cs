namespace Sensor.Contratos.Mediciones;

public sealed record PanelMedicionesDto(int Id, double PotenciaActual, string Orientacion, string Estado);
public sealed record NodoMedicionesDto(int Id, string Nombre, string Ubicacion, PanelMedicionesDto[] Paneles);
public sealed record ParqueMedicionesDto(int Id, string Nombre, NodoMedicionesDto[] Nodos);
public sealed record SeleccionMedicionesDto(int ParqueId, int NodoId, int? PanelId);
public sealed record LecturaMedicionesDto(int ParqueId, int NodoId, int PanelId, double ValorGenerado, DateTimeOffset FechaHora);
public sealed record EntregaMedicionesDto(int ParqueId, int NodoId, LecturaMedicionesDto[] Lecturas);
