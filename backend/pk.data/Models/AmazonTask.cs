using System;

namespace pk.data.Models;

/// <summary>
/// 表示 Amazon 采集任务的配置实体。
/// </summary>
public class AmazonTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Site { get; set; } = "amazon.com";

    public string CategoriesJson { get; set; } = "[]";

    public string LeaderboardsJson { get; set; } = "[]";

    public string PriceRangeJson { get; set; } = "{}";

    public string KeywordsJson { get; set; } = "{}";

    public string FiltersJson { get; set; } = "{}";

    public string ScheduleJson { get; set; } = "{}";

    public string LimitsJson { get; set; } = "{}";

    public string ProxyPolicy { get; set; } = "default";

    public string Status { get; set; } = "draft";

    public string? Notes { get; set; }

    public string? LlmSummary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
