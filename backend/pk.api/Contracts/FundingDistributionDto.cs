namespace pk.api.Contracts;

/// <summary>
/// 筹资分布区间 DTO。
/// </summary>
public class FundingDistributionDto
{
    /// <summary>
    /// 区间标签。
    /// </summary>
    public required string Label { get; init; }
    /// <summary>
    /// 最小达成率。
    /// </summary>
    public decimal MinPercent { get; init; }
    /// <summary>
    /// 最大达成率。
    /// </summary>
    public decimal MaxPercent { get; init; }
    /// <summary>
    /// 区间内项目总数。
    /// </summary>
    public int TotalProjects { get; init; }
    /// <summary>
    /// 区间内成功项目数。
    /// </summary>
    public int SuccessfulProjects { get; init; }
}
