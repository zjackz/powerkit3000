using System;
using System.Collections.Generic;

namespace pk.data.Models;

/// <summary>
/// Amazon 商品主数据，对应一个 ASIN。
/// </summary>
public class AmazonProduct
{
    public string Id { get; set; } = null!; // ASIN
    public string Title { get; set; } = null!;
    public string? Brand { get; set; }
    public int CategoryId { get; set; }
    public DateTime? ListingDate { get; set; }
    public string? ImageUrl { get; set; }

    public AmazonCategory Category { get; set; } = null!;
    public ICollection<AmazonProductDataPoint> DataPoints { get; set; } = new List<AmazonProductDataPoint>();
    public ICollection<AmazonTrend> Trends { get; set; } = new List<AmazonTrend>();
    public ICollection<AmazonProductOperationalMetric> OperationalMetrics { get; set; } = new List<AmazonProductOperationalMetric>();
}
