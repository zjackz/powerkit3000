using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.core.Amazon.Models;
using pk.data;

namespace pk.core.Amazon.Services;

/// <summary>
/// 提供 Amazon 仪表盘所需的数据查询，统一封装核心指标、榜单列表和趋势信息。
/// </summary>
public class AmazonDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly AmazonReportingService _reportingService;
    private readonly ILogger<AmazonDashboardService> _logger;

    public AmazonDashboardService(AppDbContext dbContext, AmazonReportingService reportingService, ILogger<AmazonDashboardService> logger)
    {
        _dbContext = dbContext;
        _reportingService = reportingService;
        _logger = logger;
    }

    /// <summary>
    /// 获取最新快照的核心指标，若尚未采集快照则返回 null。
    /// </summary>
    public async Task<AmazonCoreMetricsDto?> GetLatestCoreMetricsAsync(CancellationToken cancellationToken)
    {
        var latestSnapshot = await _dbContext.AmazonSnapshots
            .OrderByDescending(s => s.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (latestSnapshot == null)
        {
            return null;
        }

        var result = await _reportingService.BuildReportAsync(latestSnapshot.Id, cancellationToken).ConfigureAwait(false);
        return result?.CoreMetrics;
    }

    /// <summary>
    /// 查询 Amazon 榜单商品，自动匹配其最新快照数据，可按类目与关键字筛选。
    /// </summary>
    public async Task<IReadOnlyList<AmazonProductDto>> GetProductsAsync(int? categoryId, string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _dbContext.AmazonProducts
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p => p.Title.Contains(term) || p.Id.Contains(term));
        }

        var products = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        var productIds = products.Select(p => p.Id).ToArray();

        var latestSnapshotTimes = await _dbContext.AmazonProductDataPoints
            .AsNoTracking()
            .Where(dp => productIds.Contains(dp.ProductId))
            .GroupBy(dp => dp.ProductId)
            .Select(g => new { ProductId = g.Key, LatestCapturedAt = g.Max(dp => dp.CapturedAt) })
            .ToDictionaryAsync(x => x.ProductId, x => x.LatestCapturedAt, cancellationToken)
            .ConfigureAwait(false);

        var latestDataPoints = await _dbContext.AmazonProductDataPoints
            .AsNoTracking()
            .Where(dp => productIds.Contains(dp.ProductId) && latestSnapshotTimes.ContainsKey(dp.ProductId) && dp.CapturedAt == latestSnapshotTimes[dp.ProductId])
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dataPointLookup = latestDataPoints.ToDictionary(dp => dp.ProductId, dp => dp);

        return products.Select(p =>
        {
            dataPointLookup.TryGetValue(p.Id, out var dp);
            return new AmazonProductDto(
                p.Id,
                p.Title,
                p.Category.Name,
                p.ListingDate,
                dp?.Rank,
                dp?.Price,
                dp?.Rating,
                dp?.ReviewsCount,
                dp?.CapturedAt);
        }).ToList();
    }

    /// <summary>
    /// 查询最新快照的趋势列表，可选地按趋势类型过滤。
    /// </summary>
    public async Task<IReadOnlyList<AmazonTrendDto>> GetLatestTrendsAsync(Amazon.AmazonTrendType? trendType, CancellationToken cancellationToken)
    {
        var latestSnapshotId = await _dbContext.AmazonSnapshots
            .OrderByDescending(s => s.CapturedAt)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (latestSnapshotId == 0)
        {
            return Array.Empty<AmazonTrendDto>();
        }

        var query = _dbContext.AmazonTrends
            .AsNoTracking()
            .Include(t => t.Product)
            .Where(t => t.SnapshotId == latestSnapshotId);

        if (trendType.HasValue)
        {
            var trendTypeValue = trendType.Value.ToString();
            query = query.Where(t => t.TrendType == trendTypeValue);
        }

        var trends = await query
            .OrderBy(t => t.TrendType)
            .ThenBy(t => t.Product.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return trends
            .Select(t => new AmazonTrendDto(
                t.ProductId,
                t.Product.Title,
                Enum.Parse<Amazon.AmazonTrendType>(t.TrendType),
                t.Description,
                t.RecordedAt))
            .ToList();
    }

    /// <summary>
    /// 获取某个 ASIN 在历史快照中的排名、价格等信息，按时间升序返回。
    /// </summary>
    public async Task<IReadOnlyList<AmazonProductHistoryPoint>> GetProductHistoryAsync(string asin, CancellationToken cancellationToken)
    {
        var history = await _dbContext.AmazonProductDataPoints
            .AsNoTracking()
            .Where(dp => dp.ProductId == asin)
            .OrderBy(dp => dp.CapturedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return history
            .Select(dp => new AmazonProductHistoryPoint(dp.CapturedAt, dp.Rank, dp.Price, dp.Rating, dp.ReviewsCount))
            .ToList();
    }

    /// <summary>
    /// 返回最新快照的完整报告数据，若没有快照则返回 null。
    /// </summary>
    public async Task<AmazonSnapshotReportDto?> GetLatestReportAsync(CancellationToken cancellationToken)
    {
        var latestSnapshotId = await _dbContext.AmazonSnapshots
            .OrderByDescending(s => s.CapturedAt)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (latestSnapshotId == 0)
        {
            return null;
        }

        return await _reportingService.BuildReportAsync(latestSnapshotId, cancellationToken).ConfigureAwait(false);
    }
}
