using HappyTools.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTools.Data;

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


    /// <summary>
    /// 配置数据模型和它们之间的关系
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 在这里配置你的模型关系
    }
}