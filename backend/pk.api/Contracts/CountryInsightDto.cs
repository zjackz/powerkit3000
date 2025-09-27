namespace pk.api.Contracts;

/// <summary>
/// 国家维度洞察 DTO。
/// </summary>
public class CountryInsightDto
{
    /// <summary>
    /// 国家代码。
    /// </summary>
    public required string Country { get; init; }
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
    /// 总筹资金额。
    /// </summary>
    public decimal TotalPledged { get; init; }
}
