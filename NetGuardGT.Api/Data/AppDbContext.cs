using NetGuardGT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace NetGuardGT.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<Incidente> Incidentes => Set<Incidente>();
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tecnico>()
            .Property(t => t.Especialidad)
            .HasConversion<string>();

        modelBuilder.Entity<Incidente>()
            .Property(i => i.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Incidente>()
            .Property(i => i.Severidad)
            .HasConversion<string>();

        modelBuilder.Entity<Incidente>()
            .Property(i => i.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<HistorialEstado>()
            .Property(h => h.EstadoAnterior)
            .HasConversion<string>();

        modelBuilder.Entity<HistorialEstado>()
            .Property(h => h.EstadoNuevo)
            .HasConversion<string>();

        modelBuilder.Entity<Incidente>()
            .HasOne(i => i.Tecnico)
            .WithMany(t => t.Incidentes)
            .HasForeignKey(i => i.TecnicoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HistorialEstado>()
            .HasOne(h => h.Incidente)
            .WithMany(i => i.Historial)
            .HasForeignKey(h => h.IncidenteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
