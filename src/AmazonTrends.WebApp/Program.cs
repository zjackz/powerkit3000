
using AmazonTrends.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// 1. 注册服务到依赖注入容器

// 添加对 Controller 的支持
builder.Services.AddControllers();

// 配置并注册 EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置并注册 Hangfire 服务
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 添加 Hangfire 服务端，用于处理后台任务
builder.Services.AddHangfireServer();

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
app.UseHangfireDashboard();

// 映射 API 控制器
app.MapControllers();


app.Run();

