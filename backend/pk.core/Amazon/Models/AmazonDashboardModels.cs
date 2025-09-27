using System;
using System.Collections.Generic;

namespace pk.core.Amazon.Models;

/// <summary>
/// <summary>
/// 提供给前端展示的 Amazon 商品 DTO。
/// </summary>
/// <param name="Asin">商品 ASIN。</param>
/// <param name="Title">商品标题。</param>
/// <param name="CategoryName">所属类目名称。</param>
/// <param name="ListingDate">上架日期。</param>
/// <param name="LatestRank">最新排名。</param>
/// <param name="LatestPrice">最新价格。</param>
/// <param name="LatestRating">最新评分。</param>
/// <param name="LatestReviewsCount">最新评论数。</param>
/// <param name="LastUpdated">数据更新时间。</param>
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
/// <summary>
/// Amazon 仪表盘核心指标。
/// </summary>
/// <param name="SnapshotId">快照主键。</param>
/// <param name="CapturedAt">采集时间。</param>
/// <param name="TotalProducts">榜单商品总数。</param>
/// <param name="TotalNewEntries">新晋上榜数量。</param>
/// <param name="TotalRankSurges">排名飙升数量。</param>
/// <param name="TotalConsistentPerformers">持续霸榜数量。</param>
public record AmazonCoreMetricsDto(
    long SnapshotId,
    DateTime CapturedAt,
    int TotalProducts,
    int TotalNewEntries,
    int TotalRankSurges,
    int TotalConsistentPerformers);

/// <summary>
/// <summary>
/// Amazon 趋势展示模型。
/// </summary>
/// <param name="Asin">商品 ASIN。</param>
/// <param name="Title">商品标题。</param>
/// <param name="TrendType">趋势类型。</param>
/// <param name="Description">趋势描述。</param>
/// <param name="RecordedAt">记录时间。</param>
public record AmazonTrendDto(
    string Asin,
    string Title,
    Amazon.AmazonTrendType TrendType,
    string Description,
    DateTime RecordedAt);

/// <summary>
/// <summary>
/// Amazon 商品历史数据点。
/// </summary>
/// <param name="Timestamp">采集时间。</param>
/// <param name="Rank">排名。</param>
/// <param name="Price">价格。</param>
/// <param name="Rating">评分。</param>
/// <param name="ReviewsCount">评论数。</param>
public record AmazonProductHistoryPoint(DateTime Timestamp, int Rank, decimal? Price, float? Rating, int? ReviewsCount);

/// <summary>
/// <summary>
/// 汇总报告对象，包含指标、趋势与文本。
/// </summary>
/// <param name="CoreMetrics">核心指标。</param>
/// <param name="Trends">趋势集合。</param>
/// <param name="ReportText">文本报告。</param>
public record AmazonSnapshotReportDto(
    AmazonCoreMetricsDto CoreMetrics,
    IReadOnlyList<AmazonTrendDto> Trends,
    string ReportText);
