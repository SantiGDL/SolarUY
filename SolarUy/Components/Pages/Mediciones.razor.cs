using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;
using SolarUy.Components.Servicios;

namespace SolarUy.Components.Pages;

public partial class Mediciones
{
    private const string ErrorConfirmacion = "El motor no confirmó la configuración. Sincronización pendiente; los datos visibles conservan la última selección confirmada. Podés volver a Conectar.";
    [Parameter] public Guid SesionId { get; set; }
    [Inject] private MedicionesService Servicio { get; set; } = default!;
    [Inject] private AuthenticationStateProvider Autenticacion { get; set; } = default!;
    private readonly CancellationTokenSource _cancelacionPagina = new();
    private AccesoProduccion _acceso = new("", false);
    private ParqueProduccion[] _catalogo = [];
    // La vista conserva las últimas lecturas confirmadas. El último estado también informa esperas y desconexiones.
    private VistaProduccion? _vistaConfirmada;
    private VistaProduccion? _ultimoEstadoRecibido;

    // Estos selectores pueden apuntar a otro nodo mientras la pantalla muestra el activo.
    private int _parquePendiente;
    private int _nodoPendiente;
    private int? _panelPendienteId;
    private int? _panelId;
    private bool _cargando = true;
    private bool _guardando;
    private bool _destruida;
    private string? _error;
    private Task? _tareaLectura;
    private TaskCompletionSource<bool>? _confirmacion;
    private (int ParqueId, int NodoId)? _seleccionSolicitada;
    private PuntoGeneracion[] _serie = [];
    private NodoProduccion? NodoActivo => _vistaConfirmada?.Nodo;
    private PanelProduccion? PanelVisible => NodoActivo?.Paneles.FirstOrDefault(p => p.Id == _panelId);
    private bool Conectado => _ultimoEstadoRecibido?.Conectado == true && _ultimoEstadoRecibido.Confirmado;
    private bool HayCambiosPendientes => _vistaConfirmada?.Nodo is not null &&
        (_vistaConfirmada.ParqueId != _parquePendiente || _vistaConfirmada.Nodo.Id != _nodoPendiente);
    private bool ControlesDeshabilitados => _cargando || _catalogo.Length == 0;
    private IReadOnlyList<NodoProduccion> NodosPendientes => _catalogo.FirstOrDefault(p => p.Id == _parquePendiente)?.Nodos ?? [];
    private NodoProduccion? NodoPendiente => NodosPendientes.FirstOrDefault(n => n.Id == _nodoPendiente);
    private bool SeleccionCoincideConActiva => _vistaConfirmada?.ParqueId == _parquePendiente && NodoActivo?.Id == _nodoPendiente;
    private bool PuedeConectar => !ControlesDeshabilitados && NodoPendiente is not null;
    private IEnumerable<PanelProduccion> PanelesVisibles => (NodoActivo?.Paneles ?? [])
        .Where(p => _panelId is null || p.Id == _panelId);
    private string NombreParqueActivo => _catalogo.FirstOrDefault(p => p.Id == _vistaConfirmada?.ParqueId)?.Nombre ?? "";
    private double PotenciaVisible => _vistaConfirmada?.Potencia(_panelId) ?? 0;
    private double GeneracionVisible => _vistaConfirmada?.Generacion(_panelId) ?? 0;
    private static string Formato(double valor) => valor.ToString("N0");
    private static string ClaseEstado(string estado) => estado switch
    {
        "Intermedio" => "estado-intermedio", "Rotura" => "estado-rotura", _ => ""
    };
    private string ValorPanel(int id) => _vistaConfirmada?.Paneles.FirstOrDefault(p => p.Panel.Id == id)?.Valor is { } valor
        ? $"{Formato(valor)} W" : "Esperando mediciones";

    protected override async Task OnInitializedAsync()
    {
        _acceso = AccesoProduccionActual.Desde((await Autenticacion.GetAuthenticationStateAsync()).User);
        // Escuchamos antes de cargar porque el catálogo puede disparar la primera configuración.
        _tareaLectura = LeerActualizacionesAsync();
        await CargarCatalogoAsync();
    }
    private async Task LeerActualizacionesAsync()
    {
        try
        {
            // Aplicamos cada notificación en el contexto de renderizado de Blazor.
            await foreach (var vista in Servicio.Observar(_acceso, SesionId).ReadAllAsync(_cancelacionPagina.Token))
                await InvokeAsync(() => ActualizarVista(vista));
        }
        catch (OperationCanceledException) when (_cancelacionPagina.IsCancellationRequested) { }
    }
    // Actualiza siempre el estado de conexión y solo reemplaza las lecturas con una selección confirmada.
    private void ActualizarVista(VistaProduccion vista)
    {
        if (_destruida) return;
        _ultimoEstadoRecibido = vista;
        if (vista.Nodo is null)
        {
            // Al quedar sin nodo seleccionado, la gráfica y el filtro anterior ya no aplican.
            _vistaConfirmada = null;
            _serie = [];
            _panelId = null;
        }
        if (vista.Confirmado && vista.Nodo is not null)
        {
            var cambiaNodo = _vistaConfirmada?.Version != vista.Version;
            if (cambiaNodo || (_panelId is not null &&
                !vista.Nodo.Paneles.Any(p => p.Id == _panelId))) _panelId = null;
            _vistaConfirmada = vista;
            _serie = vista.Serie(_panelId);
            if (!vista.SincronizacionPendiente && _error == ErrorConfirmacion) _error = null;
            // Ajustamos el panel del selector solo si todavía apunta al nodo activo.
            if (SeleccionCoincideConActiva && (cambiaNodo ||
                (_panelPendienteId is not null && !vista.Nodo.Paneles.Any(p => p.Id == _panelPendienteId))))
                _panelPendienteId = null;
            if (_seleccionSolicitada == (vista.ParqueId, vista.Nodo.Id))
                _confirmacion?.TrySetResult(true);
        }
        StateHasChanged();
    }

    // Solicita la selección y espera que el motor confirme ese parque y nodo.
    private async Task ConfigurarSeleccionAsync(int parqueId, int nodoId)
    {
        _seleccionSolicitada = (parqueId, nodoId);
        // Registramos la espera antes de configurar porque la confirmación puede llegar enseguida.
        var confirmacion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _confirmacion = confirmacion;
        try
        {
            await Servicio.ConfigurarAsync(_acceso, SesionId, parqueId, nodoId, _cancelacionPagina.Token);
            // La vista puede tener la confirmación de ese nodo cuando vuelve la llamada.
            if (_ultimoEstadoRecibido is { Confirmado: true, SincronizacionPendiente: false } &&
                _ultimoEstadoRecibido.ParqueId == parqueId && _ultimoEstadoRecibido.Nodo?.Id == nodoId)
                confirmacion.TrySetResult(true);
            await confirmacion.Task.WaitAsync(TimeSpan.FromSeconds(20), _cancelacionPagina.Token);
        }
        finally { _confirmacion = null; _seleccionSolicitada = null; }
    }
    // Conserva la selección si sigue disponible y deja preparada una válida si desapareció.
    private async Task CargarCatalogoAsync()
    {
        _cargando = true;
        _error = null;
        try
        {
            _catalogo = (await Servicio.ObtenerCatalogoAsync(_acceso, _cancelacionPagina.Token)).ToArray();
            if (!_catalogo.Any(p => p.Id == _parquePendiente)) _parquePendiente = _catalogo.FirstOrDefault()?.Id ?? 0;
            if (!NodosPendientes.Any(n => n.Id == _nodoPendiente)) _nodoPendiente = NodosPendientes.FirstOrDefault()?.Id ?? 0;
            if (_panelPendienteId is not null && NodoPendiente?.Paneles.Any(p => p.Id == _panelPendienteId) != true)
                _panelPendienteId = null;
            // Aplicamos la selección al cargar el catálogo para que la primera conexión sea automática.
            if (NodoPendiente is not null)
                await ConfigurarSeleccionAsync(_parquePendiente, _nodoPendiente);
            else await Servicio.VaciarSeleccionAsync(_acceso, SesionId, _cancelacionPagina.Token);
        }
        catch (Exception ex) when (!_cancelacionPagina.IsCancellationRequested) { _error = Mensaje(ex); }
        finally { _cargando = false; }
    }
    private void CambiarParque(ChangeEventArgs args)
    {
        if (_cargando || !int.TryParse(args.Value?.ToString(), out var id) || !_catalogo.Any(p => p.Id == id)) return;
        _parquePendiente = id;
        _nodoPendiente = NodosPendientes.FirstOrDefault()?.Id ?? 0;
        RestablecerPanelPendiente();
    }
    private void CambiarNodo(ChangeEventArgs args)
    {
        if (_cargando || !int.TryParse(args.Value?.ToString(), out var id) || !NodosPendientes.Any(n => n.Id == id)) return;
        _nodoPendiente = id;
        RestablecerPanelPendiente();
    }
    // Al preparar otro nodo se limpia el selector de panel y se conserva el filtro de la gráfica activa.
    private void RestablecerPanelPendiente()
    {
        _panelPendienteId = null;
        if (SeleccionCoincideConActiva) SeleccionarPanel(null);
    }
    private void CambiarPanel(ChangeEventArgs args)
    {
        if (_cargando) return;
        int? id = int.TryParse(args.Value?.ToString(), out var valor) ? valor : null;
        if (id is not null && NodoPendiente?.Paneles.Any(p => p.Id == id) != true) return;
        _panelPendienteId = id;
        if (SeleccionCoincideConActiva) SeleccionarPanel(id);
    }
    private void SeleccionarPanel(int? id)
    {
        if (id is not null && NodoActivo?.Paneles.Any(p => p.Id == id) != true) return;
        _panelId = id;
        if (SeleccionCoincideConActiva) _panelPendienteId = id;
        _serie = _vistaConfirmada?.Serie(id) ?? [];
    }
    private async Task AplicarAsync()
    {
        if (!PuedeConectar) return;
        _cargando = true;
        _error = null;
        try { await ConfigurarSeleccionAsync(_parquePendiente, _nodoPendiente); }
        catch (Exception ex) when (!_cancelacionPagina.IsCancellationRequested) { _error = Mensaje(ex); }
        finally { _cargando = false; }
    }
    private async Task CambiarHabilitacionAsync()
    {
        if (PanelVisible is null || _guardando || _cargando) return;
        var panel = PanelVisible;
        _guardando = true;
        _error = null;
        try { await Servicio.CambiarHabilitacionAsync(_acceso, SesionId, panel.Id, !panel.Deshabilitado, _cancelacionPagina.Token); }
        catch (Exception ex) when (!_cancelacionPagina.IsCancellationRequested) { _error = Mensaje(ex); }
        finally { _guardando = false; }
    }
    private static string Mensaje(Exception ex) => ex is TimeoutException
        ? ErrorConfirmacion
        : ex is ArgumentException ? ex.Message :
        "No se pudo completar la operación. Verificá la conexión y el acceso de administrador.";
    public async ValueTask DisposeAsync()
    {
        if (_destruida) return;
        _destruida = true;
        // Cancelamos la escucha y esperamos su salida antes de liberar la señal de cancelación.
        await _cancelacionPagina.CancelAsync();
        await Servicio.CerrarAsync(_acceso, SesionId);
        if (_tareaLectura is not null) await _tareaLectura;
        _cancelacionPagina.Dispose();
    }
}
