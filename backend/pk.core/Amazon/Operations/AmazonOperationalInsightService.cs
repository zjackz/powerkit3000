using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pk.core.Amazon.Options;
using pk.data;
using pk.data.Models;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 提供亚马逊运营仪表盘所需的数据聚合与严重度判定。
/// </summary>
public class AmazonOperationalInsightService
{
    private readonly AppDbContext _dbContext;
    private readonly AmazonOperationalDashboardOptions _options;
    private readonly ILogger<AmazonOperationalInsightService> _logger;

    public AmazonOperationalInsightService(
        AppDbContext dbContext,
        IOptions<AmazonOperationalDashboardOptions> options,
        ILogger<AmazonOperationalInsightService> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AmazonOperationalSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var snapshot = await GetLatestSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (snapshot == null)
        {
            _logger.LogInformation("No Amazon operational snapshot available when querying summary.");
            return BuildEmptySummary();
        }

        var metrics = await _dbContext.AmazonProductOperationalMetrics
            .AsNoTracking()
            .Where(m => m.OperationalSnapshotId == snapshot.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var lowStockCounts = CountIssues(metrics, EvaluateLowStockSeverity);
        var negativeReviewCounts = CountIssues(metrics, EvaluateNegativeReviewSeverity);
        var isStale = IsStale(snapshot.CapturedAt);

        return new AmazonOperationalSummaryDto(
            snapshot.CapturedAt,
            isStale,
            lowStockCounts,
            negativeReviewCounts,
            BuildAdPlaceholder());
    }

    public async Task<AmazonOperationalIssuesResult> GetIssuesAsync(AmazonOperationalIssueQuery query, CancellationToken cancellationToken)
    {
        var snapshot = await GetLatestSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (snapshot == null)
        {
            _logger.LogInformation("No Amazon operational snapshot available when querying issues.");
            return new AmazonOperationalIssuesResult(null, false, Array.Empty<AmazonOperationalIssueDto>(), 0, BuildAdPlaceholder());
        }

        var metrics = await _dbContext.AmazonProductOperationalMetrics
            .AsNoTracking()
            .Where(m => m.OperationalSnapshotId == snapshot.Id)
            .Include(m => m.Product)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var issues = new List<AmazonOperationalIssueDto>();

        foreach (var metric in metrics)
        {
            if (metric.Product == null)
            {
                continue;
            }

            var lowStockIssue = CreateLowStockIssue(metric);
            if (lowStockIssue != null)
            {
                issues.Add(lowStockIssue);
            }

            var negativeReviewIssue = CreateNegativeReviewIssue(metric);
            if (negativeReviewIssue != null)
            {
                issues.Add(negativeReviewIssue);
            }
        }

        var filtered = ApplyFilters(issues, query);
        var total = filtered.Count;

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var pagedItems = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new AmazonOperationalIssuesResult(
            snapshot.CapturedAt,
            IsStale(snapshot.CapturedAt),
            pagedItems,
            total,
            BuildAdPlaceholder());
    }

    private async Task<AmazonOperationalSnapshot?> GetLatestSnapshotAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.AmazonOperationalSnapshots
            .AsNoTracking()
            .OrderByDescending(s => s.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private AmazonOperationalSummaryDto BuildEmptySummary()
    {
        return new AmazonOperationalSummaryDto(
            null,
            false,
            new AmazonOperationalIssueSummary(0, 0, 0, 0),
            new AmazonOperationalIssueSummary(0, 0, 0, 0),
            BuildAdPlaceholder());
    }

    private AmazonOperationalPlaceholderDto BuildAdPlaceholder()
        => new("comingSoon", "广告浪费分析开发中，敬请期待。");

    private bool IsStale(DateTime capturedAt)
        => DateTime.UtcNow - capturedAt > _options.DataStaleAfter;

    private AmazonOperationalIssueSummary CountIssues(
        IEnumerable<AmazonProductOperationalMetric> metrics,
        Func<AmazonProductOperationalMetric, AmazonOperationalSeverity?> severityResolver)
    {
        var totals = new Dictionary<AmazonOperationalSeverity, int>
        {
            [AmazonOperationalSeverity.High] = 0,
            [AmazonOperationalSeverity.Medium] = 0,
            [AmazonOperationalSeverity.Low] = 0
        };

        foreach (var metric in metrics)
        {
            var severity = severityResolver(metric);
            if (severity == null)
            {
                continue;
            }

            totals[severity.Value]++;
        }

        var total = totals.Values.Sum();
        return new AmazonOperationalIssueSummary(
            total,
            totals[AmazonOperationalSeverity.High],
            totals[AmazonOperationalSeverity.Medium],
            totals[AmazonOperationalSeverity.Low]);
    }

    private static List<AmazonOperationalIssueDto> ApplyFilters(
        IEnumerable<AmazonOperationalIssueDto> issues,
        AmazonOperationalIssueQuery query)
    {
        var filtered = issues;

        if (query.IssueType.HasValue)
        {
            filtered = filtered.Where(i => i.IssueType == query.IssueType.Value);
        }

        if (query.Severity.HasValue)
        {
            filtered = filtered.Where(i => i.Severity == query.Severity.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filtered = filtered.Where(i => i.Asin.Contains(term, StringComparison.OrdinalIgnoreCase) || i.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return filtered
            .OrderByDescending(i => i.Severity)
            .ThenBy(i => i.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private AmazonOperationalIssueDto? CreateLowStockIssue(AmazonProductOperationalMetric metric)
    {
        var severity = EvaluateLowStockSeverity(metric);
        if (severity == null || metric.Product == null)
        {
            return null;
        }

        var recommendation = severity switch
        {
            AmazonOperationalSeverity.High => $"立即补货，库存天数约 {FormatInventoryDays(metric.InventoryDays)}，必要时暂停广告投入。",
            AmazonOperationalSeverity.Medium => $"建议补货，库存天数约 {FormatInventoryDays(metric.InventoryDays)}，请与供应链确认。",
            _ => $"留意库存变化，当前库存天数约 {FormatInventoryDays(metric.InventoryDays)}。"
        };

        return new AmazonOperationalIssueDto(
            metric.ProductId,
            metric.Product.Title,
            AmazonOperationalIssueType.LowStock,
            severity.Value,
            BuildKpi(metric),
            recommendation,
            metric.CapturedAt);
    }

    private AmazonOperationalIssueDto? CreateNegativeReviewIssue(AmazonProductOperationalMetric metric)
    {
        var severity = EvaluateNegativeReviewSeverity(metric);
        if (severity == null || metric.Product == null)
        {
            return null;
        }

        var window = _options.NegativeReviewWindowDays;
        var recommendation = severity switch
        {
            AmazonOperationalSeverity.High => $"近 {window} 天出现 {metric.NegativeReviewCount} 条差评，请立即回复并评估原因。",
            AmazonOperationalSeverity.Medium => $"近 {window} 天出现 {metric.NegativeReviewCount} 条差评，建议安排回复并收集客户反馈。",
            _ => $"近 {window} 天出现 {metric.NegativeReviewCount} 条差评，保持关注用户反馈。"
        };

        return new AmazonOperationalIssueDto(
            metric.ProductId,
            metric.Product.Title,
            AmazonOperationalIssueType.NegativeReview,
            severity.Value,
            BuildKpi(metric),
            recommendation,
            metric.CapturedAt);
    }

    private AmazonOperationalIssueKpi BuildKpi(AmazonProductOperationalMetric metric) => new(
        metric.InventoryDays,
        metric.InventoryQuantity,
        metric.UnitsSold7d,
        metric.IsStockout,
        metric.NegativeReviewCount,
        metric.LatestNegativeReviewAt,
        metric.LatestNegativeReviewExcerpt,
        metric.LatestNegativeReviewUrl,
        metric.BuyBoxPrice);

    private AmazonOperationalSeverity? EvaluateLowStockSeverity(AmazonProductOperationalMetric metric)
    {
        if (metric.IsStockout == true || (metric.InventoryQuantity.HasValue && metric.InventoryQuantity.Value <= 0))
        {
            return AmazonOperationalSeverity.High;
        }

        if (metric.InventoryDays == null)
        {
            return null;
        }

        if (metric.InventoryDays <= 0)
        {
            return AmazonOperationalSeverity.High;
        }

        var threshold = _options.InventoryThresholdDays;
        if (threshold <= 0)
        {
            return null;
        }

        var highBoundary = Math.Max(1, (int)Math.Floor(threshold * (double)_options.InventoryHighSeverityFactor));

        if (metric.InventoryDays <= highBoundary)
        {
            return AmazonOperationalSeverity.High;
        }

        if (metric.InventoryDays < threshold)
        {
            return AmazonOperationalSeverity.Medium;
        }

        return null;
    }

    private AmazonOperationalSeverity? EvaluateNegativeReviewSeverity(AmazonProductOperationalMetric metric)
    {
        if (metric.NegativeReviewCount <= 0)
        {
            return null;
        }

        if (metric.LatestNegativeReviewAt.HasValue)
        {
            var windowStart = DateTime.UtcNow.AddDays(-_options.NegativeReviewWindowDays);
            if (metric.LatestNegativeReviewAt < windowStart)
            {
                return null;
            }
        }

        var mediumThreshold = Math.Max(1, _options.NegativeReviewMediumCount);
        var highThreshold = Math.Max(mediumThreshold, _options.NegativeReviewHighCount);

        if (metric.NegativeReviewCount >= highThreshold)
        {
            return AmazonOperationalSeverity.High;
        }

        if (metric.NegativeReviewCount >= mediumThreshold)
        {
            return AmazonOperationalSeverity.Medium;
        }

        return AmazonOperationalSeverity.Low;
    }

    private static string FormatInventoryDays(decimal? inventoryDays)
    {
        if (inventoryDays == null)
        {
            return "未知";
        }

        return inventoryDays.Value < 1 ? "不足 1 天" : Math.Round(inventoryDays.Value, 1).ToString("0.0");
    }
}
