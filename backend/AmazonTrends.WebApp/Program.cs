
using AmazonTrends.Core.Services;
using AmazonTrends.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Serilog;

// 1. 配置 Serilog 日志记录器
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "../logs/amazontrends-.log", // 使用相对路径，日志将输出到项目根目录下的 logs 文件夹
        rollingInterval: RollingInterval.Day, // 每日生成一个新文件
        retainedFileCountLimit: 7, // 最多保留7天的日志文件
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("正在启动 Web 主机...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. 将 Serilog 集成到 ASP.NET Core
    builder.Host.UseSerilog();

    // 3. 注册服务到依赖注入容器

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
        .UseSerilogLogProvider() // 将 Hangfire 的日志也整合到 Serilog
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


    // 4. 配置 HTTP 请求处理管道

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

}
catch (Exception ex)
{
    Log.Fatal(ex, "主机意外终止");
}
finally
{
    Log.CloseAndFlush();
}


// 5. 辅助方法

/// <summary>
/// 从数据库加载所有分类，并为每个分类调度一个每日运行的抓取任务。
/// 抓取成功后，分析任务会自动被触发。
/// </summary>
void ScheduleRecurringJobs(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("开始调度每日后台任务...");
        var categories = dbContext.Categories.ToList();
        if (!categories.Any())
        {
            logger.LogWarning("数据库中未找到任何分类，无法调度任务。");
            return;
        }

        foreach (var category in categories)
        {
            // 为每个分类定义一个每日运行的、唯一的重复性作业
            string jobId = $"daily-scrape-and-analyze-category-{category.Id}";
            recurringJobManager.AddOrUpdate<ScrapingService>(
                jobId,
                service => service.ScrapeBestsellersAsync(category.Id),
                Cron.Daily(3), // 每日 UTC 时间凌晨3点运行
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
            );
            logger.LogInformation("已为分类 '{CategoryName}' (ID: {CategoryId}) 调度每日任务，Job ID: {JobId}", category.Name, category.Id, jobId);
        }
        logger.LogInformation("所有分类的每日后台任务调度完成。");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "调度后台任务时发生错误。");
    }
}

