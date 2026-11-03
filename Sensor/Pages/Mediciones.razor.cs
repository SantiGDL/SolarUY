using Microsoft.AspNetCore.Components;
using Sensor.Contratos.Mediciones;
using Sensor.Servicios;

namespace Sensor.Pages;

public partial class Mediciones
{
    [Inject] private MedicionesCliente _cliente { get; set; } = default!;
    [Inject] private SimulacionMedicionesService _simulador { get; set; } = default!;
    private readonly CancellationTokenSource _vida = new();
    private ParqueMedicionesDto[] _catalogo = [];
    private int _parquePendiente;
    private int _nodoPendiente;
    private SeleccionMedicionesDto? _activa;
    private NodoMedicionesDto? _nodoActivo;
    private string _nombreParqueActivo = "";
    private EntregaSimulada? _entrega;
    private bool _cargando = true;
    private bool _destruida;
    private string? _error;
    private string _estadoGuardado = "Aún no hay entregas";
    private long _version;
    private PuntoGeneracion[] _serie = [];
    private bool HayCambiosPendientes => _activa is not null &&
        (_activa.ParqueId != _parquePendiente || _activa.NodoId != _nodoPendiente);
    private PanelMedicionesDto? PanelVisible => _nodoActivo?.Paneles.FirstOrDefault(p => p.Id == _activa?.PanelId);
    private bool ControlesDeshabilitados => _cargando || _catalogo.Length == 0;
    private NodoMedicionesDto[] NodosPendientes => _catalogo.FirstOrDefault(p => p.Id == _parquePendiente)?.Nodos ?? [];
    private IEnumerable<PanelMedicionesDto> PanelesVisibles => (_nodoActivo?.Paneles ?? []).Where(p => _activa?.PanelId is null || p.Id == _activa.PanelId);
    private double PotenciaVisible => _simulador.ObtenerPotencia(_activa?.PanelId);
    private double GeneracionVisible => _activa?.PanelId is null ? _entrega?.Total ?? 0 :
        _entrega?.Datos.Lecturas.FirstOrDefault(l => l.PanelId == _activa.PanelId)?.ValorGenerado ?? 0;
    private static string Formato(double valor) => valor.ToString("N0");
    private static string ClaseEstado(string estado) => estado switch
    {
        "Intermedio" => "estado-intermedio",
        "Rotura" => "estado-rotura",
        _ => ""
    };
    private string ValorPanel(int id) => _entrega?.Datos.Lecturas.FirstOrDefault(l => l.PanelId == id) is { } lectura
        ? $"{Formato(lectura.ValorGenerado)} W" : "Esperando mediciones";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _cliente.EstadoCambiado += ActualizarConexionAsync;
        _cliente.Reconectado += RevalidarAsync;
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        _cargando = true;
        _error = null;
        try
        {
            await _cliente.ConectarAsync(_vida.Token);
            _catalogo = await _cliente.ObtenerCatalogoAsync(_vida.Token);
            if (!_catalogo.Any(p => p.Id == _parquePendiente))
                _parquePendiente = _catalogo.FirstOrDefault()?.Id ?? 0;
            if (!NodosPendientes.Any(n => n.Id == _nodoPendiente))
                _nodoPendiente = NodosPendientes.FirstOrDefault()?.Id ?? 0;
            if (_catalogo.Length > 0)
                await ConfirmarAsync(_activa ?? new(_parquePendiente, _nodoPendiente, null));
            else
            {
                await _simulador.DetenerAsync();
                _activa = null;
                _entrega = null;
                _serie = [];
            }
        }
        catch (Exception ex) when (!_vida.IsCancellationRequested) { _error = Mensaje(ex); }
        finally { _cargando = false; if (!_destruida) StateHasChanged(); }
    }

    private void CambiarParque(ChangeEventArgs args)
    {
        _parquePendiente = int.Parse(args.Value!.ToString()!);
        _nodoPendiente = NodosPendientes.FirstOrDefault()?.Id ?? 0;
    }
    private void CambiarNodo(ChangeEventArgs args)
    {
        _nodoPendiente = int.Parse(args.Value!.ToString()!);
    }

    private void CambiarPanel(ChangeEventArgs args) => SeleccionarPanel(
        int.TryParse(args.Value?.ToString(), out var id) ? id : null);

    private void SeleccionarPanel(int? panelId)
    {
        if (_activa is null || (panelId is not null && !_nodoActivo!.Paneles.Any(p => p.Id == panelId))) return;
        _activa = _activa with { PanelId = panelId };
        _serie = _simulador.ObtenerSerie(panelId);
    }

    private async Task AplicarAsync()
    {
        if (ControlesDeshabilitados) return;
        _cargando = true;
        _error = null;
        try
        {
            await _cliente.ConectarAsync(_vida.Token);
            var panelId = HayCambiosPendientes ? null : _activa?.PanelId;
            await ConfirmarAsync(new(_parquePendiente, _nodoPendiente, panelId));
        }
        catch (Exception ex) when (!_vida.IsCancellationRequested) { _error = Mensaje(ex); }
        finally { _cargando = false; }
    }

    private async Task ConfirmarAsync(SeleccionMedicionesDto seleccion)
    {
        var nodo = await _cliente.ValidarAsync(seleccion, _vida.Token);
        var cambiaNodo = _activa?.ParqueId != seleccion.ParqueId || _activa?.NodoId != seleccion.NodoId;
        if (cambiaNodo)
        {
            await _simulador.DetenerAsync();
            _version++;
            _entrega = null;
            _serie = [];
            _estadoGuardado = "Esperando primera entrega";
        }
        _activa = seleccion;
        _nodoActivo = nodo;
        _nombreParqueActivo = _catalogo.FirstOrDefault(p => p.Id == seleccion.ParqueId)?.Nombre ?? $"Parque {seleccion.ParqueId}";
        var version = _version;
        await _simulador.AplicarAsync(seleccion.ParqueId, nodo,
            (entrega, ct) => RecibirAsync(entrega, version, ct));
        _serie = _simulador.ObtenerSerie(_activa.PanelId);
    }

    private async Task RecibirAsync(EntregaSimulada entrega, long version, CancellationToken ct)
    {
        if (_destruida || version != _version) return;
        await InvokeAsync(() =>
        {
            _entrega = entrega;
            _serie = _simulador.ObtenerSerie(_activa?.PanelId);
            _estadoGuardado = "Enviando mediciones…";
            StateHasChanged();
        });
        string estado;
        try
        {
            if (!_cliente.Conectado) throw new InvalidOperationException("Sin conexión: esta entrega no pudo guardarse.");
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct, _vida.Token);
            limite.CancelAfter(TimeSpan.FromSeconds(8));
            await _cliente.GuardarAsync(entrega.Datos, limite.Token);
            estado = "Entrega guardada";
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested || _vida.IsCancellationRequested) { return; }
        catch (Exception ex) { estado = $"No se confirmó el guardado: {Mensaje(ex)}"; }
        if (!_destruida && version == _version)
            await InvokeAsync(() => { _estadoGuardado = estado; StateHasChanged(); });
    }

    private Task ActualizarConexionAsync() => _destruida ? Task.CompletedTask : InvokeAsync(StateHasChanged);
    private Task RevalidarAsync() => InvokeAsync(async () =>
    {
        if (_destruida || _cargando || _activa is null) return;
        try { await ConfirmarAsync(_activa); }
        catch (Exception ex) when (!_vida.IsCancellationRequested) { _error = Mensaje(ex); }
        if (!_destruida) StateHasChanged();
    });
    private static string Mensaje(Exception ex) => ex is OperationCanceledException
        ? "La operación excedió el tiempo de espera. El resultado del guardado es incierto."
        : ex is Microsoft.AspNetCore.SignalR.HubException ? ex.Message
        : "No se pudo completar la operación. Verificá la conexión y el acceso de administrador.";

    public async ValueTask DisposeAsync()
    {
        if (_destruida) return;
        _destruida = true;
        await _vida.CancelAsync();
        _cliente.EstadoCambiado -= ActualizarConexionAsync;
        _cliente.Reconectado -= RevalidarAsync;
        await _simulador.DisposeAsync();
        await _cliente.DisposeAsync();
        _vida.Dispose();
    }
}
