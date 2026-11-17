using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Sensor.Contratos.Mediciones;

namespace Sensor.Servicios;

public sealed class MedicionesCliente(NavigationManager navigation, SimulacionMedicionesService simulador) : IAsyncDisposable
{
    private HubConnection? _conexion;
    private Guid _sesionId;
    private bool _destruido;
    private readonly CancellationTokenSource _cancelacionCliente = new();
    private Task? _tareaLatidos;
    private Task? _tareaRecuperacion;

    public async Task IniciarAsync(Guid sesionId, CancellationToken cancelacionInicio)
    {
        if (_conexion is not null) return;
        _sesionId = sesionId;
        _conexion = new HubConnectionBuilder().WithUrl(navigation.ToAbsoluteUri("hubs/mediciones"))
            .WithAutomaticReconnect().Build();
        // Registramos los mensajes antes de conectar para recibir la configuración inicial.
        _conexion.On<ConfiguracionMotorDto>("Configurar", async configuracion =>
        {
            if (_destruido || configuracion.SesionId != _sesionId) return;
            await simulador.AplicarAsync(configuracion, EnviarEntregaAsync);
            // Confirmamos el estado que quedó aplicado en el simulador.
            await ConfirmarConfiguracionAsync();
        });
        _conexion.On<EstadoPanelMotorDto>("CambiarHabilitacionPanel", async estado =>
        {
            if (_destruido || estado.SesionId != _sesionId) return;
            simulador.CambiarHabilitacion(estado);
            await ConfirmarConfiguracionAsync();
        });
        _conexion.On<Guid>("Detener", async id =>
        {
            if (id == _sesionId) await simulador.DetenerAsync();
        });
        // Pausamos la generación hasta que el servidor vuelva a configurar el sensor.
        _conexion.Reconnecting += _ => simulador.DetenerAsync();
        _conexion.Closed += async _ =>
        {
            await simulador.DetenerAsync();
            // Si SignalR agotó la reconexión automática, seguimos intentando mientras viva el cliente.
            if (!_destruido && !_cancelacionCliente.IsCancellationRequested) _tareaRecuperacion = RecuperarConexionAsync(_cancelacionCliente.Token);
        };
        _conexion.Reconnected += _ => _conexion.InvokeAsync("RegistrarSensor", _sesionId, cancelacionInicio);
        // La página y el sensor arrancan por separado. Reintentamos si el servidor aún no está listo.
        while (!cancelacionInicio.IsCancellationRequested)
        {
            try
            {
                await _conexion.StartAsync(cancelacionInicio);
                await _conexion.InvokeAsync("RegistrarSensor", _sesionId, cancelacionInicio);
                _tareaLatidos = EnviarLatidosAsync(_cancelacionCliente.Token);
                return;
            }
            catch (Exception) when (!cancelacionInicio.IsCancellationRequested)
            {
                await _conexion.StopAsync(cancelacionInicio);
                await Task.Delay(TimeSpan.FromSeconds(3), cancelacionInicio);
            }
        }
    }
    private Task ConfirmarConfiguracionAsync() => simulador.Configuracion is { } configuracion
        ? _conexion!.InvokeAsync("ConfirmarConfiguracion", configuracion.SesionId, configuracion.Version, configuracion.Paneles)
        : Task.CompletedTask;
    private Task EnviarEntregaAsync(EntregaMotorDto entrega, CancellationToken cancelacion) =>
        _conexion!.InvokeAsync("RegistrarEntrega", entrega, cancelacion);
    // Mantiene viva la sesión aunque no haya paneles habilitados que generen entregas.
    private async Task EnviarLatidosAsync(CancellationToken cancelacion)
    {
        using var reloj = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await reloj.WaitForNextTickAsync(cancelacion))
            {
                if (_conexion?.State != HubConnectionState.Connected) continue;
                try { await _conexion.InvokeAsync("Latido", _sesionId, cancelacion); }
                catch (Exception) when (!cancelacion.IsCancellationRequested)
                {
                    await simulador.DetenerAsync();
                    try { await _conexion.InvokeAsync("RegistrarSensor", _sesionId, cancelacion); }
                    catch (Exception) when (!cancelacion.IsCancellationRequested) { }
                }
            }
        }
        catch (OperationCanceledException) when (cancelacion.IsCancellationRequested) { }
    }
    private async Task RecuperarConexionAsync(CancellationToken cancelacion)
    {
        try
        {
            while (!cancelacion.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancelacion);
                if (_conexion?.State != HubConnectionState.Disconnected) return;
                try
                {
                    await _conexion.StartAsync(cancelacion);
                    await _conexion.InvokeAsync("RegistrarSensor", _sesionId, cancelacion);
                    return;
                }
                catch (Exception) when (!cancelacion.IsCancellationRequested) { }
            }
        }
        catch (OperationCanceledException) when (cancelacion.IsCancellationRequested) { }
    }
    public async ValueTask DisposeAsync()
    {
        if (_destruido) return;
        _destruido = true;
        // Interrumpimos las esperas de conexión antes de liberar SignalR.
        await _cancelacionCliente.CancelAsync();
        if (_tareaLatidos is not null) await _tareaLatidos;
        if (_tareaRecuperacion is not null) await _tareaRecuperacion;
        await simulador.DetenerAsync();
        if (_conexion is not null) await _conexion.DisposeAsync();
        _cancelacionCliente.Dispose();
    }
}
