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
    private static void ValidarAcceso(AccesoProduccion acceso)
    {
        if (!acceso.EsAdministrador || string.IsNullOrWhiteSpace(acceso.Propietario))
            throw new ArgumentException("Producción requiere un administrador autenticado.");
    }
    private SesionProduccion ObtenerSesionAutorizada(AccesoProduccion acceso, Guid id)
    {
        ValidarAcceso(acceso);
        var sesion = sesiones.Obtener(id);
        if (sesion.Propietario != acceso.Propietario || sesion.Cerrada)
            throw new ArgumentException("La sesión de Producción no pertenece al usuario.");
        return sesion;
    }
    public Guid CrearSesion(AccesoProduccion acceso)
    {
        ValidarAcceso(acceso);
        return sesiones.Crear(acceso.Propietario).Id;
    }
    //peticion a servicio para obtener parques
    public Task<IReadOnlyList<ParqueProduccion>> ObtenerCatalogoAsync(AccesoProduccion acceso, CancellationToken cancelacion)
    {
        ValidarAcceso(acceso);
        return catalogo.ObtenerAsync(acceso.EsAdministrador, cancelacion);
    }
    
    public ChannelReader<VistaProduccion> Observar(AccesoProduccion acceso, Guid id) => ObtenerSesionAutorizada(acceso, id).Cambios.Reader;

    private static void PublicarVista(SesionProduccion sesion)
    {
        sesion.UltimaActividad = DateTimeOffset.UtcNow;
        var configuracion = sesion.Configuracion;
        var paneles = sesion.Nodo?.Paneles.Select(panel =>
        {
            var revision = configuracion!.Paneles.First(p => p.Id == panel.Id).RevisionEstado;
            if (panel.Deshabilitado) return new PanelActual(panel, revision, 0);

            // Una lectura previa al cambio de configuración ya no representa el estado actual.
            if (sesion.Actuales.TryGetValue(panel.Id, out var lectura) && lectura.RevisionEstado == revision)
                return new PanelActual(panel, revision, lectura.ValorGenerado);

            return new PanelActual(panel, revision, null);
        }).ToArray() ?? [];
        sesion.Cambios.Writer.TryWrite(new(configuracion?.Version ?? 0, configuracion?.ParqueId ?? 0, sesion.Nodo,
            sesion.Conexion is not null, sesion.Confirmado, sesion.Pendiente, sesion.Estado, paneles, sesion.Muestras.ToArray()));
    }

    public async Task ConfigurarAsync(AccesoProduccion acceso, Guid id, int parqueId, int nodoId, CancellationToken cancelacion)
    {
        var sesion = ObtenerSesionAutorizada(acceso, id);
        var bloqueoNodo = sesiones.BloqueoNodo(nodoId);
        // Tomamos el bloqueo del nodo antes que el de la sesión, igual que al guardar entregas.
        await bloqueoNodo.WaitAsync(cancelacion);
        try
        {
            var parques = await ObtenerCatalogoAsync(acceso, cancelacion);
            var nodo = parques.FirstOrDefault(p => p.Id == parqueId)?.Nodos.FirstOrDefault(n => n.Id == nodoId)
                ?? throw new ArgumentException("El nodo seleccionado no tiene paneles o ya no existe.");
            await sesion.Exclusividad.WaitAsync(cancelacion);
            try
            {
                if (sesion.Cerrada) return;
                if (sesion.Configuracion?.ParqueId != parqueId || sesion.Configuracion.NodoId != nodoId)
                {
                    sesion.Configuracion = new(id, ++sesion.UltimaVersion, parqueId, nodoId,
                        nodo.Paneles.Select(p => new PanelMotor(p.Id, p.PotenciaActual, p.Deshabilitado, 1)).ToArray());
                    sesion.Nodo = nodo;
                    sesion.Confirmado = false;
                    // Limpiamos los datos del nodo anterior antes de publicar la nueva selección.
                    LimpiarMedicionesDeLaSesion(sesion);
                }
                else
                {
                    var paneles = nodo.Paneles.Select(p =>
                    {
                        var panelAnterior = sesion.Configuracion.Paneles.FirstOrDefault(x => x.Id == p.Id);
                        var revision = panelAnterior?.RevisionEstado ?? 1;
                        if (panelAnterior is not null &&
                            (panelAnterior.PotenciaActual != p.PotenciaActual || panelAnterior.Deshabilitado != p.Deshabilitado))
                            revision++;

                        return new PanelMotor(p.Id, p.PotenciaActual, p.Deshabilitado, revision);
                    }).ToArray();
                    sesion.Configuracion = sesion.Configuracion with { Paneles = paneles };
                    sesion.Nodo = nodo;
                }
                await EnviarConfiguracionAsync(sesion, cancelacion);
            }
            finally { sesion.Exclusividad.Release(); }
        }
        finally { bloqueoNodo.Release(); }
    }
    // Restablece la vista y el seguimiento de entregas al dejar una selección.
    private static void LimpiarMedicionesDeLaSesion(SesionProduccion sesion)
    {
        sesion.Muestras.Clear();
        sesion.Actuales.Clear();
        sesion.RevisionesConfirmadas.Clear();
        sesion.EntregasGuardadas.Clear();
    }

    // La página puede elegir un nodo antes de que el sensor se conecte.
    private async Task EnviarConfiguracionAsync(SesionProduccion sesion, CancellationToken cancelacion)
    {
        if (sesion.Configuracion is null || sesion.Conexion is null)
        {
            sesion.Estado = "Esperando el motor WebAssembly…";
            PublicarVista(sesion);
            return;
        }
        try
        {
            sesion.Pendiente = !sesion.Confirmado || sesion.Pendiente;
            await motor.ConfigurarAsync(sesion.Conexion, sesion.Configuracion, cancelacion);
            sesion.Estado = sesion.Confirmado ? "Motor conectado" : "Esperando confirmación del motor…";
        }
        catch (Exception) when (!cancelacion.IsCancellationRequested)
        {
            sesion.Pendiente = true;
            sesion.Estado = "Configuración pendiente de sincronización. Volvé a Conectar.";
        }
        PublicarVista(sesion);
    }

    public async Task VaciarSeleccionAsync(AccesoProduccion acceso, Guid id, CancellationToken cancelacion)
    {
        var sesion = ObtenerSesionAutorizada(acceso, id);
        await sesion.Exclusividad.WaitAsync(cancelacion);
        try
        {
            sesion.Configuracion = null;
            sesion.Nodo = null;
            sesion.Confirmado = false;
            sesion.Pendiente = false;
            LimpiarMedicionesDeLaSesion(sesion);
            if (sesion.Conexion is not null) await motor.DetenerAsync(sesion.Conexion, id, cancelacion);
            sesion.Estado = "No hay nodos con paneles disponibles";
            PublicarVista(sesion);
        }
        finally { sesion.Exclusividad.Release(); }
    }

    public async Task RegistrarSensorAsync(AccesoProduccion acceso, Guid id, string conexion, CancellationToken cancelacion)
    {
        var sesion = ObtenerSesionAutorizada(acceso, id);
        await sesion.Exclusividad.WaitAsync(cancelacion);
        try
        {
            if (sesion.Cerrada) return;
            if (sesion.Conexion is not null && sesion.Conexion != conexion)
                throw new ArgumentException("La sesión ya tiene un motor conectado.");
            sesion.Conexion = conexion;
            sesion.Confirmado = false;
            // El sensor recibe la selección vigente aunque se haya configurado antes de conectar.
            await EnviarConfiguracionAsync(sesion, cancelacion);
        }
        finally { sesion.Exclusividad.Release(); }
    }

    public async Task ConfirmarAsync(AccesoProduccion acceso, Guid id, string conexion,
        long version, PanelMotor[] estados, CancellationToken cancelacion)
    {
        var sesion = ObtenerSesionAutorizada(acceso, id);
        await sesion.Exclusividad.WaitAsync(cancelacion);
        try
        {
            // Una confirmación atrasada no habilita entregas para la selección actual.
            if (sesion.Conexion != conexion || sesion.Configuracion?.Version != version) return;
            var vigente = sesion.Configuracion.Paneles;
            if (estados.Length != vigente.Length || vigente.Any(p => !estados.Contains(p))) return;
            sesion.Confirmado = true;
            foreach (var panel in vigente) sesion.RevisionesConfirmadas[panel.Id] = panel.RevisionEstado;
            sesion.Pendiente = false;
            sesion.Estado = "Motor conectado · esperando próxima entrega";
            PublicarVista(sesion);
        }
        finally { sesion.Exclusividad.Release(); }
    }

    public async Task CambiarHabilitacionAsync(AccesoProduccion acceso, Guid id, int panelId,
        bool deshabilitado, CancellationToken cancelacion)
    {
        var origen = ObtenerSesionAutorizada(acceso, id);
        var configuracion = origen.Configuracion ?? throw new ArgumentException("No hay un nodo conectado.");
        var bloqueoNodo = sesiones.BloqueoNodo(configuracion.NodoId);
        await bloqueoNodo.WaitAsync(cancelacion);
        try
        {
            if (origen.Configuracion?.Version != configuracion.Version)
                throw new ArgumentException("La selección cambió. Volvé a intentar.");
            await catalogo.CambiarHabilitacionAsync(acceso.EsAdministrador, configuracion.ParqueId,
                configuracion.NodoId, panelId, deshabilitado, cancelacion);
            // Terminamos de reflejar el cambio guardado aunque se cierre la página que lo solicitó.
            foreach (var sesion in sesiones.Listar())
            {
                await sesion.Exclusividad.WaitAsync(CancellationToken.None);
                try
                {
                    if (sesion.Cerrada || sesion.Configuracion?.NodoId != configuracion.NodoId || sesion.Nodo is null) continue;
                    var anterior = sesion.Configuracion.Paneles.FirstOrDefault(p => p.Id == panelId);
                    if (anterior is null) continue;
                    var nuevo = anterior.Deshabilitado == deshabilitado ? anterior :
                        anterior with { Deshabilitado = deshabilitado, RevisionEstado = anterior.RevisionEstado + 1 };
                    sesion.Configuracion = sesion.Configuracion with { Paneles = sesion.Configuracion.Paneles
                        .Select(p => p.Id == panelId ? nuevo : p).ToArray() };
                    sesion.Nodo = sesion.Nodo with { Paneles = sesion.Nodo.Paneles.Select(p => p.Id == panelId
                        ? p with { Deshabilitado = deshabilitado } : p).ToArray() };
                    if (nuevo != anterior) sesion.Actuales.Remove(panelId);
                    sesion.Pendiente = true;
                    sesion.Estado = "Estado guardado · sincronización pendiente";
                    if (sesion.Conexion is not null)
                    {
                        try
                        {
                            await motor.CambiarHabilitacionAsync(sesion.Conexion, new(sesion.Id, sesion.Configuracion.Version,
                                panelId, deshabilitado, nuevo.RevisionEstado), CancellationToken.None);
                        }
                        catch (Exception) { /* Se reenvía el estado autoritativo al reconectar. */ }
                    }
                    PublicarVista(sesion);
                }
                finally { sesion.Exclusividad.Release(); }
            }
        }
        finally { bloqueoNodo.Release(); }
    }

    public async Task RegistrarEntregaAsync(AccesoProduccion acceso, string conexion, EntregaMotor entrega, CancellationToken cancelacion)
    {
        var sesion = ObtenerSesionAutorizada(acceso, entrega.SesionId);
        var bloqueoNodo = sesiones.BloqueoNodo(entrega.NodoId);
        await bloqueoNodo.WaitAsync(cancelacion);
        try
        {
            await sesion.Exclusividad.WaitAsync(cancelacion);
            try
            {
                var configuracion = sesion.Configuracion;
                if (entrega.EntregaId == Guid.Empty) throw new ArgumentException("La entrega requiere un identificador.");
                if (sesion.Cerrada || sesion.Conexion != conexion || configuracion is null || !sesion.Confirmado ||
                    configuracion.Version != entrega.Version || configuracion.ParqueId != entrega.ParqueId || configuracion.NodoId != entrega.NodoId)
                    throw new ArgumentException("La entrega no corresponde a la conexión y configuración vigentes.");
                // Una entrega que ya se guardó no vuelve a sumar muestras.
                if (sesion.EntregasGuardadas.Contains(entrega.EntregaId)) return;
                if (entrega.Lecturas is null || entrega.Lecturas.Length == 0 || entrega.Lecturas.Any(l => l is null) ||
                    entrega.Lecturas.Select(l => l.PanelId).Distinct().Count() != entrega.Lecturas.Length)
                    throw new ArgumentException("La entrega no es válida.");
                var actual = (await catalogo.ObtenerAsync(acceso.EsAdministrador, cancelacion))
                    .FirstOrDefault(p => p.Id == configuracion.ParqueId)?.Nodos.FirstOrDefault(n => n.Id == configuracion.NodoId)
                    ?? throw new ArgumentException("El nodo ya no existe.");
                var validas = ValidarLecturas(entrega.Lecturas, sesion, configuracion, actual);
                foreach (var p in configuracion.Paneles.Where(p => !p.Deshabilitado &&
                    actual.Paneles.Any(a => a.Id == p.Id && !a.Deshabilitado)))
                {
                    // Puede haber una revisión anterior en tránsito: se omite solo ese panel.
                    if (!entrega.Lecturas.Any(l => l.PanelId == p.Id) &&
                        sesion.RevisionesConfirmadas.GetValueOrDefault(p.Id) == p.RevisionEstado)
                    {
                        sesion.Pendiente = true;
                        await EnviarConfiguracionAsync(sesion, cancelacion);
                        throw new ArgumentException("Faltan lecturas de paneles habilitados.");
                    }
                }
                if (validas.Count == 0) return;
                // La vista solo muestra mediciones cuyo guardado terminó correctamente.
                await mediciones.AgregarEntregaAsync(validas.Select(l => Medicion.Crear(configuracion.ParqueId,
                    configuracion.NodoId, l.PanelId, l.ValorGenerado, l.FechaHora)).ToArray(), cancelacion);
                foreach (var l in validas) sesion.Actuales[l.PanelId] = l;
                sesion.EntregasGuardadas.Add(entrega.EntregaId);
                sesion.Muestras.Add(new(validas[0].FechaHora, validas.ToArray()));
                sesion.Estado = "Entrega guardada";
                PublicarVista(sesion);
            }
            catch
            {
                sesion.Estado = "No se confirmó el guardado de la entrega";
                PublicarVista(sesion);
                throw;
            }
            finally { sesion.Exclusividad.Release(); }
        }
        finally { bloqueoNodo.Release(); }
    }

    private static List<LecturaMotor> ValidarLecturas(LecturaMotor[] lecturas, SesionProduccion sesion,
        ConfiguracionMotor configuracion, NodoProduccion nodoActual)
    {
        var lecturasValidas = new List<LecturaMotor>();
        foreach (var lectura in lecturas)
        {
            var panel = nodoActual.Paneles.FirstOrDefault(p => p.Id == lectura.PanelId);
            var panelConfigurado = configuracion.Paneles.FirstOrDefault(p => p.Id == lectura.PanelId);
            if (panel is null || panelConfigurado is null) throw new ArgumentException("Panel ajeno a la selección.");
            // Un panel puede haberse deshabilitado mientras la entrega estaba en tránsito.
            if (panel.Deshabilitado || panelConfigurado.Deshabilitado || lectura.RevisionEstado != panelConfigurado.RevisionEstado) continue;
            var valorInvalido = !double.IsFinite(lectura.ValorGenerado) ||
                lectura.ValorGenerado < 0 || lectura.ValorGenerado > panel.PotenciaActual;
            if (valorInvalido || lectura.FechaHora == default || lectura.FechaHora > DateTimeOffset.UtcNow.AddSeconds(30))
                throw new ArgumentException("Valor o fecha de medición inválidos.");
            if (sesion.Actuales.TryGetValue(lectura.PanelId, out var ultima) && lectura.FechaHora <= ultima.FechaHora)
                throw new ArgumentException("La lectura ya fue recibida o está fuera de orden.");
            lecturasValidas.Add(lectura with { FechaHora = lectura.FechaHora.ToUniversalTime() });
        }
        return lecturasValidas;
    }

    public async Task DesconectarAsync(string conexion)
    {
        foreach (var sesion in sesiones.Listar())
        {
            await sesion.Exclusividad.WaitAsync();
            try
            {
                if (sesion.Conexion != conexion) continue;
                // Conservamos la selección para enviársela al sensor cuando vuelva a registrarse.
                sesion.Conexion = null;
                sesion.Confirmado = false;
                sesion.Estado = "Motor desconectado · esperando reconexión";
                PublicarVista(sesion);
            }
            finally { sesion.Exclusividad.Release(); }
        }
    }
    public async Task LatidoAsync(AccesoProduccion acceso, Guid id, string conexion, CancellationToken cancelacion)
    {
        var sesion = ObtenerSesionAutorizada(acceso, id);
        await sesion.Exclusividad.WaitAsync(cancelacion);
        try
        {
            if (sesion.Conexion != conexion) throw new ArgumentException("El motor no corresponde a la sesión.");
            sesion.UltimaActividad = DateTimeOffset.UtcNow;
        }
        finally { sesion.Exclusividad.Release(); }
    }
    public async Task ExpirarSesionesAsync()
    {
        foreach (var sesion in sesiones.Listar())
        {
            if (sesion.UltimaActividad < DateTimeOffset.UtcNow.AddMinutes(-2))
                await CerrarAsync(new(sesion.Propietario, true), sesion.Id, soloInactiva: true);
        }
    }
    public async Task CerrarAsync(AccesoProduccion acceso, Guid id, bool soloInactiva = false)
    {
        SesionProduccion sesion;
        try { sesion = ObtenerSesionAutorizada(acceso, id); } catch (ArgumentException) { return; }
        await sesion.Exclusividad.WaitAsync();
        try
        {
            // Puede haber llegado un latido mientras la limpieza esperaba su turno.
            if (soloInactiva && sesion.UltimaActividad >= DateTimeOffset.UtcNow.AddMinutes(-2)) return;
            sesion.Cerrada = true;
            sesion.Cambios.Writer.TryComplete();
            if (sesion.Conexion is not null)
                try { await motor.DetenerAsync(sesion.Conexion, id, CancellationToken.None); } catch (Exception) { }
            sesiones.Retirar(id);
        }
        finally { sesion.Exclusividad.Release(); }
    }
}
