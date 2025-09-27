namespace pk.api.Contracts;

/// <summary>
/// 类别维度洞察 DTO。
/// </summary>
public class CategoryInsightDto
{
    /// <summary>
    /// 类别名称。
    /// </summary>
    public required string CategoryName { get; init; }
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
