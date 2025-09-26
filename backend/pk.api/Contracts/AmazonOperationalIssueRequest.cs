using System;
using pk.core.Amazon.Operations;

namespace pk.api.Contracts;

public sealed record AmazonOperationalIssueRequest(
    string? IssueType,
    string? Severity,
    string? Search,
    int Page = 1,
    int PageSize = 20)
{
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
