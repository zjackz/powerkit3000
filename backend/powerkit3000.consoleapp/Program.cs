using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using consoleapp.Commands;
using Spectre.Console;
using powerkit3000.core.services;
using powerkit3000.core.translations;
using powerkit3000.data;
using powerkit3000.core.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        await RunAppAsync(args);
    }

    public static async Task RunAppAsync(string[] args, Action<IServiceCollection>? testServiceConfig = null)
    {
        // ASCII Art Logo
        AnsiConsole.MarkupLine("""
[cyan]╔════════════════════════════════════════════════════════════════════╗[/]
[cyan]║[/]  [yellow]____                          _  ___ _   _____  ___   ___   ___ [/] [cyan] ║[/]
[cyan]║[/] [yellow]|  _ \ _____      _____ _ __  | |/ (_) |_|___ / / _ \ / _ \ / _ \ [/] [cyan]║[/]
[cyan]║[/] [yellow]| |_) / _ \ \ /\ / / _ \ '__| | ' /| | __| |_ \| | | | | | | | | |[/] [cyan]║[/]
[cyan]║[/] [yellow]|  __/ (_) \ V  V /  __/ |    | . \| | |_ ___) | |_| | |_| | |_| |[/] [cyan]║[/]
[cyan]║[/] [yellow]|_|   \___/ \_/\_/ \___|_|    |_|\_\_|_\__|____/ \___/ \___/ \___/ [/][cyan]║[/]
[cyan]║[/]                                                                    [cyan]║[/]
[cyan]║[/]                   [grey]🔧[/] [white]Universal Toolkit Suite[/] [red]🚀[/]                    [cyan]║[/]
[cyan]╚════════════════════════════════════════════════════════════════════╝[/]
""");
        AnsiConsole.MarkupLine("欢迎使用 [yellow]powerkit3000[/] CLI！");

        var host = Host.CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddSimpleFileLogging(context.Configuration);
            })
            .ConfigureServices((context, services) =>
            {
                if (testServiceConfig == null) // Only use Npgsql if not in test mode
                {
                    var connectionString = context.Configuration.GetConnectionString("AppDb");
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException("Connection string 'AppDb' was not found. Please configure it via appsettings.json or environment variables.");
                    }

                    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
                }
                else
                {
                    testServiceConfig(services); // Apply test-specific service configuration
                }
                services.Configure<TranslationOptions>(context.Configuration.GetSection(TranslationOptions.SectionName));
                services.AddSingleton<ITranslationProvider, NoOpTranslationProvider>();
                services.AddSingleton<ITranslationProvider, OpenAiTranslationProvider>();
                services.AddSingleton<ITranslationProvider, GeminiTranslationProvider>();
                services.AddSingleton<ITranslationProvider, DeepSeekTranslationProvider>();
                services.AddSingleton<ITranslationService, TranslationService>();

                services.AddScoped<KickstarterDataImportService>(provider =>
                    new KickstarterDataImportService(
                        provider.GetRequiredService<AppDbContext>(),
                        provider.GetRequiredService<ILogger<KickstarterDataImportService>>()
                    ));
                
                // Register command handlers
                services.AddScoped<ImportCommand>();
                services.AddScoped<QueryCommand>();
                services.AddScoped<CountsCommand>();
                services.AddScoped<ClearDbCommand>();
                services.AddScoped<SplitCommand>();
                services.AddScoped<TranslateMissingCommand>();
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            if (args.Length > 0) // 非交互模式: 执行命令并退出
            {
                await ExecuteCommand(args, services, logger);
            }
            else // 交互模式: REPL
            {
                ShowCommandTable();

                while (true)
                {
                    AnsiConsole.Markup("> "); // 提示符
                    string? input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    string[] inputArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string commandName = inputArgs[0].ToLowerInvariant();

                    if (commandName == "exit" || commandName == "quit")
                    {
                        AnsiConsole.MarkupLine("[red]退出 powerkit3000 CLI。[/]");
                        break;
                    }

                    await ExecuteCommand(inputArgs, services, logger);
                }
            }
        }
    }

    private static void ShowCommandTable(bool isHelpCommand = false)
    {
        var table = new Table();
        table.Title = new TableTitle("可用命令");
        table.Border = TableBorder.Rounded;
        table.AddColumn(new TableColumn("[yellow]命令[/]").LeftAligned());
        table.AddColumn(new TableColumn("[cyan]描述[/]"));

        string importDesc = isHelpCommand ? "从指定文件或目录导入 Kickstarter 数据。" : "从指定文件或目录导入 Kickstarter 数据。";
        string importCommand = isHelpCommand ? "import <文件/目录路径>" : "import <文件路径>";

        table.AddRow(importCommand, importDesc);
        table.AddRow("query <查询参数...>", "查询数据库中的 Kickstarter 项目。");
        table.AddRow("counts", "显示数据库中 Kickstarter 项目、创建者、类别和位置的数量。");
        table.AddRow("clear-db", "清空数据库中的所有 Kickstarter 相关数据。");
        table.AddRow("split <文件路径> <拆分数量>", "将一个 JSON 文件拆分为指定数量的小文件。");
        table.AddRow("translate [options]", "翻译缺少中文名称/简介的项目字段。");
        table.AddRow("exit / quit", "退出 CLI。");

        AnsiConsole.Write(table);
    }

    private static async Task ExecuteCommand(string[] args, IServiceProvider services, ILogger<Program> logger)
    {
        var command = args[0];
        var commandArgs = args.Skip(1).ToArray();
        logger.LogInformation("执行命令 {Command}，参数：{Args}", command, commandArgs);
        AnsiConsole.MarkupLine($"正在执行命令：[bold blue]{command}[/]");

        try
        {
            switch (command)
            {
                case "import":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("import 命令缺少文件路径参数。");
                        AnsiConsole.MarkupLine("[red]请为 import 命令指定文件路径。[/]");
                        return;
                    }
                    var importCommand = services.GetRequiredService<ImportCommand>();
                    await importCommand.ExecuteAsync(args[1]);
                    break;

                case "query":
                    var queryCommand = services.GetRequiredService<QueryCommand>();
                    await queryCommand.ExecuteAsync(args.Skip(1).ToArray());
                    break;

                case "counts":
                    var countsCommand = services.GetRequiredService<CountsCommand>();
                    await countsCommand.ExecuteAsync();
                    break;

                case "clear-db":
                    var clearDbCommand = services.GetRequiredService<ClearDbCommand>();
                    await clearDbCommand.ExecuteAsync();
                    break;

                case "split":
                    if (args.Length < 3 || !int.TryParse(args[2], out int numberOfFiles))
                    {
                        logger.LogWarning("split 命令参数无效：{Args}", string.Join(' ', args.Skip(1)));
                        AnsiConsole.MarkupLine("[red]请为 split 命令指定文件路径和拆分数量。用法: split <文件路径> <拆分数量>[/]");
                        return;
                    }
                    var splitCommand = services.GetRequiredService<SplitCommand>();
                    await splitCommand.ExecuteAsync(args[1], numberOfFiles);
                    break;

                case "translate":
                    var translateCommand = services.GetRequiredService<TranslateMissingCommand>();
                    await translateCommand.ExecuteAsync(args.Skip(1).ToArray());
                    break;

                case "help":
                    ShowCommandTable(isHelpCommand: true);
                    break;

                default:
                    logger.LogWarning("收到未知命令 {Command}", command);
                    AnsiConsole.MarkupLine($"[red]未知命令：{command}[/]");
                    break;
            }

            logger.LogInformation("命令 {Command} 执行完成。", command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "执行命令 {Command} 时发生错误。", command);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
        }
    }
}
