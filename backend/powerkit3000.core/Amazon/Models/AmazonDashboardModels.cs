using System;
using System.Collections.Generic;

namespace powerkit3000.core.Amazon.Models;

/// <summary>
/// 提供给前端展示的 Amazon 商品 DTO。
/// </summary>
public record AmazonProductDto(
    string Asin,
    string Title,
    string CategoryName,
    DateTime? ListingDate,
    int? LatestRank,
    decimal? LatestPrice,
    float? LatestRating,
    int? LatestReviewsCount,
    DateTime? LastUpdated);

/// <summary>
/// Amazon 仪表盘核心指标。
/// </summary>
public record AmazonCoreMetricsDto(
    long SnapshotId,
    DateTime CapturedAt,
    int TotalProducts,
    int TotalNewEntries,
    int TotalRankSurges,
    int TotalConsistentPerformers);

/// <summary>
/// Amazon 趋势展示模型。
/// </summary>
public record AmazonTrendDto(
    string Asin,
    string Title,
    Amazon.AmazonTrendType TrendType,
    string Description,
    DateTime RecordedAt);

/// <summary>
/// Amazon 商品历史数据点。
/// </summary>
public record AmazonProductHistoryPoint(DateTime Timestamp, int Rank, decimal? Price, float? Rating, int? ReviewsCount);

/// <summary>
/// 汇总报告对象，包含指标、趋势与文本。
/// </summary>
public record AmazonSnapshotReportDto(
    AmazonCoreMetricsDto CoreMetrics,
    IReadOnlyList<AmazonTrendDto> Trends,
    string ReportText);
