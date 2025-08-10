using AmazonTrends.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AmazonTrends.Core.Services;

public class ReportService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ReportService> _logger;

    public ReportService(AppDbContext dbContext, ILogger<ReportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 生成每日趋势报告。
    /// </summary>
    public async Task GenerateDailyReportAsync()
    {
        _logger.LogInformation("开始生成每日趋势报告。");

        try
        {
            // 获取最新的分析结果
            var latestAnalysisResult = await _dbContext.AnalysisResults
                .OrderByDescending(ar => ar.AnalysisTime)
                .Include(ar => ar.Trends)
                    .ThenInclude(t => t.Product)
                .FirstOrDefaultAsync();

            if (latestAnalysisResult == null)
            {
                _logger.LogWarning("未找到任何分析结果，无法生成报告。");
                return;
            }

            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine($"--- 亚马逊趋势每日报告 - {latestAnalysisResult.AnalysisTime:yyyy-MM-dd HH:mm:ss} ---");
            reportBuilder.AppendLine($"本次分析基于 {latestAnalysisResult.DataCollectionRun.Timestamp:yyyy-MM-dd HH:mm:ss} 的数据采集。");
            reportBuilder.AppendLine("");

            // 核心指标概览
            var totalNewEntries = latestAnalysisResult.Trends.Count(t => t.TrendType == "NewEntry");
            var totalRankSurges = latestAnalysisResult.Trends.Count(t => t.TrendType == "RankSurge");
            var totalConsistentPerformers = latestAnalysisResult.Trends.Count(t => t.TrendType == "ConsistentPerformer");
            var totalProductsAnalyzed = await _dbContext.ProductDataPoints
                .Where(dp => dp.DataCollectionRunId == latestAnalysisResult.DataCollectionRunId)
                .Select(dp => dp.ProductId)
                .Distinct()
                .CountAsync();

            reportBuilder.AppendLine("### 核心指标概览");
            reportBuilder.AppendLine($"- 分析产品总数: {totalProductsAnalyzed}");
            reportBuilder.AppendLine($"- 新上榜产品: {totalNewEntries}");
            reportBuilder.AppendLine($"- 排名飙升产品: {totalRankSurges}");
            reportBuilder.AppendLine($"- 持续霸榜产品: {totalConsistentPerformers}");
            reportBuilder.AppendLine("");

            // 新上榜产品
            var newEntries = latestAnalysisResult.Trends
                .Where(t => t.TrendType == "NewEntry")
                .Take(5) // 只显示前5个
                .ToList();
            if (newEntries.Any())
            {
                reportBuilder.AppendLine("### 新上榜产品 (Top 5)");
                foreach (var trend in newEntries)
                {
                    reportBuilder.AppendLine($"- {trend.Product.Title} (ASIN: {trend.ProductId}) - {trend.Description}");
                }
                reportBuilder.AppendLine("");
            }

            // 排名飙升产品
            var rankSurges = latestAnalysisResult.Trends
                .Where(t => t.TrendType == "RankSurge")
                .OrderByDescending(t =>
                {
                    // 尝试解析描述中的排名变化，以便按变化幅度排序
                    var match = System.Text.RegularExpressions.Regex.Match(t.Description, @"上升了 (\d+) 位");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                })
                .Take(5) // 只显示前5个
                .ToList();
            if (rankSurges.Any())
            {
                reportBuilder.AppendLine("### 排名飙升产品 (Top 5)");
                foreach (var trend in rankSurges)
                {
                    reportBuilder.AppendLine($"- {trend.Product.Title} (ASIN: {trend.ProductId}) - {trend.Description}");
                }
                reportBuilder.AppendLine("");
            }

            var reportContent = reportBuilder.ToString();
            _logger.LogInformation("每日趋势报告已生成:\n{ReportContent}", reportContent);

            // 模拟发送报告
            SendReport(reportContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成每日趋势报告时发生错误。");
        }
    }

    /// <summary>
    /// 模拟发送报告（实际应用中会集成邮件或消息推送服务）。
    /// </summary>
    /// <param name="reportContent">报告内容。</param>
    public void SendReport(string reportContent)
    {
        _logger.LogInformation("模拟发送报告:\n{ReportContent}", reportContent);
        // 实际应用中，这里会调用第三方服务发送邮件或消息
        // 例如：_emailService.SendEmail("recipient@example.com", "每日亚马逊趋势报告", reportContent);
        // 例如：_discordService.SendMessage("channel_id", reportContent);
    }
}
