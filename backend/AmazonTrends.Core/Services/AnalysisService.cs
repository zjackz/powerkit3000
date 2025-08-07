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
    /// 分析指定数据采集运行的结果，发现热销趋势和潜力新品。
    /// </summary>
    /// <param name="dataCollectionRunId">要分析的数据采集运行ID。</param>
    /// <returns>包含趋势分析结果的列表。</returns>
    public async Task<List<ProductTrend>> AnalyzeTrendsAsync(long dataCollectionRunId)
    {
        _logger.LogInformation("开始分析数据采集运行 {DataCollectionRunId} 的产品趋势。", dataCollectionRunId);

        var run = await _dbContext.DataCollectionRuns.FindAsync(dataCollectionRunId);
        if (run == null)
        {
            _logger.LogWarning("未找到 ID 为 {DataCollectionRunId} 的数据采集运行记录。", dataCollectionRunId);
            return new List<ProductTrend>();
        }

        var trends = new List<ProductTrend>();

        // 获取本次运行抓取到的所有产品的数据点
        var currentDataPoints = await _dbContext.ProductDataPoints
            .Where(p => p.DataCollectionRunId == dataCollectionRunId)
            .ToListAsync();

        foreach (var currentDataPoint in currentDataPoints)
        {
            // 获取该产品上一次的数据点（不包括本次运行）
            var previousDataPoint = await _dbContext.ProductDataPoints
                .Where(p => p.ProductId == currentDataPoint.ProductId && p.DataCollectionRunId != dataCollectionRunId)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();

            if (previousDataPoint == null)
            {
                // 这是该产品第一次被抓取，标记为新上榜
                trends.Add(new ProductTrend
                {
                    ProductId = currentDataPoint.ProductId,
                    TrendType = "NewEntry",
                    Description = $"新产品首次进入榜单，当前排名 {currentDataPoint.Rank}。"
                });
                continue;
            }

            // 排名飙升 (Rank Surge)
            if (currentDataPoint.Rank < previousDataPoint.Rank)
            {
                trends.Add(new ProductTrend
                {
                    ProductId = currentDataPoint.ProductId,
                    TrendType = "RankSurge",
                    Description = $"排名从 {previousDataPoint.Rank} 上升到 {currentDataPoint.Rank}，上升了 {previousDataPoint.Rank - currentDataPoint.Rank} 位。"
                });
            }

            // 持续霸榜 (Consistent Performer)
            if (currentDataPoint.Rank <= 100 && previousDataPoint.Rank <= 100)
            {
                trends.Add(new ProductTrend
                {
                    ProductId = currentDataPoint.ProductId,
                    TrendType = "ConsistentPerformer",
                    Description = $"持续在 Top 100 榜单内，当前排名 {currentDataPoint.Rank}。"
                });
            }
        }

        // 注意：这里的分析结果可以存入数据库或直接返回
        // 为了简单起见，我们暂时只记录日志并返回
        _logger.LogInformation("数据采集运行 {DataCollectionRunId} 的趋势分析完成，发现 {Count} 条趋势。", dataCollectionRunId, trends.Count);
        
        // 可以在这里将分析结果存入数据库
        // var analysisResult = new AnalysisResult { ... };
        // _dbContext.AnalysisResults.Add(analysisResult);
        // await _dbContext.SaveChangesAsync();

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
