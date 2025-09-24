using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace pk.core.Logging;

public static class SimpleFileLoggingBuilderExtensions
{
    public static ILoggingBuilder AddSimpleFileLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        var optionsSection = configuration.GetSection("Logging").GetSection("File");
        var options = new SimpleFileLoggerOptions
        {
            Directory = optionsSection["Directory"] ?? "logs",
            FileName = optionsSection["FileName"] ?? "pk.log",
            MinimumLevel = optionsSection["MinimumLevel"] ?? nameof(LogLevel.Information),
            TimestampFormat = optionsSection["TimestampFormat"]
        };

        if (!Enum.TryParse<LogLevel>(options.MinimumLevel, true, out var minimumLevel))
        {
            minimumLevel = LogLevel.Information;
        }

        var directory = string.IsNullOrWhiteSpace(options.Directory) ? "logs" : options.Directory;
        var fileName = string.IsNullOrWhiteSpace(options.FileName) ? "pk.log" : options.FileName;

        var filePath = Path.Combine(AppContext.BaseDirectory, directory, fileName);

        builder.AddProvider(new SimpleFileLoggerProvider(filePath, minimumLevel, options.TimestampFormat ?? string.Empty));
        return builder;
    }
}
