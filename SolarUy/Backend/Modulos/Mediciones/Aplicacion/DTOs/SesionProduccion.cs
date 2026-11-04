using System.Threading.Channels;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

public sealed record AccesoProduccion(string Propietario, bool EsAdministrador);
public sealed record PanelMotor(int Id, double PotenciaActual, bool Deshabilitado, long RevisionEstado);
public sealed record ConfiguracionMotor(Guid SesionId, long Version, int ParqueId, int NodoId, PanelMotor[] Paneles);
public sealed record EstadoPanelMotor(Guid SesionId, long Version, int PanelId, bool Deshabilitado, long RevisionEstado);
public sealed record LecturaMotor(int PanelId, long RevisionEstado, double ValorGenerado, DateTimeOffset FechaHora);
public sealed record EntregaMotor(Guid SesionId, Guid EntregaId, long Version, int ParqueId, int NodoId, LecturaMotor[] Lecturas);
public sealed record MuestraProduccion(DateTimeOffset FechaHora, LecturaMotor[] Lecturas)
{
    public double Total => Lecturas.Sum(l => l.ValorGenerado);
}
public sealed record PuntoGeneracion(DateTime FechaHora, double ValorGenerado);
public sealed record PanelActual(PanelProduccion Panel, long RevisionEstado, double? Valor);
public sealed record VistaProduccion(long Version, int ParqueId, NodoProduccion? Nodo,
    bool Conectado, bool Confirmado, bool SincronizacionPendiente, string Estado,
    PanelActual[] Paneles, MuestraProduccion[] Muestras)
{
    public double Potencia(int? panelId) => Paneles.Where(p => panelId is null || p.Panel.Id == panelId)
        .Sum(p => p.Panel.PotenciaActual);
    public double Generacion(int? panelId) => Paneles.Where(p => panelId is null || p.Panel.Id == panelId)
        .Sum(p => p.Valor ?? 0);
    public PuntoGeneracion[] Serie(int? panelId) => Muestras
        .Where(m => panelId is null || m.Lecturas.Any(l => l.PanelId == panelId))
        .Select(m => new PuntoGeneracion(
        m.FechaHora.LocalDateTime, panelId is null ? m.Total :
            m.Lecturas.FirstOrDefault(l => l.PanelId == panelId)?.ValorGenerado ?? 0)).ToArray();
}

// Estado efímero aislado por sesión; nunca contiene servicios scoped ni componentes.
public sealed class SesionProduccion(Guid id, string propietario)
{
    public Guid Id { get; } = id;
    public string Propietario { get; } = propietario;
    public SemaphoreSlim Exclusividad { get; } = new(1, 1);
    public Channel<VistaProduccion> Cambios { get; } = Channel.CreateBounded<VistaProduccion>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
    public string? Conexion { get; set; }
    public ConfiguracionMotor? Configuracion { get; set; }
    public long UltimaVersion { get; set; }
    public NodoProduccion? Nodo { get; set; }
    public bool Confirmado { get; set; }
    public bool Pendiente { get; set; }
    public bool Cerrada { get; set; }
    public DateTimeOffset UltimaActividad { get; set; } = DateTimeOffset.UtcNow;
    public string Estado { get; set; } = "Esperando el motor WebAssembly…";
    public List<MuestraProduccion> Muestras { get; } = [];
    public Dictionary<int, LecturaMotor> Actuales { get; } = [];
    public Dictionary<int, long> RevisionesConfirmadas { get; } = [];
    public HashSet<Guid> EntregasGuardadas { get; } = [];
}
