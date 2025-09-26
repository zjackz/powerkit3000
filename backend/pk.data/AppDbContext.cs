using pk.data.Models;
using Microsoft.EntityFrameworkCore;

namespace pk.data;

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

    public DbSet<AmazonCategory> AmazonCategories { get; set; }
    public DbSet<AmazonProduct> AmazonProducts { get; set; }
    public DbSet<AmazonSnapshot> AmazonSnapshots { get; set; }
    public DbSet<AmazonProductDataPoint> AmazonProductDataPoints { get; set; }
    public DbSet<AmazonTrend> AmazonTrends { get; set; }
    public DbSet<AmazonTask> AmazonTasks { get; set; }
    public DbSet<AmazonOperationalSnapshot> AmazonOperationalSnapshots { get; set; }
    public DbSet<AmazonProductOperationalMetric> AmazonProductOperationalMetrics { get; set; }


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

        modelBuilder.Entity<AmazonCategory>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
            entity.HasIndex(c => c.AmazonCategoryId).IsUnique();

            entity.Property(c => c.Name).HasMaxLength(200);
            entity.Property(c => c.AmazonCategoryId).HasMaxLength(50);

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AmazonProduct>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasMaxLength(10);
            entity.Property(p => p.Title).HasMaxLength(500);
            entity.Property(p => p.Brand).HasMaxLength(200);
            entity.Property(p => p.ImageUrl).HasMaxLength(1000);

            entity.HasIndex(p => new { p.CategoryId, p.Title });

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AmazonSnapshot>(entity =>
        {
            entity.Property(s => s.BestsellerType).HasMaxLength(50);
            entity.Property(s => s.Status).HasMaxLength(50);
            entity.Property(s => s.ErrorMessage).HasMaxLength(2000);

            entity.HasIndex(s => new { s.CategoryId, s.BestsellerType, s.CapturedAt });

            entity.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AmazonProductDataPoint>(entity =>
        {
            entity.HasIndex(p => new { p.ProductId, p.CapturedAt });
            entity.Property(p => p.Price).HasColumnType("numeric(12,2)");

            entity.HasOne(p => p.Product)
                .WithMany(p => p.DataPoints)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Snapshot)
                .WithMany(s => s.DataPoints)
                .HasForeignKey(p => p.SnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AmazonTrend>(entity =>
        {
            entity.Property(t => t.TrendType).HasMaxLength(50);
            entity.Property(t => t.Description).HasMaxLength(1000);

            entity.HasIndex(t => new { t.SnapshotId, t.TrendType });

            entity.HasOne(t => t.Product)
                .WithMany(p => p.Trends)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Snapshot)
                .WithMany(s => s.Trends)
                .HasForeignKey(t => t.SnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AmazonTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Name).IsUnique();

            entity.Property(t => t.Name).HasMaxLength(120);
            entity.Property(t => t.Site).HasMaxLength(100);
            entity.Property(t => t.ProxyPolicy).HasMaxLength(100);
            entity.Property(t => t.Status).HasMaxLength(50);

            entity.Property(t => t.CategoriesJson).HasColumnType("jsonb");
            entity.Property(t => t.LeaderboardsJson).HasColumnType("jsonb");
            entity.Property(t => t.PriceRangeJson).HasColumnType("jsonb");
            entity.Property(t => t.KeywordsJson).HasColumnType("jsonb");
            entity.Property(t => t.FiltersJson).HasColumnType("jsonb");
            entity.Property(t => t.ScheduleJson).HasColumnType("jsonb");
            entity.Property(t => t.LimitsJson).HasColumnType("jsonb");

            entity.Property(t => t.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(t => t.UpdatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<AmazonOperationalSnapshot>(entity =>
        {
            entity.Property(s => s.Status).HasMaxLength(50);
            entity.Property(s => s.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(s => s.CapturedAt);
        });

        modelBuilder.Entity<AmazonProductOperationalMetric>(entity =>
        {
            entity.HasIndex(m => new { m.ProductId, m.CapturedAt });
            entity.Property(m => m.BuyBoxPrice).HasColumnType("numeric(12,2)");
            entity.Property(m => m.InventoryDays).HasColumnType("numeric(10,2)");
            entity.Property(m => m.LatestNegativeReviewExcerpt).HasMaxLength(2000);
            entity.Property(m => m.LatestNegativeReviewUrl).HasMaxLength(1000);

            entity.HasOne(m => m.OperationalSnapshot)
                .WithMany(s => s.ProductMetrics)
                .HasForeignKey(m => m.OperationalSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Product)
                .WithMany(p => p.OperationalMetrics)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
