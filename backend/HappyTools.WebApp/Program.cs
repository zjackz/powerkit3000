using HappyTools.Core.Services;
using HappyTools.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using Microsoft.OpenApi.Models;
using System.IO;
using HappyTools.WebApp.Filters;

// 1. 配置 Serilog 日志记录器
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "../logs/app-.log", // 使用相对路径
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
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

    builder.Services.AddControllers();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddHttpClient();

    // --- 在此注册你的核心业务服务 ---
    builder.Services.AddScoped<KickstarterDataImportService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "HappyTools API", Version = "v1", Description = "HappyTools 后端 API 文档" });
        options.OperationFilter<FileUploadOperationFilter>();

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });


    var app = builder.Build();

    // 4. 配置 HTTP 请求处理管道

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();

    app.MapControllers();

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