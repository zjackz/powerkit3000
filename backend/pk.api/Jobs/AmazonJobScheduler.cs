using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pk.core.Amazon;
using pk.core.Amazon.Options;
using pk.core.Amazon.Services;

namespace pk.api.Jobs;

/// <summary>
/// 负责读取配置并注册 Amazon 定时任务的调度器。
/// </summary>
public class AmazonJobScheduler
{
    private readonly AmazonModuleOptions _options;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly AmazonIngestionService _ingestionService;
    private readonly ILogger<AmazonJobScheduler> _logger;

    /// <summary>
    /// 初始化 <see cref="AmazonJobScheduler"/>。
    /// </summary>
    public AmazonJobScheduler(
        IOptions<AmazonModuleOptions> options,
        IRecurringJobManager recurringJobManager,
        AmazonIngestionService ingestionService,
        ILogger<AmazonJobScheduler> logger)
    {
        _options = options.Value;
        _recurringJobManager = recurringJobManager;
        _ingestionService = ingestionService;
        _logger = logger;
    }

    /// <summary>
    /// 初始化调度任务：若启用则同步类目并注册 Hangfire 定时作业。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (!_options.EnableScheduling)
        {
            _logger.LogInformation("Amazon 调度未启用，跳过任务注册。");
            return;
        }

        if (_options.Jobs.Count == 0)
        {
            _logger.LogWarning("Amazon 调度启用但未配置 Jobs 数组，跳过注册。");
            return;
        }

        await _ingestionService.EnsureCategoriesAsync(cancellationToken);

        foreach (var job in _options.Jobs)
        {
            var jobId = $"amazon:{job.AmazonCategoryId}:{job.BestsellerType}";
            var cronExpression = string.IsNullOrWhiteSpace(job.Cron) ? Cron.Daily() : job.Cron;
            var timeZoneInfo = TimeZoneInfo.Utc;
            if (!string.IsNullOrWhiteSpace(job.TimeZone))
            {
                try
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(job.TimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    _logger.LogWarning("未找到时区 {TimeZone}，任务 {JobId} 将改用 UTC。", job.TimeZone, jobId);
                }
            }

            _recurringJobManager.AddOrUpdate<AmazonRecurringJobService>(
                jobId,
                service => service.CaptureAndAnalyzeAsync(job.AmazonCategoryId, job.BestsellerType),
                cronExpression,
                new RecurringJobOptions { TimeZone = timeZoneInfo });

            _logger.LogInformation("已注册 Amazon 定时任务：{JobId} (Cron: {Cron}, TimeZone: {TimeZoneId})", jobId, cronExpression, timeZoneInfo.Id);
        }
    }
}
