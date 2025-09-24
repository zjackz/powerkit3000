using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using powerkit3000.core.Amazon.Services;
using powerkit3000.data;

namespace consoleapp.Commands;

/// <summary>
/// CLI 命令：触发 Amazon 快照的趋势分析。
/// </summary>
public class AmazonAnalyzeCommand
{
    private readonly AmazonTrendAnalysisService _analysisService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<Program> _logger;

    public AmazonAnalyzeCommand(AmazonTrendAnalysisService analysisService, AppDbContext dbContext, ILogger<Program> logger)
    {
        _analysisService = analysisService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(string snapshotArg, CancellationToken cancellationToken)
    {
        long snapshotId;
        if (string.Equals(snapshotArg, "latest", StringComparison.OrdinalIgnoreCase))
        {
            snapshotId = await _dbContext.AmazonSnapshots
                .OrderByDescending(s => s.CapturedAt)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (snapshotId == 0)
            {
                Console.WriteLine("尚未存在任何 Amazon Snapshot。");
                return;
            }
        }
        else if (!long.TryParse(snapshotArg, out snapshotId))
        {
            Console.WriteLine("请提供有效的 SnapshotId 或使用 'latest'。");
            return;
        }

        Console.WriteLine($"开始分析 Amazon Snapshot {snapshotId}...");
        try
        {
            // 调用核心服务重新生成趋势数据，前端/报告即可读取最新结果。
            var trendCount = await _analysisService.AnalyzeSnapshotAsync(snapshotId, cancellationToken);
            Console.WriteLine($"分析完成，生成 {trendCount} 条趋势。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析 Amazon Snapshot 失败");
            Console.WriteLine($"分析失败：{ex.Message}");
            throw;
        }
    }
}
