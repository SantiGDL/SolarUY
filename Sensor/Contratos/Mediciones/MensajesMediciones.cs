namespace Sensor.Contratos.Mediciones;

public sealed record PanelMotorDto(int Id, double PotenciaActual, bool Deshabilitado, long RevisionEstado);
public sealed record ConfiguracionMotorDto(Guid SesionId, long Version, int ParqueId, int NodoId, PanelMotorDto[] Paneles);
public sealed record EstadoPanelMotorDto(Guid SesionId, long Version, int PanelId, bool Deshabilitado, long RevisionEstado);
public sealed record LecturaMotorDto(int PanelId, long RevisionEstado, double ValorGenerado, DateTimeOffset FechaHora);
public sealed record EntregaMotorDto(Guid SesionId, Guid EntregaId, long Version, int ParqueId, int NodoId, LecturaMotorDto[] Lecturas);
