using System;
using System.Collections.Generic;

namespace powerkit3000.data.Models;

/// <summary>
/// Amazon 榜单快照，记录一次抓取任务的状态。
/// </summary>
public class AmazonSnapshot
{
    public long Id { get; set; }
    public DateTime CapturedAt { get; set; }
    public int CategoryId { get; set; }
    public string BestsellerType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public AmazonCategory Category { get; set; } = null!;
    public ICollection<AmazonProductDataPoint> DataPoints { get; set; } = new List<AmazonProductDataPoint>();
    public ICollection<AmazonTrend> Trends { get; set; } = new List<AmazonTrend>();
}
