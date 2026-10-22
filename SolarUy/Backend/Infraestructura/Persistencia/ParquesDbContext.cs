using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Infraestructura.Persistencia;

public sealed class ParquesDbContext(DbContextOptions<ParquesDbContext> options)
    : DbContext(options)
{
    public DbSet<Parque> Parques => Set<Parque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var parque = modelBuilder.Entity<Parque>();

        parque.ToTable("Parques");
        parque.HasKey(item => item.Id);
        parque.Property(item => item.Id).ValueGeneratedOnAdd();
        parque.Property(item => item.Nombre).HasMaxLength(100).IsRequired();
        parque.Property(item => item.Direccion).HasMaxLength(250).IsRequired();
        parque.Property(item => item.NombreNormalizado)
            .HasMaxLength(100)
            .UseCollation("utf8mb4_bin")
            .IsRequired();
        parque.HasIndex(item => item.NombreNormalizado).IsUnique();
    }
}
