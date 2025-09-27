namespace pk.api.Contracts;

/// <summary>
/// 月度趋势数据点。
/// </summary>
public class MonthlyTrendPointDto
{
    /// <summary>
    /// 年份。
    /// </summary>
    public int Year { get; init; }
    /// <summary>
    /// 月份。
    /// </summary>
    public int Month { get; init; }
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
}
