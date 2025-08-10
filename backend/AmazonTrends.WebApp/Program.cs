
using AmazonTrends.Core.Services;
using AmazonTrends.Data;
using AmazonTrends.Data.Models;
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
    builder.Services.AddScoped<ReportService>();

    // 配置 Redis 缓存
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
        options.InstanceName = "AmazonTrends:"; // 缓存键前缀
    });

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

    // 5. 初始化数据库和种子数据
    InitializeDatabase(app);

    // 6. 配置 HTTP 请求处理管道

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

    // 全局异常处理中间件
    app.UseExceptionHandler("/error");

    // 启用 Hangfire Dashboard，默认路径是 /hangfire
    // TODO: 在生产环境中，需要为 Dashboard 添加授权验证
    app.UseHangfireDashboard();

    // 映射 API 控制器
    app.MapControllers();

    // 添加一个通用的错误处理路由
    app.Map("/error", (HttpContext context) =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        Log.Error(exception, "发生未处理的异常: {Message}", exception?.Message);

        return Results.Problem(
            detail: exception?.Message,
            title: "服务器内部错误",
            statusCode: StatusCodes.Status500InternalServerError
        );
    });

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


// 辅助方法

/// <summary>
/// 初始化数据库：如果不存在则创建，并应用所有挂起的迁移。
/// 然后，如果数据库是空的，则填充种子数据。
/// </summary>
void InitializeDatabase(IHost app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("正在检查并应用数据库迁移...");
        dbContext.Database.Migrate();
        logger.LogInformation("数据库迁移完成。");

        logger.LogInformation("正在检查是否需要填充种子数据...");
        if (!dbContext.Categories.Any())
        {
            logger.LogInformation("数据库为空，开始填充种子数据。");

            var electronics = new Category { Name = "Electronics", AmazonCategoryId = "172282" };
            var books = new Category { Name = "Books", AmazonCategoryId = "283155" };
            var homeAndKitchen = new Category { Name = "Home & Kitchen", AmazonCategoryId = "1055398" };

            dbContext.Categories.AddRange(electronics, books, homeAndKitchen);
            dbContext.SaveChanges(); // 先保存分类以获取ID

            dbContext.Products.AddRange(
                new Product { Id = "B0863FR3S9", Title = "Echo Dot (4th Gen) | Smart speaker with Alexa", Brand = "Amazon", CategoryId = electronics.Id, ListingDate = new DateTime(2020, 10, 22) },
                new Product { Id = "B07WCS3G78", Title = "Kindle Paperwhite (8 GB) – Now with a 6.8" display and adjustable warm light", Brand = "Amazon", CategoryId = electronics.Id, ListingDate = new DateTime(2021, 10, 27) },
                new Product { Id = "B08P3QVFMK", Title = "Atomic Habits: An Easy & Proven Way to Build Good Habits & Break Bad Ones", Brand = "James Clear", CategoryId = books.Id, ListingDate = new DateTime(2018, 10, 16) },
                new Product { Id = "B07Y8B6B7X", Title = "Instant Pot Duo 7-in-1 Electric Pressure Cooker, 6 Quart", Brand = "Instant Pot", CategoryId = homeAndKitchen.Id, ListingDate = new DateTime(2017, 10, 4) }
            );

            dbContext.SaveChanges();
            logger.LogInformation("种子数据填充成功。");
        }
        else
        {
            logger.LogInformation("数据库中已存在数据，无需填充。");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "在初始化数据库或填充种子数据时发生错误。");
    }
}


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
            logger.LogWarning("数据库中未找到任何分类，无法调度任务。请先填充种子数据。");
            return;
        }

        foreach (var category in categories)
        {
            // 为每个分类定义一个每日运行的、唯一的重复性作业
            string jobIdBestSellers = $"daily-scrape-best-sellers-category-{category.Id}";
            recurringJobManager.AddOrUpdate<ScrapingService>(
                jobIdBestSellers,
                service => service.ScrapeBestsellersAsync(category.Id, BestsellerType.BestSellers),
                Cron.Daily(3), // 每日 UTC 时间凌晨3点运行
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
            );
            logger.LogInformation("已为分类 '{CategoryName}' (ID: {CategoryId}) 调度每日 Best Sellers 任务，Job ID: {JobId}", category.Name, category.Id, jobIdBestSellers);

            string jobIdNewReleases = $"daily-scrape-new-releases-category-{category.Id}";
            recurringJobManager.AddOrUpdate<ScrapingService>(
                jobIdNewReleases,
                service => service.ScrapeBestsellersAsync(category.Id, BestsellerType.NewReleases),
                Cron.Daily(4), // 每日 UTC 时间凌晨4点运行
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
            );
            logger.LogInformation("已为分类 '{CategoryName}' (ID: {CategoryId}) 调度每日 New Releases 任务，Job ID: {JobId}", category.Name, category.Id, jobIdNewReleases);

            string jobIdMoversAndShakers = $"daily-scrape-movers-and-shakers-category-{category.Id}";
            recurringJobManager.AddOrUpdate<ScrapingService>(
                jobIdMoversAndShakers,
                service => service.ScrapeBestsellersAsync(category.Id, BestsellerType.MoversAndShakers),
                Cron.Daily(5), // 每日 UTC 时间凌晨5点运行
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
            );
            logger.LogInformation("已为分类 '{CategoryName}' (ID: {CategoryId}) 调度每日 Movers & Shakers 任务，Job ID: {JobId}", category.Name, category.Id, jobIdMoversAndShakers);
        }

        // 调度每日报告生成任务
        recurringJobManager.AddOrUpdate<ReportService>(
            "daily-report-generation",
            service => service.GenerateDailyReportAsync(),
            Cron.Daily(6), // 每日 UTC 时间凌晨6点运行
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
        );
        logger.LogInformation("已调度每日报告生成任务，Job ID: daily-report-generation");

        logger.LogInformation("所有分类的每日后台任务调度完成。");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "调度后台任务时发生错误。");
    }
}

