using System;

namespace pk.data.Models;

/// <summary>
/// Amazon 趋势记录，用于标记榜单变化。
/// </summary>
public class AmazonTrend
{
    public long Id { get; set; }
    public string ProductId { get; set; } = null!;
    public long SnapshotId { get; set; }
    public string TrendType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime RecordedAt { get; set; }

    public AmazonProduct Product { get; set; } = null!;
    public AmazonSnapshot Snapshot { get; set; } = null!;
}
