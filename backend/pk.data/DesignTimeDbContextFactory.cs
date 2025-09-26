using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace pk.data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Development";

            var basePath = Directory.GetCurrentDirectory();
            // 配置文件与控制台应用保持一致，默认位于 pk.consoleapp 目录下。
            var consoleAppPath = Path.GetFullPath(Path.Combine(basePath, "..", "pk.consoleapp"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(consoleAppPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("AppDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("缺少 AppDb 连接字符串，请在配置文件或环境变量中提供。");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
