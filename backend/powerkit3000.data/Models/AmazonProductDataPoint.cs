using System;

namespace powerkit3000.data.Models;

/// <summary>
/// Amazon 商品在某次快照中的数据点。
/// </summary>
public class AmazonProductDataPoint
{
    public long Id { get; set; }
    public string ProductId { get; set; } = null!;
    public long SnapshotId { get; set; }
    public DateTime CapturedAt { get; set; }
    public int Rank { get; set; }
    public decimal? Price { get; set; }
    public float? Rating { get; set; }
    public int? ReviewsCount { get; set; }

    public AmazonProduct Product { get; set; } = null!;
    public AmazonSnapshot Snapshot { get; set; } = null!;
}
