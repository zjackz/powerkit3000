namespace pk.api.Contracts;

/// <summary>
/// 项目筛选器选项集合。
/// </summary>
public class ProjectFiltersDto
{
    /// <summary>
    /// 状态选项。
    /// </summary>
    public required IReadOnlyList<FilterOptionDto> States { get; init; }
    /// <summary>
    /// 国家选项。
    /// </summary>
    public required IReadOnlyList<FilterOptionDto> Countries { get; init; }
    /// <summary>
    /// 类别选项。
    /// </summary>
    public required IReadOnlyList<FilterOptionDto> Categories { get; init; }
}
