
using AmazonTrends.Data;
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

    // --- 如需用户认证，请取消以下注释 ---
    // builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    //     .AddEntityFrameworkStores<AppDbContext>()
    //     .AddDefaultTokenProviders();
    // 
    // builder.Services.AddAuthentication(options =>
    // {
    //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    // })
    // .AddJwtBearer(options =>
    // {
    //     options.TokenValidationParameters = new TokenValidationParameters
    //     {
    //         ValidateIssuer = true,
    //         ValidateAudience = true,
    //         ValidateLifetime = true,
    //         ValidateIssuerSigningKey = true,
    //         ValidIssuer = builder.Configuration["Jwt:Issuer"],
    //         ValidAudience = builder.Configuration["Jwt:Audience"],
    //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    //     };
    // });

    builder.Services.AddHttpClient();

    // --- 在此注册你的核心业务服务 ---
    // builder.Services.AddScoped<MyService>();

    // --- 如需后台任务，请取消以下注释 ---
    // builder.Services.AddHangfire(config => config
    //     .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    //     .UseSimpleAssemblyNameTypeSerializer()
    //     .UseRecommendedSerializerSettings()
    //     .UseSerilogLogProvider()
    //     .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
    //
    // builder.Services.AddHangfireServer(options =>
    // {
    //     options.WorkerCount = Environment.ProcessorCount * 2;
    // });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
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

    // --- 如需用户认证，请取消以下注释 ---
    // app.UseAuthentication();
    // app.UseAuthorization();

    // --- 如需后台任务，请取消以下注释 ---
    // app.UseHangfireDashboard();

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