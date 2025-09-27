namespace pk.api.Contracts;

/// <summary>
/// 项目高光卡片 DTO。
/// </summary>
public class ProjectHighlightDto
{
    /// <summary>
    /// 项目 ID。
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// 项目名称。
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// 项目中文名称。
    /// </summary>
    public string? NameCn { get; init; }
    /// <summary>
    /// 类别名称。
    /// </summary>
    public required string CategoryName { get; init; }
    /// <summary>
    /// 所在国家。
    /// </summary>
    public required string Country { get; init; }
    /// <summary>
    /// 达成率。
    /// </summary>
    public decimal PercentFunded { get; init; }
    /// <summary>
    /// 已筹资金额。
    /// </summary>
    public decimal Pledged { get; init; }
    /// <summary>
    /// 筹资速度。
    /// </summary>
    public decimal FundingVelocity { get; init; }
    /// <summary>
    /// 支持者数量。
    /// </summary>
    public int BackersCount { get; init; }
    /// <summary>
    /// 货币代码。
    /// </summary>
    public required string Currency { get; init; }
    /// <summary>
    /// 上线时间。
    /// </summary>
    public DateTime LaunchedAt { get; init; }
}
