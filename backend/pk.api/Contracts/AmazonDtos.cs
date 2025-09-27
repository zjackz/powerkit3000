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
    /// <summary>
    /// 内部类目主键。
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// 榜单类型。
    /// </summary>
    public AmazonBestsellerType BestsellerType { get; set; } = AmazonBestsellerType.BestSellers;
}

/// <summary>
/// 外部抓取工具导入快照的请求体。
/// </summary>
public class AmazonImportSnapshotRequest
{
    /// <summary>
    /// 内部类目主键，可选。与 <see cref="AmazonCategoryId"/> 二选一。
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Amazon 官方类目编号，可选。与 <see cref="CategoryId"/> 二选一。
    /// </summary>
    public string? AmazonCategoryId { get; set; }

    /// <summary>
    /// 榜单类型。
    /// </summary>
    public AmazonBestsellerType BestsellerType { get; set; } = AmazonBestsellerType.BestSellers;

    /// <summary>
    /// 快照采集时间，未提供时由服务器补当前时间。
    /// </summary>
    public DateTime? CapturedAt { get; set; }

    /// <summary>
    /// 榜单条目集合。
    /// </summary>
    [MinLength(1, ErrorMessage = "Entries must contain at least one item.")]
    public List<AmazonImportSnapshotEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// 导入快照的单条榜单记录。
/// </summary>
public class AmazonImportSnapshotEntryDto
{
    [Required]
    /// <summary>
    /// 商品 ASIN。
    /// </summary>
    public string Asin { get; set; } = null!;

    [Required]
    /// <summary>
    /// 商品标题。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 品牌名称。
    /// </summary>
    public string? Brand { get; set; }
    /// <summary>
    /// 商品主图 URL。
    /// </summary>
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue)]
    /// <summary>
    /// 榜单排名。
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// 商品价格。
    /// </summary>
    public decimal? Price { get; set; }
    /// <summary>
    /// 商品评分。
    /// </summary>
    public float? Rating { get; set; }
    /// <summary>
    /// 评论数量。
    /// </summary>
    public int? ReviewsCount { get; set; }

    /// <summary>
    /// 商品首次上架日期，如可获取。
    /// </summary>
    public DateTime? ListingDate { get; set; }
}
