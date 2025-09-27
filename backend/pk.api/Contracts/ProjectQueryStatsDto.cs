namespace pk.api.Contracts;

/// <summary>
/// 项目查询统计指标。
/// </summary>
public class ProjectQueryStatsDto
{
    /// <summary>
    /// 成功项目数量。
    /// </summary>
    public int SuccessfulCount { get; init; }
    /// <summary>
    /// 总筹资金额。
    /// </summary>
    public decimal TotalPledged { get; init; }
    /// <summary>
    /// 平均达成率。
    /// </summary>
    public decimal AveragePercentFunded { get; init; }
    /// <summary>
    /// 支持者总数。
    /// </summary>
    public int TotalBackers { get; init; }
    /// <summary>
    /// 平均目标金额。
    /// </summary>
    public decimal AverageGoal { get; init; }
    /// <summary>
    /// 关注度最高的项目。
    /// </summary>
    public ProjectListItemDto? TopProject { get; init; }
}
