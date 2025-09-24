using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.data;
using pk.data.Models;

namespace pk.core.Amazon.Services;

/// <summary>
/// 负责解析 Amazon 榜单快照，生成“新晋上榜”“排名飙升”“持续霸榜”等趋势标签。
/// </summary>
public class AmazonTrendAnalysisService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AmazonTrendAnalysisService> _logger;

    private const int RankSurgeThreshold = 10;

    public AmazonTrendAnalysisService(AppDbContext dbContext, ILogger<AmazonTrendAnalysisService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 对指定快照重新计算趋势标签，返回生成的趋势数量。
    /// </summary>
    public async Task<int> AnalyzeSnapshotAsync(long snapshotId, CancellationToken cancellationToken)
    {
        var snapshot = await _dbContext.AmazonSnapshots
            .Include(s => s.DataPoints)
            .ThenInclude(dp => dp.Product)
            .FirstOrDefaultAsync(s => s.Id == snapshotId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Amazon snapshot {snapshotId} not found.");

        var existing = await _dbContext.AmazonTrends
            .Where(t => t.SnapshotId == snapshotId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing.Count > 0)
        {
            // 保证幂等：重新分析前先移除旧的趋势记录。
            _dbContext.AmazonTrends.RemoveRange(existing);
        }

        var trends = snapshot.DataPoints.SelectMany(dp => AnalyzeDataPoint(dp, snapshot.CapturedAt, cancellationToken)).ToList();

        if (trends.Count > 0)
        {
            await _dbContext.AmazonTrends.AddRangeAsync(trends, cancellationToken).ConfigureAwait(false);
        }

        snapshot.Status = "Analyzed";
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Analyzed snapshot {SnapshotId} and produced {TrendCount} trends", snapshotId, trends.Count);
        return trends.Count;
    }

    /// <summary>
    /// 基于当前数据点与历史比较结果产生 0~N 条趋势信息。
    /// </summary>
    private IEnumerable<AmazonTrend> AnalyzeDataPoint(AmazonProductDataPoint currentDataPoint, DateTime snapshotTime, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trends = new List<AmazonTrend>();

        var previousDataPoint = _dbContext.AmazonProductDataPoints
            .AsNoTracking()
            .Where(p => p.ProductId == currentDataPoint.ProductId && p.CapturedAt < snapshotTime)
            .OrderByDescending(p => p.CapturedAt)
            .FirstOrDefault();

        if (previousDataPoint == null)
        {
            // 没有历史记录，表示刚进入榜单。
            trends.Add(new AmazonTrend
            {
                ProductId = currentDataPoint.ProductId,
                SnapshotId = currentDataPoint.SnapshotId,
                TrendType = Amazon.AmazonTrendType.NewEntry.ToString(),
                Description = $"首次进入榜单，当前排名 {currentDataPoint.Rank}",
                RecordedAt = snapshotTime
            });
            return trends;
        }

        if (currentDataPoint.Rank > 0 && previousDataPoint.Rank > 0)
        {
            var delta = previousDataPoint.Rank - currentDataPoint.Rank;
            if (delta >= RankSurgeThreshold)
            {
                // 排名提升超过阈值，判定为排名飙升。
                trends.Add(new AmazonTrend
                {
                    ProductId = currentDataPoint.ProductId,
                    SnapshotId = currentDataPoint.SnapshotId,
                    TrendType = Amazon.AmazonTrendType.RankSurge.ToString(),
                    Description = $"排名从 {previousDataPoint.Rank} 升至 {currentDataPoint.Rank}，提升 {delta} 名", 
                    RecordedAt = snapshotTime
                });
            }

            if (currentDataPoint.Rank <= 100 && previousDataPoint.Rank <= 100)
            {
                // 连续位于 Top100，标记为持续霸榜。
                trends.Add(new AmazonTrend
                {
                    ProductId = currentDataPoint.ProductId,
                    SnapshotId = currentDataPoint.SnapshotId,
                    TrendType = Amazon.AmazonTrendType.ConsistentPerformer.ToString(),
                    Description = $"持续保持 Top 100，当前排名 {currentDataPoint.Rank}",
                    RecordedAt = snapshotTime
                });
            }
        }

        return trends;
    }
}
