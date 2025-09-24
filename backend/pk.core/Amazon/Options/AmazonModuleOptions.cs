using System;
using System.Collections.Generic;
using pk.core.Amazon;

namespace pk.core.Amazon.Options;

/// <summary>
/// Amazon 模块的配置项，支持配置类目、抓取节流与 UA。
/// </summary>
public class AmazonModuleOptions
{
    public const string SectionName = "Amazon";

    public List<AmazonCategoryOption> Categories { get; set; } = new();

    public int MinDelayMilliseconds { get; set; } = 1000;
    public int MaxDelayMilliseconds { get; set; } = 3000;

    public string? UserAgent { get; set; }
    public List<string> UserAgentPool { get; set; } = new();

    /// <summary>
    /// 是否启用 Hangfire 自动调度。
    /// </summary>
    public bool EnableScheduling { get; set; }

    /// <summary>
    /// 定时任务列表，每一项对应一条 Hangfire RecurringJob。
    /// </summary>
    public List<AmazonScheduleOption> Jobs { get; set; } = new();
}

/// <summary>
/// 单个 Amazon 类目的配置结构。
/// </summary>
public class AmazonCategoryOption
{
    public string Name { get; set; } = null!;
    public string AmazonCategoryId { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public string? Alias { get; set; }
}

/// <summary>
/// Amazon 调度配置项，描述一个定时任务。
/// </summary>
public class AmazonScheduleOption
{
    /// <summary>
    /// 要抓取的 Amazon 类目 ID（与配置的 Categories 相匹配）。
    /// </summary>
    public string AmazonCategoryId { get; set; } = null!;

    /// <summary>
    /// 榜单类型，默认抓取热销榜。
    /// </summary>
    public AmazonBestsellerType BestsellerType { get; set; } = AmazonBestsellerType.BestSellers;

    /// <summary>
    /// Cron 表达式，若为空默认使用 Hangfire 的每日一次。
    /// </summary>
    public string? Cron { get; set; }

    /// <summary>
    /// 时区 ID，默认 UTC，可填写 "Asia/Shanghai" 等系统可识别的时区。
    /// </summary>
    public string? TimeZone { get; set; }
}
