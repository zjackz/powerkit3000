namespace pk.api.Contracts;

/// <summary>
/// 类别关键词统计 DTO。
/// </summary>
public class CategoryKeywordDto
{
    /// <summary>
    /// 关键词文本。
    /// </summary>
    public required string Keyword { get; init; }
    /// <summary>
    /// 覆盖项目数。
    /// </summary>
    public int ProjectCount { get; init; }
    /// <summary>
    /// 出现次数。
    /// </summary>
    public int OccurrenceCount { get; init; }
    /// <summary>
    /// 平均达成率。
    /// </summary>
    public decimal AveragePercentFunded { get; init; }
}
