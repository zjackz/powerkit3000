using Microsoft.EntityFrameworkCore;

namespace AmazonTrends.Data;

/// <summary>
/// 数据库上下文类，负责与数据库进行交互。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 在这里定义你的数据模型
    // public DbSet<YourModel> YourModels { get; set; }

    /// <summary>
    /// 配置数据模型和它们之间的关系
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 在这里配置你的模型关系
    }
}