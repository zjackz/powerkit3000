using System;
using Microsoft.Extensions.Logging;
using pk.core.Amazon;
using pk.core.Amazon.Services;

namespace consoleapp.Commands;

/// <summary>
/// CLI 命令：根据配置采集 Amazon 榜单快照。
/// </summary>
public class AmazonFetchCommand
{
    private readonly AmazonIngestionService _ingestionService;
    private readonly ILogger<Program> _logger;

    public AmazonFetchCommand(AmazonIngestionService ingestionService, ILogger<Program> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    public async Task ExecuteAsync(int categoryId, AmazonBestsellerType bestsellerType, CancellationToken cancellationToken)
    {
        Console.WriteLine($"开始采集 Amazon 类目 {categoryId} 的 {bestsellerType} 榜单...");
        try
        {
            // 调用核心服务执行抓取，成功后输出快照主键便于后续分析或排查。
            var snapshotId = await _ingestionService.CaptureSnapshotAsync(categoryId, bestsellerType, cancellationToken);
            Console.WriteLine($"采集完成，SnapshotId = {snapshotId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "采集 Amazon 榜单失败");
            Console.WriteLine($"采集失败：{ex.Message}");
            throw;
        }
    }
}
