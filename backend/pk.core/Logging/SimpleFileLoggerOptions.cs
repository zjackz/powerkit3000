using Microsoft.Extensions.Logging;

namespace pk.core.Logging;

public class SimpleFileLoggerOptions
{
    public const string SectionName = "Logging:File";

    public string Directory { get; set; } = "logs";

    public string FileName { get; set; } = "pk.log";

    public string MinimumLevel { get; set; } = nameof(LogLevel.Information);

    public string? TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";
}
