using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using pk.core.Amazon;

namespace pk.api.Contracts;

/// <summary>
/// Amazon 核心指标响应 DTO。
/// </summary>
public record AmazonCoreMetricsResponseDto(
    long SnapshotId,
    DateTime CapturedAt,
    int TotalProducts,
    int TotalNewEntries,
    int TotalRankSurges,
    int TotalConsistentPerformers);

/// <summary>
/// 榜单商品列表项。
/// </summary>
public record AmazonProductListItemDto(
    string Asin,
    string Title,
    string CategoryName,
    DateTime? ListingDate,
    int? LatestRank,
    decimal? LatestPrice,
    float? LatestRating,
    int? LatestReviews,
    DateTime? LastUpdated);

/// <summary>
/// 榜单趋势条目。
/// </summary>
public record AmazonTrendListItemDto(
    string Asin,
    string Title,
    AmazonTrendType TrendType,
    string Description,
    DateTime RecordedAt);

/// <summary>
/// 单个商品的历史数据点。
/// </summary>
public record AmazonProductHistoryPointDto(DateTime Timestamp, int Rank, decimal? Price, float? Rating, int? ReviewsCount);

/// <summary>
/// 榜单商品查询参数。
/// </summary>
public class AmazonProductsQueryRequest
{
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// 趋势查询参数。
/// </summary>
public class AmazonTrendsQueryRequest
{
    public AmazonTrendType? TrendType { get; set; }
}

/// <summary>
/// 快照采集请求体。
/// </summary>
public class AmazonFetchSnapshotRequest
{
    [Required]
    public int CategoryId { get; set; }

    public AmazonBestsellerType BestsellerType { get; set; } = AmazonBestsellerType.BestSellers;
}
