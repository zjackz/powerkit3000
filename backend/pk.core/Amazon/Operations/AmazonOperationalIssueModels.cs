using System;
using System.Collections.Generic;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 查询运营问题时的请求过滤条件。
/// </summary>
public sealed record AmazonOperationalIssueQuery(
    AmazonOperationalIssueType? IssueType = null,
    AmazonOperationalSeverity? Severity = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// 单条运营问题的指标信息。
/// </summary>
public sealed record AmazonOperationalIssueKpi(
    decimal? InventoryDays,
    int? InventoryQuantity,
    int? UnitsSold7d,
    bool? IsStockout,
    int NegativeReviewCount,
    DateTime? LatestNegativeReviewAt,
    string? LatestNegativeReviewExcerpt,
    string? LatestNegativeReviewUrl,
    decimal? BuyBoxPrice);

/// <summary>
/// 运营问题详情。
/// </summary>
public sealed record AmazonOperationalIssueDto(
    string Asin,
    string Title,
    AmazonOperationalIssueType IssueType,
    AmazonOperationalSeverity Severity,
    AmazonOperationalIssueKpi Kpi,
    string Recommendation,
    DateTime CapturedAt);

/// <summary>
/// 运营问题分页结果。
/// </summary>
public sealed record AmazonOperationalIssuesResult(
    DateTime? LastUpdatedAt,
    bool IsStale,
    IReadOnlyList<AmazonOperationalIssueDto> Items,
    int Total,
    AmazonOperationalPlaceholderDto AdWastePlaceholder);

/// <summary>
/// 仪表盘概要数据。
/// </summary>
public sealed record AmazonOperationalSummaryDto(
    DateTime? LastUpdatedAt,
    bool IsStale,
    AmazonOperationalIssueSummary LowStock,
    AmazonOperationalIssueSummary NegativeReview,
    AmazonOperationalPlaceholderDto AdWastePlaceholder);

/// <summary>
/// 单个问题类型的数量统计。
/// </summary>
public sealed record AmazonOperationalIssueSummary(int Total, int High, int Medium, int Low);

/// <summary>
/// 广告占位说明。
/// </summary>
public sealed record AmazonOperationalPlaceholderDto(string Status, string Message);

/// <summary>
/// 运营采集批次结果。
/// </summary>
public sealed record AmazonOperationalDataBatch(
    DateTime CapturedAt,
    long? SourceSnapshotId,
    IReadOnlyList<AmazonOperationalMetricRecord> Metrics)
{
    public static AmazonOperationalDataBatch Empty(DateTime capturedAt) => new(capturedAt, null, Array.Empty<AmazonOperationalMetricRecord>());
}

/// <summary>
/// 运营采集时单个 ASIN 的原始数据。
/// </summary>
public sealed record AmazonOperationalMetricRecord(
    string Asin,
    int? InventoryQuantity,
    decimal? InventoryDays,
    int? UnitsSold7d,
    bool? IsStockout,
    int NegativeReviewCount,
    DateTime? LatestNegativeReviewAt,
    string? LatestNegativeReviewExcerpt,
    string? LatestNegativeReviewUrl,
    decimal? BuyBoxPrice,
    DateTime? LatestPriceUpdatedAt);
