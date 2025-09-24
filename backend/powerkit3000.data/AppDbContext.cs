using powerkit3000.data.Models;
using Microsoft.EntityFrameworkCore;

namespace powerkit3000.data;

/// <summary>
/// 数据库上下文类，负责与数据库进行交互。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<KickstarterProject> KickstarterProjects { get; set; }
    public DbSet<Creator> Creators { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProjectFavorite> ProjectFavorites { get; set; }


    /// <summary>
    /// 配置数据模型和它们之间的关系
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectFavorite>(entity =>
        {
            entity.HasIndex(f => f.ProjectId);
            entity.HasIndex(f => new { f.ClientId, f.ProjectId }).IsUnique();

            entity.Property(f => f.ClientId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(f => f.SavedAt)
                .HasColumnType("timestamp with time zone");

            entity.HasOne(f => f.Project)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
