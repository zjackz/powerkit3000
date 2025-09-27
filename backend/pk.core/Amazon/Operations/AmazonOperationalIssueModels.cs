using System;
using System.Collections.Generic;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 查询运营问题时的请求过滤条件。
/// </summary>
/// <param name="IssueType">指定问题类别，null 表示不过滤。</param>
/// <param name="Severity">指定严重度，null 表示不过滤。</param>
/// <param name="Search">按 ASIN/标题等关键字模糊匹配。</param>
/// <param name="Page">页码，从 1 开始。</param>
/// <param name="PageSize">每页记录数。</param>
public sealed record AmazonOperationalIssueQuery(
    AmazonOperationalIssueType? IssueType = null,
    AmazonOperationalSeverity? Severity = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// 单条运营问题的指标信息。
/// </summary>
/// <param name="InventoryDays">预计还能维持的库存天数。</param>
/// <param name="InventoryQuantity">当前库存数量。</param>
/// <param name="UnitsSold7d">近 7 天销量。</param>
/// <param name="IsStockout">是否缺货。</param>
/// <param name="NegativeReviewCount">近窗口期差评数量。</param>
/// <param name="LatestNegativeReviewAt">最新差评时间。</param>
/// <param name="LatestNegativeReviewExcerpt">差评摘要。</param>
/// <param name="LatestNegativeReviewUrl">差评链接。</param>
/// <param name="BuyBoxPrice">当前 BuyBox 价格。</param>
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
/// <param name="Asin">商品 ASIN。</param>
/// <param name="Title">商品标题。</param>
/// <param name="IssueType">问题类别。</param>
/// <param name="Severity">严重度。</param>
/// <param name="Kpi">关联指标。</param>
/// <param name="Recommendation">处理建议。</param>
/// <param name="CapturedAt">指标采集时间。</param>
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
/// <param name="LastUpdatedAt">上次采集时间。</param>
/// <param name="IsStale">数据是否陈旧。</param>
/// <param name="Items">问题列表。</param>
/// <param name="Total">总记录数。</param>
/// <param name="AdWastePlaceholder">广告占位说明。</param>
public sealed record AmazonOperationalIssuesResult(
    DateTime? LastUpdatedAt,
    bool IsStale,
    IReadOnlyList<AmazonOperationalIssueDto> Items,
    int Total,
    AmazonOperationalPlaceholderDto AdWastePlaceholder);

/// <summary>
/// 仪表盘概要数据。
/// </summary>
/// <param name="LastUpdatedAt">上次采集时间。</param>
/// <param name="IsStale">是否超过阈值被视为陈旧。</param>
/// <param name="LowStock">库存问题统计。</param>
/// <param name="NegativeReview">差评问题统计。</param>
/// <param name="AdWastePlaceholder">广告占位信息。</param>
public sealed record AmazonOperationalSummaryDto(
    DateTime? LastUpdatedAt,
    bool IsStale,
    AmazonOperationalIssueSummary LowStock,
    AmazonOperationalIssueSummary NegativeReview,
    AmazonOperationalPlaceholderDto AdWastePlaceholder);

/// <summary>
/// <summary>
/// 单个问题类型的数量统计。
/// </summary>
/// <param name="Total">总数。</param>
/// <param name="High">高危数量。</param>
/// <param name="Medium">中危数量。</param>
/// <param name="Low">低危数量。</param>
public sealed record AmazonOperationalIssueSummary(int Total, int High, int Medium, int Low);

/// <summary>
/// 广告占位说明。
/// </summary>
/// <param name="Status">占位状态。</param>
/// <param name="Message">说明文案。</param>
public sealed record AmazonOperationalPlaceholderDto(string Status, string Message);

/// <summary>
/// 运营采集批次结果。
/// </summary>
public sealed record AmazonOperationalDataBatch(
    DateTime CapturedAt,
    long? SourceSnapshotId,
    IReadOnlyList<AmazonOperationalMetricRecord> Metrics)
{
    /// <summary>
    /// 构造一个空的采集结果。
    /// </summary>
    /// <param name="capturedAt">记录采集时间。</param>
    public static AmazonOperationalDataBatch Empty(DateTime capturedAt) => new(capturedAt, null, Array.Empty<AmazonOperationalMetricRecord>());
}

/// <summary>
/// <summary>
/// 运营采集时单个 ASIN 的原始数据。
/// </summary>
/// <param name="Asin">ASIN 编号。</param>
/// <param name="InventoryQuantity">库存数量。</param>
/// <param name="InventoryDays">库存可支撑天数。</param>
/// <param name="UnitsSold7d">近 7 天销量。</param>
/// <param name="IsStockout">是否缺货。</param>
/// <param name="NegativeReviewCount">差评数量。</param>
/// <param name="LatestNegativeReviewAt">最新差评时间。</param>
/// <param name="LatestNegativeReviewExcerpt">差评摘录。</param>
/// <param name="LatestNegativeReviewUrl">差评链接。</param>
/// <param name="BuyBoxPrice">BuyBox 价格。</param>
/// <param name="LatestPriceUpdatedAt">价格信息更新时间。</param>
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
