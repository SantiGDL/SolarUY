using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Mediciones.Infraestructura.Persistencia;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Repositorios;

public sealed class MedicionRepository(IDbContextFactory<MedicionesDbContext> factory) : IMedicionRepository
{
    public async Task AgregarEntregaAsync(IReadOnlyList<Medicion> mediciones, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        context.Mediciones.AddRange(mediciones);
        await context.SaveChangesAsync(cancellationToken);
    }
}
