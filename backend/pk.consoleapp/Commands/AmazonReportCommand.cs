using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.core.Amazon.Services;
using pk.data;

namespace consoleapp.Commands;

/// <summary>
/// CLI 命令：生成并打印 Amazon 榜单分析报告。
/// </summary>
public class AmazonReportCommand
{
    private readonly AmazonReportingService _reportingService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<Program> _logger;

    public AmazonReportCommand(AmazonReportingService reportingService, AppDbContext dbContext, ILogger<Program> logger)
    {
        _reportingService = reportingService;
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

        try
        {
            var report = await _reportingService.BuildReportAsync(snapshotId, cancellationToken);
            if (report == null)
            {
                Console.WriteLine("未找到报告数据。");
                return;
            }

            Console.WriteLine(report.ReportText); // 直接打印方便重定向到文件或后续发送
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成 Amazon 报告失败");
            Console.WriteLine($"生成报告失败：{ex.Message}");
            throw;
        }
    }
}
