using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.core.Amazon.Models;
using pk.data;

namespace pk.core.Amazon.Services;

/// <summary>
/// 将快照与趋势结果整合成结构化报告，便于下游播报或审计。
/// </summary>
public class AmazonReportingService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AmazonReportingService> _logger;

    /// <summary>
    /// 初始化 <see cref="AmazonReportingService"/>。
    /// </summary>
    public AmazonReportingService(AppDbContext dbContext, ILogger<AmazonReportingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 构建指定快照的报告对象，若快照不存在则返回 null。
    /// </summary>
    /// <param name="snapshotId">快照主键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>包含核心指标、趋势集合与文本摘要的报告。</returns>
    public async Task<AmazonSnapshotReportDto?> BuildReportAsync(long snapshotId, CancellationToken cancellationToken)
    {
        var snapshot = await _dbContext.AmazonSnapshots
            .Include(s => s.Category)
            .Include(s => s.DataPoints)
                .ThenInclude(dp => dp.Product)
            .Include(s => s.Trends)
                .ThenInclude(t => t.Product)
            .FirstOrDefaultAsync(s => s.Id == snapshotId, cancellationToken)
            .ConfigureAwait(false);

        if (snapshot == null)
        {
            _logger.LogWarning("Snapshot {SnapshotId} not found when building report", snapshotId);
            return null;
        }

        var coreMetrics = new AmazonCoreMetricsDto(
            snapshot.Id,
            snapshot.CapturedAt,
            snapshot.DataPoints.Select(dp => dp.ProductId).Distinct().Count(),
            snapshot.Trends.Count(t => t.TrendType == Amazon.AmazonTrendType.NewEntry.ToString()),
            snapshot.Trends.Count(t => t.TrendType == Amazon.AmazonTrendType.RankSurge.ToString()),
            snapshot.Trends.Count(t => t.TrendType == Amazon.AmazonTrendType.ConsistentPerformer.ToString())
        );

        var orderedTrends = snapshot.Trends
            .OrderBy(t => t.TrendType)
            .ThenBy(t => t.Product.Title)
            .Select(t => new AmazonTrendDto(
                t.ProductId,
                t.Product.Title,
                Enum.Parse<Amazon.AmazonTrendType>(t.TrendType),
                t.Description,
                t.RecordedAt))
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"Amazon 类目 {snapshot.Category.Name} {snapshot.BestsellerType} 榜单分析报告");
        builder.AppendLine($"采集时间: {snapshot.CapturedAt:yyyy-MM-dd HH:mm:ss} UTC");
        builder.AppendLine("核心指标:");
        builder.AppendLine($"- 榜单产品数: {coreMetrics.TotalProducts}");
        builder.AppendLine($"- 新晋上榜: {coreMetrics.TotalNewEntries}");
        builder.AppendLine($"- 排名飙升: {coreMetrics.TotalRankSurges}");
        builder.AppendLine($"- 持续霸榜: {coreMetrics.TotalConsistentPerformers}");
        builder.AppendLine();

        foreach (var trendGroup in orderedTrends.GroupBy(t => t.TrendType))
        {
            // 不同分组展示成独立的标题段落，保留最多 10 条代表项。
            builder.AppendLine(trendGroup.Key switch
            {
                Amazon.AmazonTrendType.NewEntry => "[新晋上榜]",
                Amazon.AmazonTrendType.RankSurge => "[排名飙升]",
                Amazon.AmazonTrendType.ConsistentPerformer => "[持续霸榜]",
                _ => "[趋势]"
            });

            foreach (var trend in trendGroup.Take(10))
            {
                builder.AppendLine($"- {trend.Title} ({trend.Asin}) {trend.Description}");
            }

            builder.AppendLine();
        }

        return new AmazonSnapshotReportDto(coreMetrics, orderedTrends, builder.ToString());
    }
}
