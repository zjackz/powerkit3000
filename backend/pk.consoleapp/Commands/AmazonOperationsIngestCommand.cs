using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using pk.core.Amazon.Operations;

namespace consoleapp.Commands;

/// <summary>
/// 触发亚马逊运营指标采集的命令。
/// </summary>
public class AmazonOperationsIngestCommand
{
    private readonly AmazonOperationalIngestionService _ingestionService;
    private readonly ILogger<Program> _logger;

    public AmazonOperationsIngestCommand(AmazonOperationalIngestionService ingestionService, ILogger<Program> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始采集亚马逊运营指标...");
        var snapshotId = await _ingestionService.IngestAsync(cancellationToken).ConfigureAwait(false);
        AnsiConsole.MarkupLine($"[green]采集完成，生成运营快照 ID:[/] {snapshotId}");
    }
}
