using Sensor.Contratos.Mediciones;

namespace Sensor.Servicios;

public sealed class SimulacionMedicionesService : IAsyncDisposable
{
    private ConfiguracionMotorDto? _configuracion;
    private CancellationTokenSource? _cancelacion;
    private Task? _ciclo;
    private readonly SemaphoreSlim _control = new(1, 1);
    public ConfiguracionMotorDto? Configuracion => _configuracion;

    public async Task AplicarAsync(ConfiguracionMotorDto configuracion,
        Func<EntregaMotorDto, CancellationToken, Task> enviar)
    {
        await _control.WaitAsync();
        try
        {
            var anterior = _configuracion;
            if (anterior is not null && configuracion.Version < anterior.Version) return;
            var mismoNodo = anterior?.ParqueId == configuracion.ParqueId && anterior.NodoId == configuracion.NodoId;
            if (anterior?.Version == configuracion.Version)
                configuracion = configuracion with { Paneles = configuracion.Paneles.Select(p =>
                {
                    var previo = anterior.Paneles.FirstOrDefault(a => a.Id == p.Id);
                    return previo?.RevisionEstado > p.RevisionEstado ? previo : p;
                }).ToArray() };
            if (!mismoNodo) await DetenerCicloAsync();
            _configuracion = configuracion;
            if (_ciclo is null)
            {
                _cancelacion = new CancellationTokenSource();
                _ciclo = EjecutarAsync(enviar, _cancelacion.Token);
            }
        }
        finally { _control.Release(); }
    }

    public void CambiarHabilitacion(EstadoPanelMotorDto estado)
    {
        var c = _configuracion;
        if (c is null || c.SesionId != estado.SesionId || c.Version != estado.Version) return;
        _configuracion = c with { Paneles = c.Paneles.Select(p => p.Id == estado.PanelId &&
            estado.RevisionEstado >= p.RevisionEstado ? p with
            { Deshabilitado = estado.Deshabilitado, RevisionEstado = estado.RevisionEstado } : p).ToArray() };
    }

    private async Task EjecutarAsync(Func<EntregaMotorDto, CancellationToken, Task> enviar, CancellationToken ct)
    {
        using var reloj = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await reloj.WaitForNextTickAsync(ct))
            {
                var c = _configuracion;
                if (c is null) continue;
                var fecha = DateTimeOffset.UtcNow;
                var lecturas = c.Paneles.Where(p => !p.Deshabilitado).Select(p =>
                    new LecturaMotorDto(p.Id, p.RevisionEstado, Random.Shared.NextDouble() * p.PotenciaActual, fecha)).ToArray();
                if (lecturas.Length == 0) continue;
                try { await enviar(new(c.SesionId, Guid.NewGuid(), c.Version, c.ParqueId, c.NodoId, lecturas), ct); }
                catch (Exception) when (!ct.IsCancellationRequested)
                {
                    // No se reenvían entregas inciertas ni se inventa historia local.
                    // El próximo tick conserva la cadencia y genera una muestra nueva.
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    private async Task DetenerCicloAsync()
    {
        if (_cancelacion is null) return;
        await _cancelacion.CancelAsync();
        if (_ciclo is not null) await _ciclo;
        _cancelacion.Dispose();
        _cancelacion = null;
        _ciclo = null;
    }
    public async Task DetenerAsync()
    {
        await _control.WaitAsync();
        try { await DetenerCicloAsync(); }
        finally { _control.Release(); }
    }
    public ValueTask DisposeAsync() => new(DetenerAsync());
}
