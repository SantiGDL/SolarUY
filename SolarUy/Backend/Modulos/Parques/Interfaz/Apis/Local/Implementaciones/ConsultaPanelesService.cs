using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Implementaciones;

public sealed class ConsultaPanelesService(IDbContextFactory<ParquesDbContext> contextFactory) : IConsultaPaneles
{
    public async Task<bool> ExisteAsync(int panelId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Paneles.AsNoTracking().AnyAsync(panel => panel.Id == panelId, cancellationToken);
    }
}
