namespace pk.api.Contracts;

/// <summary>
/// 分析接口的通用筛选请求。
/// </summary>
public class AnalyticsFilterRequest
{
    /// <summary>
    /// 上线时间（之后）。
    /// </summary>
    public DateTime? LaunchedAfter { get; set; }
    /// <summary>
    /// 上线时间（之前）。
    /// </summary>
    public DateTime? LaunchedBefore { get; set; }
    /// <summary>
    /// 国家筛选集合。
    /// </summary>
    public IEnumerable<string>? Countries { get; set; }
    /// <summary>
    /// 类别筛选集合。
    /// </summary>
    public IEnumerable<string>? Categories { get; set; }
    /// <summary>
    /// 最小达成率。
    /// </summary>
    public decimal? MinPercentFunded { get; set; }
}
