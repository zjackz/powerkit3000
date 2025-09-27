using System;

namespace pk.core.Amazon.Options;

/// <summary>
/// 运营仪表盘的全局阈值配置，允许通过 appsettings 覆盖。
/// </summary>
public class AmazonOperationalDashboardOptions
{
    /// <summary>
    /// 配置节点名称常量。
    /// </summary>
    public const string SectionName = "AmazonOperationalDashboard";

    /// <summary>
    /// 库存低于多少天视为达标（默认 10 天）。
    /// </summary>
    public int InventoryThresholdDays { get; set; } = 10;

    /// <summary>
    /// 库存阈值为高危的倍数（默认 0.5，意味着低于一半则判定高危）。
    /// </summary>
    public decimal InventoryHighSeverityFactor { get; set; } = 0.5m;

    /// <summary>
    /// 统计差评的时间窗口，单位：天。
    /// </summary>
    public int NegativeReviewWindowDays { get; set; } = 7;

    /// <summary>
    /// 差评达到多少条视为高危。
    /// </summary>
    public int NegativeReviewHighCount { get; set; } = 3;

    /// <summary>
    /// 差评达到多少条视为中危。
    /// </summary>
    public int NegativeReviewMediumCount { get; set; } = 1;

    /// <summary>
    /// 数据超过多长时间未更新时提示陈旧。
    /// </summary>
    public TimeSpan DataStaleAfter { get; set; } = TimeSpan.FromHours(48);

    /// <summary>
    /// 广告相关阈值（占位，后续接入广告数据）。
    /// </summary>
    public decimal AdWasteAcosThreshold { get; set; } = 0.30m;

    /// <summary>
    /// ACOS 环比增长多少视为浪费（占位，后续接入广告数据）。
    /// </summary>
    public decimal AdWasteAcosDelta { get; set; } = 0.05m;
}
