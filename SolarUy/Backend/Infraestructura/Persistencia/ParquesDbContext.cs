using Microsoft.EntityFrameworkCore;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

namespace SolarUy.Backend.Infraestructura.Persistencia;

public sealed class ParquesDbContext(DbContextOptions<ParquesDbContext> options)
    : DbContext(options)
{
    public DbSet<Parque> Parques => Set<Parque>();

    public DbSet<Nodo> Nodos => Set<Nodo>();

    public DbSet<Panel> Paneles => Set<Panel>();

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

        var nodo = modelBuilder.Entity<Nodo>();
        nodo.ToTable("Nodos");
        nodo.HasKey(item => item.Id);
        nodo.Property(item => item.Id).ValueGeneratedOnAdd();
        nodo.Property(item => item.ParqueId).IsRequired();
        nodo.Property(item => item.Nombre).HasMaxLength(100).IsRequired();
        nodo.Property(item => item.UbicacionDentroParque)
            .HasMaxLength(250)
            .IsRequired();
        nodo.HasOne<Parque>()
            .WithMany()
            .HasForeignKey(item => item.ParqueId)
            .OnDelete(DeleteBehavior.Restrict);
        nodo.HasIndex(item => item.ParqueId);

        var panel = modelBuilder.Entity<Panel>();
        panel.ToTable("Paneles");
        panel.HasKey(item => item.Id);
        panel.Property(item => item.Id).ValueGeneratedOnAdd();
        panel.Property(item => item.Estado).HasConversion<int>().IsRequired();
        panel.Property(item => item.PotenciaActual).IsRequired();
        panel.Property(item => item.Orientacion).HasMaxLength(50).IsRequired();
        panel.Property(item => item.NodoId).IsRequired();
        panel.HasOne<Nodo>()
            .WithMany()
            .HasForeignKey(item => item.NodoId)
            .OnDelete(DeleteBehavior.Restrict);
        panel.HasIndex(item => item.NodoId);
    }
}
