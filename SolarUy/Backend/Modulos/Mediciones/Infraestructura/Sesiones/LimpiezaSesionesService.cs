using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Sesiones;

public sealed class LimpiezaSesionesService(IServiceScopeFactory scopes,
    ILogger<LimpiezaSesionesService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var reloj = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await reloj.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await using var scope = scopes.CreateAsyncScope();
                    await scope.ServiceProvider.GetRequiredService<MedicionesService>().ExpirarSesionesAsync();
                }
                catch (Exception ex) { logger.LogError(ex, "Falló la limpieza de sesiones de Producción."); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
