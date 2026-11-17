using Sensor.Contratos.Mediciones;

namespace Sensor.Servicios;

public sealed class SimulacionMedicionesService : IAsyncDisposable
{
    private ConfiguracionMotorDto? _configuracion;
    private CancellationTokenSource? _cancelacionCiclo;
    private Task? _ciclo;
    // Configurar y detener esperan su turno para que exista un solo ciclo.
    private readonly SemaphoreSlim _controlConfiguracion = new(1, 1);
    public ConfiguracionMotorDto? Configuracion => _configuracion;

    public async Task AplicarAsync(ConfiguracionMotorDto configuracion,
        Func<EntregaMotorDto, CancellationToken, Task> enviar)
    {
        await _controlConfiguracion.WaitAsync();
        try
        {
            var configuracionAnterior = _configuracion;
            if (configuracionAnterior is not null && configuracion.Version < configuracionAnterior.Version) return;

            var mismoParque = configuracionAnterior?.ParqueId == configuracion.ParqueId;
            var mismoNodo = mismoParque && configuracionAnterior?.NodoId == configuracion.NodoId;
            if (configuracionAnterior is not null && configuracionAnterior.Version == configuracion.Version)
            {
                // Una configuración repetida puede llegar después de un cambio de habilitación.
                var paneles = configuracion.Paneles.Select(panel =>
                {
                    var panelAnterior = configuracionAnterior.Paneles.FirstOrDefault(p => p.Id == panel.Id);
                    return panelAnterior?.RevisionEstado > panel.RevisionEstado ? panelAnterior : panel;
                }).ToArray();
                configuracion = configuracion with { Paneles = paneles };
            }

            // Esperamos el fin del ciclo anterior antes de generar para otro nodo.
            if (!mismoNodo) await DetenerCicloAsync();
            _configuracion = configuracion;
            if (_ciclo is null)
            {
                _cancelacionCiclo = new CancellationTokenSource();
                _ciclo = GenerarMedicionesAsync(enviar, _cancelacionCiclo.Token);
            }
        }
        finally { _controlConfiguracion.Release(); }
    }

    public void CambiarHabilitacion(EstadoPanelMotorDto estado)
    {
        var configuracion = _configuracion;
        if (configuracion is null || configuracion.SesionId != estado.SesionId ||
            configuracion.Version != estado.Version) return;

        var paneles = configuracion.Paneles.Select(panel =>
        {
            if (panel.Id != estado.PanelId || estado.RevisionEstado < panel.RevisionEstado) return panel;
            return panel with { Deshabilitado = estado.Deshabilitado, RevisionEstado = estado.RevisionEstado };
        }).ToArray();
        _configuracion = configuracion with { Paneles = paneles };
    }

    private async Task GenerarMedicionesAsync(Func<EntregaMotorDto, CancellationToken, Task> enviar,
        CancellationToken cancelacionCiclo)
    {
        using var reloj = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await reloj.WaitForNextTickAsync(cancelacionCiclo))
            {
                // Todas las lecturas del tick salen de la misma configuración.
                var configuracion = _configuracion;
                if (configuracion is null) continue;
                var fecha = DateTimeOffset.UtcNow;
                var lecturas = configuracion.Paneles.Where(p => !p.Deshabilitado).Select(p =>
                    new LecturaMotorDto(p.Id, p.RevisionEstado, Random.Shared.NextDouble() * p.PotenciaActual, fecha)).ToArray();
                if (lecturas.Length == 0) continue;
                var entrega = new EntregaMotorDto(configuracion.SesionId, Guid.NewGuid(),
                    configuracion.Version, configuracion.ParqueId, configuracion.NodoId, lecturas);
                try
                {
                    await enviar(entrega, cancelacionCiclo);
                }
                catch (Exception) when (!cancelacionCiclo.IsCancellationRequested)
                {
                    // No se reenvían entregas inciertas ni se inventa historia local.
                    // El próximo tick conserva la cadencia y genera una muestra nueva.
                }
            }
        }
        catch (OperationCanceledException) when (cancelacionCiclo.IsCancellationRequested) { }
    }

    private async Task DetenerCicloAsync()
    {
        if (_cancelacionCiclo is null) return;
        // Interrumpimos la espera del temporizador o del envío antes de liberar el ciclo.
        await _cancelacionCiclo.CancelAsync();
        if (_ciclo is not null) await _ciclo;
        _cancelacionCiclo.Dispose();
        _cancelacionCiclo = null;
        _ciclo = null;
    }
    public async Task DetenerAsync()
    {
        // El cierre espera su turno aunque ya se haya cancelado el ciclo.
        await _controlConfiguracion.WaitAsync();
        try { await DetenerCicloAsync(); }
        finally { _controlConfiguracion.Release(); }
    }
    public ValueTask DisposeAsync() => new(DetenerAsync());
}
