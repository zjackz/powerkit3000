namespace pk.api.Contracts;

/// <summary>
/// 项目查询结果。
/// </summary>
public class ProjectQueryResponseDto
{
    /// <summary>
    /// 总记录数。
    /// </summary>
    public required int Total { get; init; }
    /// <summary>
    /// 当前页项目列表。
    /// </summary>
    public required IReadOnlyList<ProjectListItemDto> Items { get; init; }
    /// <summary>
    /// 汇总统计信息。
    /// </summary>
    public required ProjectQueryStatsDto Stats { get; init; }
}
