using System;

namespace pk.api.Contracts;

/// <summary>
/// 项目汇总指标 DTO。
/// </summary>
public class ProjectSummaryDto
{
    /// <summary>
    /// 项目总数。
    /// </summary>
    public int TotalProjects { get; init; }
    /// <summary>
    /// 成功项目数。
    /// </summary>
    public int SuccessfulProjects { get; init; }
    /// <summary>
    /// 总筹资金额。
    /// </summary>
    public decimal TotalPledged { get; init; }
    /// <summary>
    /// 涉及国家数量。
    /// </summary>
    public int DistinctCountries { get; init; }
    /// <summary>
    /// 成功率（百分比）。
    /// </summary>
    public decimal SuccessRate => TotalProjects == 0 ? 0 : Math.Round((decimal)SuccessfulProjects / TotalProjects * 100, 1);
}
