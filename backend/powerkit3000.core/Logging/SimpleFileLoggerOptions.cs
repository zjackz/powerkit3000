using Microsoft.Extensions.Logging;

namespace powerkit3000.core.Logging;

public class SimpleFileLoggerOptions
{
    public const string SectionName = "Logging:File";

    public string Directory { get; set; } = "logs";

    public string FileName { get; set; } = "powerkit3000.log";

    public string MinimumLevel { get; set; } = nameof(LogLevel.Information);

    public string? TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";
}
