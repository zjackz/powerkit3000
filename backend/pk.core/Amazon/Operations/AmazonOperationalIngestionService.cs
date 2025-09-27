using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.data;
using pk.data.Models;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 将采集到的运营指标写入数据库。
/// </summary>
public class AmazonOperationalIngestionService
{
    private readonly AppDbContext _dbContext;
    private readonly IAmazonOperationalDataSource _dataSource;
    private readonly ILogger<AmazonOperationalIngestionService> _logger;

    /// <summary>
    /// 初始化 <see cref="AmazonOperationalIngestionService"/>。
    /// </summary>
    public AmazonOperationalIngestionService(
        AppDbContext dbContext,
        IAmazonOperationalDataSource dataSource,
        ILogger<AmazonOperationalIngestionService> logger)
    {
        _dbContext = dbContext;
        _dataSource = dataSource;
        _logger = logger;
    }

    /// <summary>
    /// 触发一次运营数据采集并落库，返回生成的快照 ID。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>新生成的运营快照主键。</returns>
    public async Task<long> IngestAsync(CancellationToken cancellationToken)
    {
        var batch = await _dataSource.FetchAsync(cancellationToken).ConfigureAwait(false);
        var snapshot = new AmazonOperationalSnapshot
        {
            CapturedAt = batch.CapturedAt,
            SourceSnapshotId = batch.SourceSnapshotId,
            Status = "InProgress"
        };

        await _dbContext.AmazonOperationalSnapshots.AddAsync(snapshot, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var metricAsins = batch.Metrics.Select(m => m.Asin).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var existingProductIds = metricAsins.Length == 0
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(await _dbContext.AmazonProducts
                    .AsNoTracking()
                    .Where(p => metricAsins.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false), StringComparer.OrdinalIgnoreCase);

            foreach (var metric in batch.Metrics)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!existingProductIds.Contains(metric.Asin))
                {
                    _logger.LogDebug("Skipping operational metric for ASIN {Asin} because product does not exist", metric.Asin);
                    continue;
                }

                var entity = new AmazonProductOperationalMetric
                {
                    OperationalSnapshotId = snapshot.Id,
                    ProductId = metric.Asin,
                    CapturedAt = batch.CapturedAt,
                    InventoryQuantity = metric.InventoryQuantity,
                    InventoryDays = metric.InventoryDays,
                    UnitsSold7d = metric.UnitsSold7d,
                    IsStockout = metric.IsStockout,
                    NegativeReviewCount = metric.NegativeReviewCount,
                    LatestNegativeReviewAt = metric.LatestNegativeReviewAt,
                    LatestNegativeReviewExcerpt = metric.LatestNegativeReviewExcerpt,
                    LatestNegativeReviewUrl = metric.LatestNegativeReviewUrl,
                    BuyBoxPrice = metric.BuyBoxPrice,
                    LatestPriceUpdatedAt = metric.LatestPriceUpdatedAt
                };

                await _dbContext.AmazonProductOperationalMetrics.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            }

            snapshot.Status = "Completed";
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return snapshot.Id;
        }
        catch (Exception ex)
        {
            snapshot.Status = "Failed";
            snapshot.ErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "Failed to ingest Amazon operational metrics");
            throw;
        }
    }
}
