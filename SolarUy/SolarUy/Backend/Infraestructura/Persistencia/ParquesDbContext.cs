using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
namespace SolarUy.Backend.Infraestructura.Persistencia;

public sealed class ParquesDbContext(DbContextOptions<ParquesDbContext> options) : DbContext(options)
{
    public DbSet<Parque> Parques => Set<Parque>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var parque = modelBuilder.Entity<Parque>();
        parque.ToTable("Parques");
        parque.HasKey(p => p.Id);
        parque.Property(p => p.Id).ValueGeneratedOnAdd();
        parque.Property(p => p.Nombre).HasMaxLength(Parque.LongitudMaximaNombre).IsRequired();
        parque.Property(p => p.Direccion).HasMaxLength(Parque.LongitudMaximaDireccion).IsRequired();
        parque.Property(p => p.NombreNormalizado).HasMaxLength(Parque.LongitudMaximaNombre)
            .UseCollation("utf8mb4_bin").IsRequired();
        parque.HasIndex(p => p.NombreNormalizado).IsUnique();
    }
}
