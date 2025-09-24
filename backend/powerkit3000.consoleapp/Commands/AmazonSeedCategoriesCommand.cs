using Microsoft.Extensions.Logging;
using powerkit3000.core.Amazon.Options;
using powerkit3000.core.Amazon.Services;

namespace consoleapp.Commands;

/// <summary>
/// CLI 命令：根据配置同步 Amazon 类目表。
/// </summary>
public class AmazonSeedCategoriesCommand
{
    private readonly AmazonIngestionService _ingestionService;
    private readonly ILogger<Program> _logger;
    private readonly AmazonModuleOptions _options;

    public AmazonSeedCategoriesCommand(AmazonIngestionService ingestionService, ILogger<Program> logger, Microsoft.Extensions.Options.IOptions<AmazonModuleOptions> options)
    {
        _ingestionService = ingestionService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_options.Categories.Count == 0)
        {
            Console.WriteLine("未在配置中找到 Amazon 类目。请在 appsettings.json 的 Amazon:Categories 中添加。");
            return;
        }

        // 同步类目并输出本次更新的记录数，方便运维确认。
        var affected = await _ingestionService.EnsureCategoriesAsync(cancellationToken);
        Console.WriteLine($"已同步 {affected} 个 Amazon 类目。");
    }
}
