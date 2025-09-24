using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.core.Amazon;
using pk.core.Amazon.Services;
using pk.data;

namespace pk.api.Jobs;

/// <summary>
/// Hangfire 调度使用的执行器，负责抓取榜单并同步触发趋势分析。
/// </summary>
public class AmazonRecurringJobService
{
    private readonly AppDbContext _dbContext;
    private readonly AmazonIngestionService _ingestionService;
    private readonly AmazonTrendAnalysisService _analysisService;
    private readonly ILogger<AmazonRecurringJobService> _logger;

    public AmazonRecurringJobService(
        AppDbContext dbContext,
        AmazonIngestionService ingestionService,
        AmazonTrendAnalysisService analysisService,
        ILogger<AmazonRecurringJobService> logger)
    {
        _dbContext = dbContext;
        _ingestionService = ingestionService;
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// 根据配置抓取指定类目的榜单，并在成功后立即计算趋势。
    /// </summary>
    public async Task CaptureAndAnalyzeAsync(string amazonCategoryId, AmazonBestsellerType bestsellerType)
    {
        // Hangfire 不支持传入 CancellationToken，这里默认无取消场景。
        var category = await _dbContext.AmazonCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.AmazonCategoryId == amazonCategoryId);

        if (category == null)
        {
            _logger.LogWarning("未找到 Amazon 类目 {AmazonCategoryId}，请先执行 amazon-seed 同步配置。", amazonCategoryId);
            return;
        }

        try
        {
            var snapshotId = await _ingestionService.CaptureSnapshotAsync(category.Id, bestsellerType, default);
            _logger.LogInformation("已采集 Amazon 类目 {CategoryId} 的 {BestsellerType} 快照 {SnapshotId}", category.Id, bestsellerType, snapshotId);

            var trendCount = await _analysisService.AnalyzeSnapshotAsync(snapshotId, default);
            _logger.LogInformation("快照 {SnapshotId} 分析完成，生成 {TrendCount} 条趋势。", snapshotId, trendCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行 Amazon 定时任务失败，类目 {AmazonCategoryId}, 榜单 {BestsellerType}", amazonCategoryId, bestsellerType);
            throw;
        }
    }
}
