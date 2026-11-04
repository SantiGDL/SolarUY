using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Sensor.Contratos.Mediciones;

namespace Sensor.Servicios;

public sealed class MedicionesCliente(NavigationManager navigation, SimulacionMedicionesService simulador) : IAsyncDisposable
{
    private HubConnection? _conexion;
    private Guid _sesionId;
    private bool _destruido;
    private readonly CancellationTokenSource _vida = new();
    private Task? _latidos;
    private Task? _recuperacion;
    public async Task IniciarAsync(Guid sesionId, CancellationToken ct)
    {
        if (_conexion is not null) return;
        _sesionId = sesionId;
        _conexion = new HubConnectionBuilder().WithUrl(navigation.ToAbsoluteUri("hubs/mediciones"))
            .WithAutomaticReconnect().Build();
        _conexion.On<ConfiguracionMotorDto>("Configurar", async config =>
        {
            if (_destruido || config.SesionId != _sesionId) return;
            await simulador.AplicarAsync(config, EnviarAsync);
            await ConfirmarAsync();
        });
        _conexion.On<EstadoPanelMotorDto>("CambiarHabilitacionPanel", async estado =>
        {
            if (_destruido || estado.SesionId != _sesionId) return;
            simulador.CambiarHabilitacion(estado);
            await ConfirmarAsync();
        });
        _conexion.On<Guid>("Detener", async id =>
        {
            if (id == _sesionId) await simulador.DetenerAsync();
        });
        _conexion.Reconnecting += _ => simulador.DetenerAsync();
        _conexion.Closed += async _ =>
        {
            await simulador.DetenerAsync();
            if (!_destruido && !_vida.IsCancellationRequested) _recuperacion = RecuperarAsync(_vida.Token);
        };
        _conexion.Reconnected += _ => _conexion.InvokeAsync("RegistrarSensor", _sesionId, ct);
        // El fallo de inicio puede ser transitorio mientras se establece el documento.
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _conexion.StartAsync(ct);
                await _conexion.InvokeAsync("RegistrarSensor", _sesionId, ct);
                _latidos = MantenerSesionAsync(_vida.Token);
                return;
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                await _conexion.StopAsync(ct);
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }
    private Task ConfirmarAsync() => simulador.Configuracion is { } c
        ? _conexion!.InvokeAsync("ConfirmarConfiguracion", c.SesionId, c.Version, c.Paneles)
        : Task.CompletedTask;
    private Task EnviarAsync(EntregaMotorDto entrega, CancellationToken ct) =>
        _conexion!.InvokeAsync("RegistrarEntrega", entrega, ct);
    private async Task MantenerSesionAsync(CancellationToken ct)
    {
        using var reloj = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await reloj.WaitForNextTickAsync(ct))
            {
                if (_conexion?.State != HubConnectionState.Connected) continue;
                try { await _conexion.InvokeAsync("Latido", _sesionId, ct); }
                catch (Exception) when (!ct.IsCancellationRequested)
                {
                    await simulador.DetenerAsync();
                    try { await _conexion.InvokeAsync("RegistrarSensor", _sesionId, ct); }
                    catch (Exception) when (!ct.IsCancellationRequested) { }
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
    private async Task RecuperarAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
                if (_conexion?.State != HubConnectionState.Disconnected) return;
                try
                {
                    await _conexion.StartAsync(ct);
                    await _conexion.InvokeAsync("RegistrarSensor", _sesionId, ct);
                    return;
                }
                catch (Exception) when (!ct.IsCancellationRequested) { }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
    public async ValueTask DisposeAsync()
    {
        if (_destruido) return;
        _destruido = true;
        await _vida.CancelAsync();
        if (_latidos is not null) await _latidos;
        if (_recuperacion is not null) await _recuperacion;
        await simulador.DetenerAsync();
        if (_conexion is not null) await _conexion.DisposeAsync();
        _vida.Dispose();
    }
}
