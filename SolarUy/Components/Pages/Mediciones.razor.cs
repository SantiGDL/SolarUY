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
    private readonly CancellationTokenSource _vida = new();
    private AccesoProduccion _acceso = new("", false);
    private ParqueProduccion[] _catalogo = [];
    private VistaProduccion? _vista;
    private VistaProduccion? _estado;
    private int _parquePendiente;
    private int _nodoPendiente;
    private int? _panelPendienteId;
    private int? _panelId;
    private bool _cargando = true;
    private bool _guardando;
    private bool _destruida;
    private string? _error;
    private Task? _lector;
    private TaskCompletionSource<bool>? _confirmacion;
    private (int ParqueId, int NodoId)? _seleccionSolicitada;
    private PuntoGeneracion[] _serie = [];
    private NodoProduccion? NodoActivo => _vista?.Nodo;
    private PanelProduccion? PanelVisible => NodoActivo?.Paneles.FirstOrDefault(p => p.Id == _panelId);
    private bool Conectado => _estado?.Conectado == true && _estado.Confirmado;
    private bool HayCambiosPendientes => _vista?.Nodo is not null &&
        (_vista.ParqueId != _parquePendiente || _vista.Nodo.Id != _nodoPendiente);
    private bool ControlesDeshabilitados => _cargando || _catalogo.Length == 0;
    private IReadOnlyList<NodoProduccion> NodosPendientes => _catalogo.FirstOrDefault(p => p.Id == _parquePendiente)?.Nodos ?? [];
    private NodoProduccion? NodoPendiente => NodosPendientes.FirstOrDefault(n => n.Id == _nodoPendiente);
    private bool SeleccionCoincideConActiva => _vista?.ParqueId == _parquePendiente && NodoActivo?.Id == _nodoPendiente;
    private bool PuedeConectar => !ControlesDeshabilitados && NodoPendiente is not null;
    private IEnumerable<PanelProduccion> PanelesVisibles => (NodoActivo?.Paneles ?? [])
        .Where(p => _panelId is null || p.Id == _panelId);
    private string NombreParqueActivo => _catalogo.FirstOrDefault(p => p.Id == _vista?.ParqueId)?.Nombre ?? "";
    private double PotenciaVisible => _vista?.Potencia(_panelId) ?? 0;
    private double GeneracionVisible => _vista?.Generacion(_panelId) ?? 0;
    private static string Formato(double valor) => valor.ToString("N0");
    private static string ClaseEstado(string estado) => estado switch
    {
        "Intermedio" => "estado-intermedio", "Rotura" => "estado-rotura", _ => ""
    };
    private string ValorPanel(int id) => _vista?.Paneles.FirstOrDefault(p => p.Panel.Id == id)?.Valor is { } valor
        ? $"{Formato(valor)} W" : "Esperando mediciones";

    protected override async Task OnInitializedAsync()
    {
        _acceso = AccesoProduccionActual.Desde((await Autenticacion.GetAuthenticationStateAsync()).User);
        _lector = LeerAsync();
        await CargarAsync();
    }
    private async Task LeerAsync()
    {
        try
        {
            await foreach (var vista in Servicio.Observar(_acceso, SesionId).ReadAllAsync(_vida.Token))
                await InvokeAsync(() =>
                {
                    if (_destruida) return;
                    _estado = vista;
                    if (vista.Nodo is null)
                    {
                        _vista = null;
                        _serie = [];
                        _panelId = null;
                    }
                    if (vista.Confirmado && vista.Nodo is not null)
                    {
                        var cambiaNodo = _vista?.Version != vista.Version;
                        if (cambiaNodo || (_panelId is not null &&
                            !vista.Nodo.Paneles.Any(p => p.Id == _panelId))) _panelId = null;
                        _vista = vista;
                        _serie = vista.Serie(_panelId);
                        if (!vista.SincronizacionPendiente && _error == ErrorConfirmacion) _error = null;
                        // Una notificación del activo no sobrescribe controles de otro nodo preparado.
                        if (SeleccionCoincideConActiva && (cambiaNodo ||
                            (_panelPendienteId is not null && !vista.Nodo.Paneles.Any(p => p.Id == _panelPendienteId))))
                            _panelPendienteId = null;
                        if (_seleccionSolicitada == (vista.ParqueId, vista.Nodo.Id))
                            _confirmacion?.TrySetResult(true);
                    }
                    StateHasChanged();
                });
        }
        catch (OperationCanceledException) when (_vida.IsCancellationRequested) { }
    }
    private async Task ConfigurarSeleccionAsync(int parqueId, int nodoId)
    {
        _seleccionSolicitada = (parqueId, nodoId);
        var confirmacion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _confirmacion = confirmacion;
        try
        {
            await Servicio.ConfigurarAsync(_acceso, SesionId, parqueId, nodoId, _vida.Token);
            if (_estado is { Confirmado: true, SincronizacionPendiente: false } &&
                _estado.ParqueId == parqueId && _estado.Nodo?.Id == nodoId)
                confirmacion.TrySetResult(true);
            await confirmacion.Task.WaitAsync(TimeSpan.FromSeconds(20), _vida.Token);
        }
        finally { _confirmacion = null; _seleccionSolicitada = null; }
    }
    private async Task CargarAsync()
    {
        _cargando = true;
        _error = null;
        try
        {
            _catalogo = (await Servicio.ObtenerCatalogoAsync(_acceso, _vida.Token)).ToArray();
            if (!_catalogo.Any(p => p.Id == _parquePendiente)) _parquePendiente = _catalogo.FirstOrDefault()?.Id ?? 0;
            if (!NodosPendientes.Any(n => n.Id == _nodoPendiente)) _nodoPendiente = NodosPendientes.FirstOrDefault()?.Id ?? 0;
            if (_panelPendienteId is not null && NodoPendiente?.Paneles.Any(p => p.Id == _panelPendienteId) != true)
                _panelPendienteId = null;
            if (NodoPendiente is not null)
                await ConfigurarSeleccionAsync(_parquePendiente, _nodoPendiente);
            else await Servicio.VaciarSeleccionAsync(_acceso, SesionId, _vida.Token);
        }
        catch (Exception ex) when (!_vida.IsCancellationRequested) { _error = Mensaje(ex); }
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
        _serie = _vista?.Serie(id) ?? [];
    }
    private async Task AplicarAsync()
    {
        if (!PuedeConectar) return;
        _cargando = true;
        _error = null;
        try { await ConfigurarSeleccionAsync(_parquePendiente, _nodoPendiente); }
        catch (Exception ex) when (!_vida.IsCancellationRequested) { _error = Mensaje(ex); }
        finally { _cargando = false; }
    }
    private async Task CambiarHabilitacionAsync()
    {
        if (PanelVisible is null || _guardando || _cargando) return;
        var panel = PanelVisible;
        _guardando = true;
        _error = null;
        try { await Servicio.CambiarHabilitacionAsync(_acceso, SesionId, panel.Id, !panel.Deshabilitado, _vida.Token); }
        catch (Exception ex) when (!_vida.IsCancellationRequested) { _error = Mensaje(ex); }
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
        await _vida.CancelAsync();
        await Servicio.CerrarAsync(_acceso, SesionId);
        if (_lector is not null) await _lector;
        _vida.Dispose();
    }
}
