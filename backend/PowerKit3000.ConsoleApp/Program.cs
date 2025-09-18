using PowerKit3000.Core.Services;
using PowerKit3000.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConsoleApp.Commands;
using Spectre.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ASCII Art Logo
        AnsiConsole.MarkupLine(@"
[cyan]╔══════════════════════════════════════════════════════════════════╗[/]
[cyan]║[/]  [yellow]____                          _  ___ _   _____  ___   ___   ___ [/] [cyan]║[/]
[cyan]║[/] [yellow]|  _ \ _____      _____ _ __  | |/ (_) |_|___ / / _ \ / _ \ / _ \ [/] [cyan]║[/]
[cyan]║[/] [yellow]| |_) / _ \ \ /\ / / _ \ '__| | ' /| | __| |_ \| | | | | | | | | |[/] [cyan]║[/]
[cyan]║[/] [yellow]|  __/ (_) \ V  V /  __/ |    | . \| | |_ ___) | |_| | |_| | |_| |[/] [cyan]║[/]
[cyan]║[/] [yellow]|_|   \___/ \_/\_/ \___|_|    |_|\_\_|_\__|____/ \___/ \___/ \___/ [/] [cyan]║[/]
[cyan]║[/]                                                                  [cyan]║[/]
[cyan]║[/]                   [grey]🔧[/] [white]Universal Toolkit Suite[/] [red]🚀[/]                  [cyan]║[/]
[cyan]╚══════════════════════════════════════════════════════════════════╝[/]");
        AnsiConsole.MarkupLine("欢迎使用 [yellow]PowerKit3000[/] CLI！");
        AnsiConsole.WriteLine("-----------------------------------");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql("Host=192.168.0.124;Port=5432;Database=postgres;Username=postgres;Password=123321"));
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
                AnsiConsole.WriteLine("-----------------------------------");

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
                        AnsiConsole.MarkupLine("[red]退出 PowerKit3000 CLI。[/]");
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
        table.AddRow("exit / quit", "退出 CLI。");

        AnsiConsole.Write(table);
    }

    private static async Task ExecuteCommand(string[] args, IServiceProvider services, ILogger<Program> logger)
    {
        var command = args[0];
        AnsiConsole.MarkupLine($"正在执行命令：[bold blue]{command}[/]");

        try
        {
            switch (command)
            {
                case "import":
                    if (args.Length < 2)
                    {
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
                        AnsiConsole.MarkupLine("[red]请为 split 命令指定文件路径和拆分数量。用法: split <文件路径> <拆分数量>[/]");
                        return;
                    }
                    var splitCommand = services.GetRequiredService<SplitCommand>();
                    await splitCommand.ExecuteAsync(args[1], numberOfFiles);
                    break;

                case "help":
                    ShowCommandTable(isHelpCommand: true);
                    break;

                default:
                    AnsiConsole.MarkupLine($"[red]未知命令：{command}[/]");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"执行命令时发生错误：{command}");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
        }
    }
}