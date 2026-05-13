using JewelryStoreCashFlowTracker.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace JewelryStoreCashFlowTracker.Database;

/// <summary>
/// Entity Framework Core database context for the application.
/// Manages DaySession and Transaction entities with SQLite persistence.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<DaySession> DaySessions { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    /// <summary>
    /// Database file path - stored in application data folder.
    /// </summary>
    private static string DatabasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "JewelryStoreCashFlowTracker",
        "cashflow.db");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ensure directory exists
        var dbDirectory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure DaySession entity
        modelBuilder.Entity<DaySession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.InitialCash).HasPrecision(18, 2);
            entity.Property(e => e.CountedCash).HasPrecision(18, 2);
            entity.Property(e => e.Difference).HasPrecision(18, 2);
            entity.Property(e => e.OpenedAt).IsRequired();
            entity.HasIndex(e => e.Date).IsUnique();
        });

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            entity.HasIndex(e => e.DaySessionId);
            entity.HasIndex(e => e.IsDeleted);

            // Configure relationship with DaySession
            entity.HasOne(t => t.DaySession)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.DaySessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
