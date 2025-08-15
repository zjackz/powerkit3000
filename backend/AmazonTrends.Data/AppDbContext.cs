using AmazonTrends.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AmazonTrends.Data;

/// <summary>
/// 数据库上下文类，负责与数据库进行交互。
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 定义实体对应的数据库表集合
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductDataPoint> ProductDataPoints { get; set; }
    public DbSet<DataCollectionRun> DataCollectionRuns { get; set; }
    public DbSet<AnalysisResult> AnalysisResults { get; set; }
    public DbSet<ProductTrend> ProductTrends { get; set; }

    /// <summary>
    /// 配置数据模型和它们之间的关系
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- 配置关系 -----

        // Identity apec-related configurations, if any, should be called after base.OnModelCreating(builder);
        // modelBuilder.Entity<ApplicationUser>().ToTable("Users", "security");
        // modelBuilder.Entity<IdentityRole>().ToTable("Roles", "security");
        // modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "security");
        // modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "security");
        // modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "security");
        // modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "security");
        // modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "security");

        // 配置 Category 的父子自引用关系
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)       // 一个分类有一个父分类
            .WithMany(c => c.SubCategories)      // 一个父分类可以有多个子分类
            .HasForeignKey(c => c.ParentCategoryId) // 外键是 ParentCategoryId
            .OnDelete(DeleteBehavior.Restrict);  // 不允许级联删除，防止意外删除整个分类树

        // 配置 Product 和 Category 之间的关系
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)             // 一个商品属于一个分类
            .WithMany(c => c.Products)           // 一个分类拥有多个商品
            .HasForeignKey(p => p.CategoryId);   // 外键是 CategoryId

        // 配置 ProductDataPoint 和 Product 之间的关系
        modelBuilder.Entity<ProductDataPoint>()
            .HasOne(pdp => pdp.Product)          // 一个数据点属于一个商品
            .WithMany(p => p.DataPoints)         // 一个商品拥有多个数据点
            .HasForeignKey(pdp => pdp.ProductId); // 外键是 ProductId

        // 配置 ProductDataPoint 和 DataCollectionRun 之间的关系
        modelBuilder.Entity<ProductDataPoint>()
            .HasOne(pdp => pdp.DataCollectionRun) // 一个数据点由一次采集任务生成
            .WithMany(dcr => dcr.DataPoints)      // 一次采集任务生成多个数据点
            .HasForeignKey(pdp => pdp.DataCollectionRunId); // 外键是 DataCollectionRunId

        // 配置 DataCollectionRun 和 Category 之间的关系
        modelBuilder.Entity<DataCollectionRun>()
            .HasOne(dcr => dcr.Category)          // 一次采集任务针对一个分类
            .WithMany()                           // Category 没有直接的 DataCollectionRuns 集合，因为是单向关系
            .HasForeignKey(dcr => dcr.CategoryId); // 外键是 CategoryId

        // 配置 AnalysisResult 和 DataCollectionRun 之间的关系
        modelBuilder.Entity<AnalysisResult>()
            .HasOne(ar => ar.DataCollectionRun)
            .WithMany()
            .HasForeignKey(ar => ar.DataCollectionRunId);

        // 配置 ProductTrend 和 AnalysisResult 之间的关系
        modelBuilder.Entity<ProductTrend>()
            .HasOne(pt => pt.AnalysisResult)
            .WithMany(ar => ar.Trends)
            .HasForeignKey(pt => pt.AnalysisResultId);

        // 配置 ProductTrend 和 Product 之间的关系
        modelBuilder.Entity<ProductTrend>()
            .HasOne(pt => pt.Product)
            .WithMany()
            .HasForeignKey(pt => pt.ProductId);

        // ----- 添加索引 -----

        // 为 ProductDataPoint 表的 ProductId 和 Timestamp 创建复合索引，以加速趋势查询
        modelBuilder.Entity<ProductDataPoint>()
            .HasIndex(pdp => new { pdp.ProductId, pdp.Timestamp });

        // 为 Category 表的 Name 字段创建唯一索引，确保分类名称不重复
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }
}
