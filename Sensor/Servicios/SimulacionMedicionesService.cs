using Sensor.Contratos.Mediciones;

namespace Sensor.Servicios;

public sealed record EntregaSimulada(EntregaMedicionesDto Datos, double Total);
public sealed record PuntoGeneracion(DateTime FechaHora, double ValorGenerado);

public sealed class SimulacionMedicionesService : IAsyncDisposable
{
    private CancellationTokenSource? _cancelacion;
    private Task _ciclo = Task.CompletedTask;
    private int _parqueId;
    private int _nodoId;
    private PanelMedicionesDto[] _paneles = [];
    private readonly List<EntregaSimulada> _muestras = [];

    public PuntoGeneracion[] ObtenerSerie(int? panelId) => _muestras
        .Where(m => panelId is null || m.Datos.Lecturas.Any(l => l.PanelId == panelId))
        .Select(m => new PuntoGeneracion(m.Datos.Lecturas[0].FechaHora.LocalDateTime,
            panelId is null ? m.Total : m.Datos.Lecturas.First(l => l.PanelId == panelId).ValorGenerado)).ToArray();

    public double ObtenerPotencia(int? panelId) => _paneles.Where(p => panelId is null || p.Id == panelId)
        .Sum(p => p.PotenciaActual);

    public async Task AplicarAsync(int parqueId, NodoMedicionesDto nodo,
        Func<EntregaSimulada, CancellationToken, Task> entregar)
    {
        if (_cancelacion is not null && _parqueId == parqueId && _nodoId == nodo.Id)
        {
            _paneles = nodo.Paneles.ToArray();
            return;
        }
        await DetenerAsync();
        _muestras.Clear();
        _parqueId = parqueId;
        _nodoId = nodo.Id;
        _paneles = nodo.Paneles.ToArray();
        _cancelacion = new CancellationTokenSource();
        _ciclo = EjecutarAsync(entregar, _cancelacion.Token);
    }

    private async Task EjecutarAsync(Func<EntregaSimulada, CancellationToken, Task> entregar, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                var fecha = DateTimeOffset.UtcNow;
                var lecturas = _paneles.Select(p => new LecturaMedicionesDto(_parqueId, _nodoId, p.Id,
                    Random.Shared.NextDouble() * p.PotenciaActual, fecha)).ToArray();
                var entrega = new EntregaSimulada(new(_parqueId, _nodoId, lecturas),
                    lecturas.Sum(l => l.ValorGenerado));
                _muestras.Add(entrega);
                await entregar(entrega, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    public async Task DetenerAsync()
    {
        if (_cancelacion is null) return;
        await _cancelacion.CancelAsync();
        await _ciclo;
        _cancelacion.Dispose();
        _cancelacion = null;
    }
    public async ValueTask DisposeAsync()
    {
        await DetenerAsync();
        _muestras.Clear();
    }
}
