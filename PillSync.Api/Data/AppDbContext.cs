using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PillSync.Api.Models;

namespace PillSync.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Сюда добавляются DbSet для согласованных моделей:
    // Medication, MedicalAnalysis, LabMeasurement.
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Analysis> Analyses => Set<Analysis>();
    public DbSet<LabMeasurement> LabMeasurements => Set<LabMeasurement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //настраиваем таблицы Identity.
        base.OnModelCreating(builder);

        // Многие препараты принадлежат одному пользователю.
        builder.Entity<Medication>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Многие анализы принадлежат одному пользователю.
        builder.Entity<Analysis>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // У одного анализа может быть много измерений.
        builder.Entity<LabMeasurement>()
            .HasOne<Analysis>()
            .WithMany()
            .HasForeignKey(x => x.AnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}