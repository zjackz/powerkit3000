using System;

namespace powerkit3000.core.Amazon.Contracts;

/// <summary>
/// 表示从 Amazon 榜单抓取到的一条原始条目。
/// </summary>
public record AmazonBestsellerEntry(
    string Asin,
    string Title,
    string? Brand,
    string? ImageUrl,
    int Rank,
    decimal? Price,
    float? Rating,
    int? ReviewsCount,
    DateTime? ListingDate);
