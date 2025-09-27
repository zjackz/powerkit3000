namespace pk.api.Contracts;

/// <summary>
/// 创作者表现指标。
/// </summary>
public class CreatorPerformanceDto
{
    /// <summary>
    /// 创作者 ID。
    /// </summary>
    public long CreatorId { get; init; }
    /// <summary>
    /// 创作者名称。
    /// </summary>
    public required string CreatorName { get; init; }
    /// <summary>
    /// 项目总数。
    /// </summary>
    public int TotalProjects { get; init; }
    /// <summary>
    /// 成功项目数。
    /// </summary>
    public int SuccessfulProjects { get; init; }
    /// <summary>
    /// 成功率。
    /// </summary>
    public decimal SuccessRate { get; init; }
    /// <summary>
    /// 平均达成率。
    /// </summary>
    public decimal AveragePercentFunded { get; init; }
    /// <summary>
    /// 总筹资金额。
    /// </summary>
    public decimal TotalPledged { get; init; }
}
