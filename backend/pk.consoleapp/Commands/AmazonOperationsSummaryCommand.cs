using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using pk.core.Amazon.Operations;

namespace consoleapp.Commands;

/// <summary>
/// 查看亚马逊运营问题概览的命令。
/// </summary>
public class AmazonOperationsSummaryCommand
{
    private readonly AmazonOperationalInsightService _insightService;

    public AmazonOperationsSummaryCommand(AmazonOperationalInsightService insightService)
    {
        _insightService = insightService;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var summary = await _insightService.GetSummaryAsync(cancellationToken).ConfigureAwait(false);

        if (summary.LastUpdatedAt == null)
        {
            AnsiConsole.MarkupLine("[yellow]暂无运营快照，请先执行 amazon-operations ingest。[/]");
            return;
        }

        var table = new Table().Width(80);
        table.Title = new TableTitle("Amazon 运营概览");
        table.AddColumn("指标");
        table.AddColumn("数量");

        table.AddRow("最后更新", summary.LastUpdatedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("数据状态", summary.IsStale ? "已超过阈值" : "正常");
        table.AddRow(string.Empty, string.Empty);
        table.AddRow("库存告警", FormatIssueSummary(summary.LowStock));
        table.AddRow("差评告警", FormatIssueSummary(summary.NegativeReview));
        table.AddRow(string.Empty, string.Empty);
        table.AddRow("广告模块", summary.AdWastePlaceholder.Message);

        AnsiConsole.Write(table);
    }

    private static string FormatIssueSummary(AmazonOperationalIssueSummary summary)
        => $"总计 {summary.Total}（高 {summary.High} / 中 {summary.Medium} / 低 {summary.Low}）";
}
