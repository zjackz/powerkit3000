
using AmazonTrends.Core.Services;
using AmazonTrends.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册服务到依赖注入容器

// 添加对 Controller 的支持
builder.Services.AddControllers();

// 配置并注册 EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 注册 HttpClient
builder.Services.AddHttpClient();

// 注册核心业务服务
builder.Services.AddScoped<ScrapingService>();
builder.Services.AddScoped<AnalysisService>();

// 配置并注册 Hangfire 服务
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 添加 Hangfire 服务端，用于处理后台任务
builder.Services.AddHangfireServer(options =>
{
    // 设置 Worker 数量，可以根据服务器性能调整
    options.WorkerCount = Environment.ProcessorCount * 2;
});

// 添加并配置 Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Amazon Trends API", Version = "v1" });
});


var app = builder.Build();


// 2. 配置 HTTP 请求处理管道

// 在开发环境中启用 Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // 将 Swagger UI 设置为应用的根路径 (e.g., http://localhost:5000/)
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Amazon Trends API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// 启用 Hangfire Dashboard，默认路径是 /hangfire
// TODO: 在生产环境中，需要为 Dashboard 添加授权验证
app.UseHangfireDashboard();

// 映射 API 控制器
app.MapControllers();

// 动态调度后台任务
ScheduleRecurringJobs(app.Services);

app.Run();


// 3. 辅助方法

/// <summary>
/// 从数据库加载所有分类，并为每个分类调度一个每小时运行的抓取任务。
/// </summary>
void ScheduleRecurringJobs(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("开始调度后台抓取任务...");
        var categories = dbContext.Categories.ToList();
        if (!categories.Any())
        {
            logger.LogWarning("数据库中未找到任何分类，无法调度抓取任务。");
            return;
        }

        foreach (var category in categories)
        {
            string jobId = $"scrape-category-{category.Id}";
            recurringJobManager.AddOrUpdate<ScrapingService>(
                jobId,
                service => service.ScrapeBestsellersAsync(category.Id),
                Cron.Hourly); // 每小时运行一次
            logger.LogInformation("已为分类 '{CategoryName}' (ID: {CategoryId}) 调度任务，Job ID: {JobId}", category.Name, category.Id, jobId);
        }
        logger.LogInformation("所有分类的后台抓取任务调度完成。");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "调度后台任务时发生错误。");
    }
}

