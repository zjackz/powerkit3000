namespace pk.core.Amazon.Operations;

/// <summary>
/// 运营问题类别。
/// </summary>
public enum AmazonOperationalIssueType
{
    /// <summary>
    /// 库存不足。
    /// </summary>
    LowStock,
    /// <summary>
    /// 差评过多。
    /// </summary>
    NegativeReview,
    /// <summary>
    /// 广告浪费。
    /// </summary>
    AdWaste
}

/// <summary>
/// 运营问题严重度。
/// </summary>
public enum AmazonOperationalSeverity
{
    /// <summary>
    /// 低危。
    /// </summary>
    Low,
    /// <summary>
    /// 中危。
    /// </summary>
    Medium,
    /// <summary>
    /// 高危。
    /// </summary>
    High
}
