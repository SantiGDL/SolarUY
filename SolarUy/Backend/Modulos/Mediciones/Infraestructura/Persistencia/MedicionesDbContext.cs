using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Persistencia;

public sealed class MedicionesDbContext(DbContextOptions<MedicionesDbContext> options) : DbContext(options)
{
    public DbSet<Medicion> Mediciones => Set<Medicion>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var medicion = modelBuilder.Entity<Medicion>();
        medicion.ToTable("Mediciones");
        medicion.HasKey(m => m.Id);
        medicion.Property(m => m.Id).ValueGeneratedOnAdd();
        medicion.Property(m => m.FechaHora).HasConversion(
            fecha => fecha.UtcDateTime,
            fecha => new DateTimeOffset(DateTime.SpecifyKind(fecha, DateTimeKind.Utc)))
            .HasColumnType("datetime(6)");
        medicion.HasIndex(m => new { m.PanelId, m.FechaHora });
        medicion.HasIndex(m => new { m.ParqueId, m.NodoId, m.FechaHora });
    }
}
