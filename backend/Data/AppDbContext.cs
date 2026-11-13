using FixItAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FixItAI.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TroubleshootingSession> Sessions => Set<TroubleshootingSession>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TroubleshootingSession>(entity => {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DeviceCategory).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DeviceName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(100);
            entity.Property(x => x.Model).HasMaxLength(100);
            entity.Property(x => x.ProblemDescription).HasMaxLength(2000).IsRequired();
        });
    }
}
