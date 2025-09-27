using System;

namespace pk.core.Amazon.Contracts;

/// <summary>
/// 表示从 Amazon 榜单抓取或导入的一条原始条目。
/// </summary>
public record AmazonBestsellerEntry
{
    /// <summary>
    /// 商品 ASIN。
    /// </summary>
    public string Asin { get; init; } = null!;

    /// <summary>
    /// 商品标题。
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// 品牌名称。
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// 商品主图 URL。
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// 该商品在榜单中的排名。
    /// </summary>
    public int Rank { get; init; }

    /// <summary>
    /// 当前价格（如可获取）。
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// 商品评分。
    /// </summary>
    public float? Rating { get; init; }

    /// <summary>
    /// 评论数量。
    /// </summary>
    public int? ReviewsCount { get; init; }

    /// <summary>
    /// 上架时间或首次发布日期。
    /// </summary>
    public DateTime? ListingDate { get; init; }

    public AmazonBestsellerEntry()
    {
    }

    public AmazonBestsellerEntry(
        string asin,
        string title,
        string? brand,
        string? imageUrl,
        int rank,
        decimal? price,
        float? rating,
        int? reviewsCount,
        DateTime? listingDate)
    {
        Asin = asin;
        Title = title;
        Brand = brand;
        ImageUrl = imageUrl;
        Rank = rank;
        Price = price;
        Rating = rating;
        ReviewsCount = reviewsCount;
        ListingDate = listingDate;
    }
}
