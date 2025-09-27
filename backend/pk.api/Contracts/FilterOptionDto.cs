namespace pk.api.Contracts;

/// <summary>
/// 筛选器选项 DTO。
/// </summary>
/// <param name="Value">选项值。</param>
/// <param name="Label">显示名称。</param>
/// <param name="Count">对应数量。</param>
public record FilterOptionDto(string Value, string Label, int Count);
