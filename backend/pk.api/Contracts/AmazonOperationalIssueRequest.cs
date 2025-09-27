using System;
using pk.core.Amazon.Operations;

namespace pk.api.Contracts;

/// <summary>
/// API 层接收的运营问题查询参数。
/// </summary>
/// <param name="IssueType">问题类别（字符串枚举）。</param>
/// <param name="Severity">严重度（字符串枚举）。</param>
/// <param name="Search">搜索关键字。</param>
/// <param name="Page">页码。</param>
/// <param name="PageSize">每页数量。</param>
public sealed record AmazonOperationalIssueRequest(
    string? IssueType,
    string? Severity,
    string? Search,
    int Page = 1,
    int PageSize = 20)
{
    /// <summary>
    /// 转换为核心服务所需的查询对象。
    /// </summary>
    public AmazonOperationalIssueQuery ToQuery()
    {
        var parsedType = Enum.TryParse<AmazonOperationalIssueType>(IssueType, true, out var issueType)
            ? issueType
            : (AmazonOperationalIssueType?)null;

        var parsedSeverity = Enum.TryParse<AmazonOperationalSeverity>(Severity, true, out var severityValue)
            ? severityValue
            : (AmazonOperationalSeverity?)null;

        return new AmazonOperationalIssueQuery(parsedType, parsedSeverity, Search, Page, PageSize);
    }
}
