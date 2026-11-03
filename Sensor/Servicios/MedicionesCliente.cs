using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Sensor.Contratos.Mediciones;

namespace Sensor.Servicios;

public sealed class MedicionesCliente(NavigationManager navigation) : IAsyncDisposable
{
    private HubConnection? _conexion;
    private bool _destruido;
    public bool Conectado => _conexion?.State == HubConnectionState.Connected;
    public event Func<Task>? EstadoCambiado;
    public event Func<Task>? Reconectado;

    public async Task ConectarAsync(CancellationToken cancellationToken)
    {
        if (_conexion is null)
        {
            _conexion = new HubConnectionBuilder().WithUrl(navigation.ToAbsoluteUri("hubs/mediciones"))
                .WithAutomaticReconnect().Build();
            _conexion.Reconnecting += _ => NotificarAsync();
            _conexion.Closed += _ => NotificarAsync();
            _conexion.Reconnected += async _ =>
            {
                await NotificarAsync();
                if (Reconectado is not null) await Reconectado();
            };
        }
        if (_conexion.State == HubConnectionState.Disconnected)
            await _conexion.StartAsync(cancellationToken);
        await NotificarAsync();
    }

    public Task<ParqueMedicionesDto[]> ObtenerCatalogoAsync(CancellationToken ct) =>
        _conexion!.InvokeAsync<ParqueMedicionesDto[]>("ObtenerCatalogo", ct);
    public Task<NodoMedicionesDto> ValidarAsync(SeleccionMedicionesDto seleccion, CancellationToken ct) =>
        _conexion!.InvokeAsync<NodoMedicionesDto>("ValidarSeleccion", seleccion, ct);
    public Task<bool> GuardarAsync(EntregaMedicionesDto entrega, CancellationToken ct) =>
        _conexion!.InvokeAsync<bool>("RegistrarEntrega", entrega, ct);
    private Task NotificarAsync() => EstadoCambiado?.Invoke() ?? Task.CompletedTask;
    public async ValueTask DisposeAsync()
    {
        if (_destruido) return;
        _destruido = true;
        if (_conexion is not null) await _conexion.DisposeAsync();
    }
}
