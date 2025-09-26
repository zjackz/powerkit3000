using System;
using System.Collections.Generic;

namespace pk.data.Models;

/// <summary>
/// Amazon 运营快照，记录一次运营指标采集的汇总信息。
/// </summary>
public class AmazonOperationalSnapshot
{
    public long Id { get; set; }
    public DateTime CapturedAt { get; set; }
    public long? SourceSnapshotId { get; set; }
    public string Status { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public ICollection<AmazonProductOperationalMetric> ProductMetrics { get; set; } = new List<AmazonProductOperationalMetric>();
}
