using System;

namespace pk.data.Models;

/// <summary>
/// 模板配置使用的通用字典条目。
/// </summary>
public class TemplateDictionaryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Category { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
