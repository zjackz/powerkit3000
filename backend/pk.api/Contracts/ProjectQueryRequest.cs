namespace pk.api.Contracts;

/// <summary>
/// 项目查询请求参数。
/// </summary>
public class ProjectQueryRequest
{
    /// <summary>
    /// 关键字搜索（名称、简介等）。
    /// </summary>
    public string? Search { get; set; }
    /// <summary>
    /// 项目状态集合。
    /// </summary>
    public IEnumerable<string>? States { get; set; }
    /// <summary>
    /// 国家筛选集合。
    /// </summary>
    public IEnumerable<string>? Countries { get; set; }
    /// <summary>
    /// 类别筛选集合。
    /// </summary>
    public IEnumerable<string>? Categories { get; set; }
    /// <summary>
    /// 最小目标金额。
    /// </summary>
    public decimal? MinGoal { get; set; }
    /// <summary>
    /// 最大目标金额。
    /// </summary>
    public decimal? MaxGoal { get; set; }
    /// <summary>
    /// 最小达成率。
    /// </summary>
    public decimal? MinPercentFunded { get; set; }
    /// <summary>
    /// 筛选上线时间（之后）。
    /// </summary>
    public DateTime? LaunchedAfter { get; set; }
    /// <summary>
    /// 筛选上线时间（之前）。
    /// </summary>
    public DateTime? LaunchedBefore { get; set; }
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// 每页数量。
    /// </summary>
    public int? PageSize { get; set; }
}
