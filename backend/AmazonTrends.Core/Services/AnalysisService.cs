using AmazonTrends.Data;
using AmazonTrends.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AmazonTrends.Core.Services;

public class AnalysisService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(AppDbContext dbContext, ILogger<AnalysisService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 分析产品数据点，发现热销趋势和潜力新品。
    /// </summary>
    /// <param name="categoryId">要分析的分类ID。</param>
    /// <returns>包含趋势分析结果的列表。</returns>
    public async Task<List<ProductTrend>> AnalyzeTrendsAsync(int categoryId)
    {
        _logger.LogInformation("开始分析分类 {CategoryId} 的产品趋势。", categoryId);

        var trends = new List<ProductTrend>();

        // 获取指定分类下的所有产品及其最近的数据点
        var products = await _dbContext.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.DataPoints.OrderByDescending(dp => dp.Timestamp).Take(2)) // 获取最近的两个数据点用于比较
            .ToListAsync();

        foreach (var product in products)
        {
            if (product.DataPoints.Count < 2)
            {
                // 数据点不足，无法进行趋势分析
                continue;
            }

            var latestDataPoint = product.DataPoints.OrderByDescending(dp => dp.Timestamp).First();
            var previousDataPoint = product.DataPoints.OrderByDescending(dp => dp.Timestamp).Skip(1).First();

            // 排名飙升 (Rank Surge)
            if (latestDataPoint.Rank < previousDataPoint.Rank)
            {
                trends.Add(new ProductTrend
                {
                    ProductId = product.Id,
                    ProductTitle = product.Title,
                    TrendType = "RankSurge",
                    Description = $"排名从 {previousDataPoint.Rank} 上升到 {latestDataPoint.Rank}，上升了 {previousDataPoint.Rank - latestDataPoint.Rank} 位。"
                });
            }

            // 新晋上榜 (New Entry - 假设之前不在榜单，现在进入)
            // 这里的逻辑需要更精确，可能需要检查更长时间的历史数据
            // 暂时简化为：如果之前排名很高（不在Top 100），现在进入Top 100
            if (previousDataPoint.Rank > 100 && latestDataPoint.Rank <= 100)
            {
                trends.Add(new ProductTrend
                {
                    ProductId = product.Id,
                    ProductTitle = product.Title,
                    TrendType = "NewEntry",
                    Description = $"新晋进入 Top 100 榜单，当前排名 {latestDataPoint.Rank}。"
                });
            }

            // 持续霸榜 (Consistent Performer)
            // 假设连续多次数据采集都在 Top 100 内
            // 这里需要更复杂的逻辑，例如检查过去7天或30天的数据点
            // 暂时简化为：如果两次数据采集都在 Top 100 内
            if (latestDataPoint.Rank <= 100 && previousDataPoint.Rank <= 100)
            {
                trends.Add(new ProductTrend
                {
                    ProductId = product.Id,
                    ProductTitle = product.Title,
                    TrendType = "ConsistentPerformer",
                    Description = $"持续在 Top 100 榜单内，当前排名 {latestDataPoint.Rank}。"
                });
            }
        }

        _logger.LogInformation("分类 {CategoryId} 的趋势分析完成，发现 {Count} 条趋势。", categoryId, trends.Count);
        return trends;
    }
}

/// <summary>
/// 用于表示产品趋势的 DTO。
/// </summary>
public class ProductTrend
{
    public string ProductId { get; set; } = null!;
    public string ProductTitle { get; set; } = null!;
    public string TrendType { get; set; } = null!; // 例如: RankSurge, NewEntry, ConsistentPerformer
    public string Description { get; set; } = null!;
    public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
}
