using System.Threading.Channels;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;

public sealed class MedicionesService(ICatalogoProduccion catalogo, IMedicionRepository mediciones,
    ISesionesProduccion sesiones, IControlMotor motor)
{
    private static void Autorizar(AccesoProduccion acceso)
    {
        if (!acceso.EsAdministrador || string.IsNullOrWhiteSpace(acceso.Propietario))
            throw new ArgumentException("Producción requiere un administrador autenticado.");
    }
    private SesionProduccion Obtener(AccesoProduccion acceso, Guid id)
    {
        Autorizar(acceso);
        var s = sesiones.Obtener(id);
        if (s.Propietario != acceso.Propietario || s.Cerrada)
            throw new ArgumentException("La sesión de Producción no pertenece al usuario.");
        return s;
    }
    public Guid CrearSesion(AccesoProduccion acceso)
    {
        Autorizar(acceso);
        return sesiones.Crear(acceso.Propietario).Id;
    }
    public Task<IReadOnlyList<ParqueProduccion>> ObtenerCatalogoAsync(AccesoProduccion acceso, CancellationToken ct)
    {
        Autorizar(acceso);
        return catalogo.ObtenerAsync(acceso.EsAdministrador, ct);
    }
    public ChannelReader<VistaProduccion> Observar(AccesoProduccion acceso, Guid id) => Obtener(acceso, id).Cambios.Reader;

    private static void Publicar(SesionProduccion s)
    {
        s.UltimaActividad = DateTimeOffset.UtcNow;
        var config = s.Configuracion;
        var paneles = s.Nodo?.Paneles.Select(p =>
        {
            var revision = config!.Paneles.First(x => x.Id == p.Id).RevisionEstado;
            double? valor = p.Deshabilitado ? 0 : s.Actuales.TryGetValue(p.Id, out var l)
                && l.RevisionEstado == revision ? l.ValorGenerado : null;
            return new PanelActual(p, revision, valor);
        }).ToArray() ?? [];
        s.Cambios.Writer.TryWrite(new(config?.Version ?? 0, config?.ParqueId ?? 0, s.Nodo,
            s.Conexion is not null, s.Confirmado, s.Pendiente, s.Estado, paneles, s.Muestras.ToArray()));
    }

    public async Task ConfigurarAsync(AccesoProduccion acceso, Guid id, int parqueId, int nodoId, CancellationToken ct)
    {
        var s = Obtener(acceso, id);
        var nodoGate = sesiones.BloqueoNodo(nodoId);
        await nodoGate.WaitAsync(ct);
        try
        {
            var parques = await ObtenerCatalogoAsync(acceso, ct);
            var nodo = parques.FirstOrDefault(p => p.Id == parqueId)?.Nodos.FirstOrDefault(n => n.Id == nodoId)
                ?? throw new ArgumentException("El nodo seleccionado no tiene paneles o ya no existe.");
            await s.Exclusividad.WaitAsync(ct);
            try
            {
                if (s.Cerrada) return;
                if (s.Configuracion?.ParqueId != parqueId || s.Configuracion.NodoId != nodoId)
                {
                    s.Configuracion = new(id, ++s.UltimaVersion, parqueId, nodoId,
                        nodo.Paneles.Select(p => new PanelMotor(p.Id, p.PotenciaActual, p.Deshabilitado, 1)).ToArray());
                    s.Nodo = nodo;
                    s.Confirmado = false;
                    s.Muestras.Clear();
                    s.Actuales.Clear();
                    s.RevisionesConfirmadas.Clear();
                    s.EntregasGuardadas.Clear();
                }
                else
                {
                    var paneles = nodo.Paneles.Select(p =>
                    {
                        var previo = s.Configuracion.Paneles.FirstOrDefault(x => x.Id == p.Id);
                        var revision = previo is null ? 1 : previo.RevisionEstado +
                            (previo.PotenciaActual != p.PotenciaActual || previo.Deshabilitado != p.Deshabilitado ? 1 : 0);
                        return new PanelMotor(p.Id, p.PotenciaActual, p.Deshabilitado, revision);
                    }).ToArray();
                    s.Configuracion = s.Configuracion with { Paneles = paneles };
                    s.Nodo = nodo;
                }
                await EnviarConfiguracionAsync(s, ct);
            }
            finally { s.Exclusividad.Release(); }
        }
        finally { nodoGate.Release(); }
    }
    private async Task EnviarConfiguracionAsync(SesionProduccion s, CancellationToken ct)
    {
        if (s.Configuracion is null || s.Conexion is null)
        {
            s.Estado = "Esperando el motor WebAssembly…";
            Publicar(s);
            return;
        }
        try
        {
            s.Pendiente = !s.Confirmado || s.Pendiente;
            await motor.ConfigurarAsync(s.Conexion, s.Configuracion, ct);
            s.Estado = s.Confirmado ? "Motor conectado" : "Esperando confirmación del motor…";
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            s.Pendiente = true;
            s.Estado = "Configuración pendiente de sincronización. Volvé a Conectar.";
        }
        Publicar(s);
    }

    public async Task VaciarSeleccionAsync(AccesoProduccion acceso, Guid id, CancellationToken ct)
    {
        var s = Obtener(acceso, id);
        await s.Exclusividad.WaitAsync(ct);
        try
        {
            s.Configuracion = null;
            s.Nodo = null;
            s.Confirmado = false;
            s.Pendiente = false;
            s.Muestras.Clear();
            s.Actuales.Clear();
            s.RevisionesConfirmadas.Clear();
            s.EntregasGuardadas.Clear();
            if (s.Conexion is not null) await motor.DetenerAsync(s.Conexion, id, ct);
            s.Estado = "No hay nodos con paneles disponibles";
            Publicar(s);
        }
        finally { s.Exclusividad.Release(); }
    }

    public async Task RegistrarSensorAsync(AccesoProduccion acceso, Guid id, string conexion, CancellationToken ct)
    {
        var s = Obtener(acceso, id);
        await s.Exclusividad.WaitAsync(ct);
        try
        {
            if (s.Cerrada) return;
            if (s.Conexion is not null && s.Conexion != conexion)
                throw new ArgumentException("La sesión ya tiene un motor conectado.");
            s.Conexion = conexion;
            s.Confirmado = false;
            await EnviarConfiguracionAsync(s, ct);
        }
        finally { s.Exclusividad.Release(); }
    }

    public async Task ConfirmarAsync(AccesoProduccion acceso, Guid id, string conexion,
        long version, PanelMotor[] estados, CancellationToken ct)
    {
        var s = Obtener(acceso, id);
        await s.Exclusividad.WaitAsync(ct);
        try
        {
            if (s.Conexion != conexion || s.Configuracion?.Version != version) return;
            var vigente = s.Configuracion.Paneles;
            if (estados.Length != vigente.Length || vigente.Any(p => !estados.Contains(p))) return;
            s.Confirmado = true;
            foreach (var panel in vigente) s.RevisionesConfirmadas[panel.Id] = panel.RevisionEstado;
            s.Pendiente = false;
            s.Estado = "Motor conectado · esperando próxima entrega";
            Publicar(s);
        }
        finally { s.Exclusividad.Release(); }
    }

    public async Task CambiarHabilitacionAsync(AccesoProduccion acceso, Guid id, int panelId,
        bool deshabilitado, CancellationToken ct)
    {
        var origen = Obtener(acceso, id);
        var config = origen.Configuracion ?? throw new ArgumentException("No hay un nodo conectado.");
        var gate = sesiones.BloqueoNodo(config.NodoId);
        await gate.WaitAsync(ct);
        try
        {
            if (origen.Configuracion?.Version != config.Version)
                throw new ArgumentException("La selección cambió. Volvé a intentar.");
            await catalogo.CambiarHabilitacionAsync(acceso.EsAdministrador, config.ParqueId,
                config.NodoId, panelId, deshabilitado, ct);
            // El flag ya está persistido. Un fallo de transporte no lo revierte.
            foreach (var s in sesiones.Listar())
            {
                await s.Exclusividad.WaitAsync(CancellationToken.None);
                try
                {
                    if (s.Cerrada || s.Configuracion?.NodoId != config.NodoId || s.Nodo is null) continue;
                    var anterior = s.Configuracion.Paneles.FirstOrDefault(p => p.Id == panelId);
                    if (anterior is null) continue;
                    var nuevo = anterior.Deshabilitado == deshabilitado ? anterior :
                        anterior with { Deshabilitado = deshabilitado, RevisionEstado = anterior.RevisionEstado + 1 };
                    s.Configuracion = s.Configuracion with { Paneles = s.Configuracion.Paneles
                        .Select(p => p.Id == panelId ? nuevo : p).ToArray() };
                    s.Nodo = s.Nodo with { Paneles = s.Nodo.Paneles.Select(p => p.Id == panelId
                        ? p with { Deshabilitado = deshabilitado } : p).ToArray() };
                    if (nuevo != anterior) s.Actuales.Remove(panelId);
                    s.Pendiente = true;
                    s.Estado = "Estado guardado · sincronización pendiente";
                    if (s.Conexion is not null)
                    {
                        try
                        {
                            await motor.CambiarHabilitacionAsync(s.Conexion, new(s.Id, s.Configuracion.Version,
                                panelId, deshabilitado, nuevo.RevisionEstado), CancellationToken.None);
                        }
                        catch (Exception) { /* Se reenvía el estado autoritativo al reconectar. */ }
                    }
                    Publicar(s);
                }
                finally { s.Exclusividad.Release(); }
            }
        }
        finally { gate.Release(); }
    }

    public async Task RegistrarEntregaAsync(AccesoProduccion acceso, string conexion, EntregaMotor entrega, CancellationToken ct)
    {
        var s = Obtener(acceso, entrega.SesionId);
        var gate = sesiones.BloqueoNodo(entrega.NodoId);
        await gate.WaitAsync(ct);
        try
        {
            await s.Exclusividad.WaitAsync(ct);
            try
            {
                var config = s.Configuracion;
                if (entrega.EntregaId == Guid.Empty) throw new ArgumentException("La entrega requiere un identificador.");
                if (s.Cerrada || s.Conexion != conexion || config is null || !s.Confirmado ||
                    config.Version != entrega.Version || config.ParqueId != entrega.ParqueId || config.NodoId != entrega.NodoId)
                    throw new ArgumentException("La entrega no corresponde a la conexión y configuración vigentes.");
                if (s.EntregasGuardadas.Contains(entrega.EntregaId)) return;
                if (entrega.Lecturas is null || entrega.Lecturas.Length == 0 || entrega.Lecturas.Any(l => l is null) ||
                    entrega.Lecturas.Select(l => l.PanelId).Distinct().Count() != entrega.Lecturas.Length)
                    throw new ArgumentException("La entrega no es válida.");
                var actual = (await catalogo.ObtenerAsync(acceso.EsAdministrador, ct))
                    .FirstOrDefault(p => p.Id == config.ParqueId)?.Nodos.FirstOrDefault(n => n.Id == config.NodoId)
                    ?? throw new ArgumentException("El nodo ya no existe.");
                var validas = new List<LecturaMotor>();
                foreach (var l in entrega.Lecturas)
                {
                    var panel = actual.Paneles.FirstOrDefault(p => p.Id == l.PanelId);
                    var estado = config.Paneles.FirstOrDefault(p => p.Id == l.PanelId);
                    if (panel is null || estado is null) throw new ArgumentException("Panel ajeno a la selección.");
                    if (panel.Deshabilitado || estado.Deshabilitado || l.RevisionEstado != estado.RevisionEstado) continue;
                    if (!double.IsFinite(l.ValorGenerado) || l.ValorGenerado < 0 || l.ValorGenerado > panel.PotenciaActual ||
                        l.FechaHora == default || l.FechaHora > DateTimeOffset.UtcNow.AddSeconds(30))
                        throw new ArgumentException("Valor o fecha de medición inválidos.");
                    if (s.Actuales.TryGetValue(l.PanelId, out var ultima) && l.FechaHora <= ultima.FechaHora)
                        throw new ArgumentException("La lectura ya fue recibida o está fuera de orden.");
                    validas.Add(l with { FechaHora = l.FechaHora.ToUniversalTime() });
                }
                foreach (var p in config.Paneles.Where(p => !p.Deshabilitado &&
                    actual.Paneles.Any(a => a.Id == p.Id && !a.Deshabilitado)))
                {
                    // Puede haber una revisión anterior en tránsito: se omite solo ese panel.
                    if (!entrega.Lecturas.Any(l => l.PanelId == p.Id) &&
                        s.RevisionesConfirmadas.GetValueOrDefault(p.Id) == p.RevisionEstado)
                    {
                        s.Pendiente = true;
                        await EnviarConfiguracionAsync(s, ct);
                        throw new ArgumentException("Faltan lecturas de paneles habilitados.");
                    }
                }
                if (validas.Count == 0) return;
                await mediciones.AgregarEntregaAsync(validas.Select(l => Medicion.Crear(config.ParqueId,
                    config.NodoId, l.PanelId, l.ValorGenerado, l.FechaHora)).ToArray(), ct);
                foreach (var l in validas) s.Actuales[l.PanelId] = l;
                s.EntregasGuardadas.Add(entrega.EntregaId);
                s.Muestras.Add(new(validas[0].FechaHora, validas.ToArray()));
                s.Estado = "Entrega guardada";
                Publicar(s);
            }
            catch
            {
                s.Estado = "No se confirmó el guardado de la entrega";
                Publicar(s);
                throw;
            }
            finally { s.Exclusividad.Release(); }
        }
        finally { gate.Release(); }
    }

    public async Task DesconectarAsync(string conexion)
    {
        foreach (var s in sesiones.Listar())
        {
            await s.Exclusividad.WaitAsync();
            try
            {
                if (s.Conexion != conexion) continue;
                s.Conexion = null;
                s.Confirmado = false;
                s.Estado = "Motor desconectado · esperando reconexión";
                Publicar(s);
            }
            finally { s.Exclusividad.Release(); }
        }
    }
    public async Task LatidoAsync(AccesoProduccion acceso, Guid id, string conexion, CancellationToken ct)
    {
        var s = Obtener(acceso, id);
        await s.Exclusividad.WaitAsync(ct);
        try
        {
            if (s.Conexion != conexion) throw new ArgumentException("El motor no corresponde a la sesión.");
            s.UltimaActividad = DateTimeOffset.UtcNow;
        }
        finally { s.Exclusividad.Release(); }
    }
    public async Task ExpirarSesionesAsync()
    {
        foreach (var s in sesiones.Listar())
        {
            if (s.UltimaActividad < DateTimeOffset.UtcNow.AddMinutes(-2))
                await CerrarAsync(new(s.Propietario, true), s.Id, soloInactiva: true);
        }
    }
    public async Task CerrarAsync(AccesoProduccion acceso, Guid id, bool soloInactiva = false)
    {
        SesionProduccion s;
        try { s = Obtener(acceso, id); } catch (ArgumentException) { return; }
        await s.Exclusividad.WaitAsync();
        try
        {
            if (soloInactiva && s.UltimaActividad >= DateTimeOffset.UtcNow.AddMinutes(-2)) return;
            s.Cerrada = true;
            s.Cambios.Writer.TryComplete();
            if (s.Conexion is not null)
                try { await motor.DetenerAsync(s.Conexion, id, CancellationToken.None); } catch (Exception) { }
            sesiones.Retirar(id);
        }
        finally { s.Exclusividad.Release(); }
    }
}
