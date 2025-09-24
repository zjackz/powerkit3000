using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using powerkit3000.core.translations;
using powerkit3000.data;
using powerkit3000.data.Models;

namespace consoleapp.Commands;

public class TranslateMissingCommand
{
    private readonly AppDbContext _dbContext;
    private readonly ITranslationService _translationService;
    private readonly TranslationOptions _options;
    private readonly ILogger<TranslateMissingCommand> _logger;

    public TranslateMissingCommand(
        AppDbContext dbContext,
        ITranslationService translationService,
        IOptions<TranslationOptions> options,
        ILogger<TranslateMissingCommand> logger)
    {
        _dbContext = dbContext;
        _translationService = translationService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            PrintHelp();
            return;
        }

        var (batchSize, maxProjects, dryRun) = ParseArguments(args);

        Console.WriteLine($"翻译命令启动：Provider={_options.Provider}, 批量={batchSize}, 最大项目数={maxProjects?.ToString() ?? "不限"}, DryRun={dryRun}");

        var pendingQuery = _dbContext.KickstarterProjects
            .Where(NeedsTranslationExpression())
            .OrderBy(p => p.Id);

        var totalPending = await pendingQuery.CountAsync(cancellationToken);
        if (totalPending == 0)
        {
            Console.WriteLine("暂无需要翻译的项目，已退出。");
            return;
        }

        Console.WriteLine($"待翻译项目数：{totalPending}");

        var processedProjects = 0;
        var translatedFields = 0;
        var failedFields = 0;
        long? lastId = null;

        while (true)
        {
            var effectiveBatchSize = batchSize;
            if (maxProjects.HasValue)
            {
                var remainingCapacity = maxProjects.Value - processedProjects;
                if (remainingCapacity <= 0)
                {
                    break;
                }

                effectiveBatchSize = Math.Min(effectiveBatchSize, remainingCapacity);
            }

            var batch = await _dbContext.KickstarterProjects
                .Where(NeedsTranslationExpression())
                .Where(p => !lastId.HasValue || p.Id > lastId.Value)
                .OrderBy(p => p.Id)
                .Take(effectiveBatchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            var translationInputs = BuildTranslationInputs(batch);
            if (translationInputs.Count == 0)
            {
                lastId = batch.Last().Id;
                processedProjects += batch.Count;
                continue;
            }

            var translationResults = await _translationService.TranslateAsync(translationInputs, cancellationToken);
            var resultLookup = translationResults
                .Where(r => !string.IsNullOrWhiteSpace(r.Identifier))
                .ToDictionary(r => r.Identifier!, r => r, StringComparer.OrdinalIgnoreCase);

            foreach (var project in batch)
            {
                var nameKey = BuildIdentifier(project.Id, "name");
                var blurbKey = BuildIdentifier(project.Id, "blurb");

                if (resultLookup.TryGetValue(nameKey, out var nameResult))
                {
                    if (nameResult.Success && !string.IsNullOrWhiteSpace(nameResult.TranslatedText))
                    {
                        project.NameCn = nameResult.TranslatedText;
                        translatedFields++;
                    }
                    else if (!nameResult.Success)
                    {
                        failedFields++;
                    }
                }

                if (resultLookup.TryGetValue(blurbKey, out var blurbResult))
                {
                    if (blurbResult.Success && !string.IsNullOrWhiteSpace(blurbResult.TranslatedText))
                    {
                        project.BlurbCn = blurbResult.TranslatedText;
                        translatedFields++;
                    }
                    else if (!blurbResult.Success)
                    {
                        failedFields++;
                    }
                }
            }

            if (dryRun)
            {
                Console.WriteLine("Dry-run 模式：以下为首条翻译预览");
                var sample = batch.First();
                Console.WriteLine($"项目 {sample.Id}:\n  原名称: {sample.Name}\n  新名称: {sample.NameCn ?? "<未翻译>"}\n  原简介: {TrimPreview(sample.Blurb)}\n  新简介: {TrimPreview(sample.BlurbCn)}");
            }
            else
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            processedProjects += batch.Count;
            lastId = batch.Last().Id;
            Console.WriteLine($"已处理 {processedProjects} 个项目，累计写入 {translatedFields} 个字段，失败 {failedFields} 个。");
        }

        if (dryRun)
        {
            _dbContext.ChangeTracker.Clear();
        }

        Console.WriteLine($"翻译完成。共处理 {processedProjects} 个项目，写入 {translatedFields} 个字段{(dryRun ? "（Dry-run 未写入数据库）" : string.Empty)}，失败 {failedFields} 个。");
    }

    private (int BatchSize, int? MaxProjects, bool DryRun) ParseArguments(string[] args)
    {
        var batchSize = _options.BatchSize;
        int? maxProjects = null;
        var dryRun = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--batch-size":
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out batchSize))
                    {
                        throw new ArgumentException("--batch-size 需要一个整数值。");
                    }
                    i++;
                    break;
                case "--max-projects":
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out var parsedMax))
                    {
                        throw new ArgumentException("--max-projects 需要一个整数值。");
                    }
                    maxProjects = parsedMax;
                    i++;
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
            }
        }

        if (batchSize <= 0)
        {
            throw new ArgumentException("批量大小必须大于 0。");
        }

        return (batchSize, maxProjects, dryRun);
    }

    private static System.Linq.Expressions.Expression<Func<KickstarterProject, bool>> NeedsTranslationExpression()
    {
        return project =>
            (!string.IsNullOrEmpty(project.Name) && string.IsNullOrEmpty(project.NameCn)) ||
            (!string.IsNullOrEmpty(project.Blurb) && string.IsNullOrEmpty(project.BlurbCn));
    }

    private List<TranslationInput> BuildTranslationInputs(IEnumerable<KickstarterProject> projects)
    {
        var list = new List<TranslationInput>();

        foreach (var project in projects)
        {
            if (!string.IsNullOrWhiteSpace(project.Name) && string.IsNullOrWhiteSpace(project.NameCn))
            {
                list.Add(new TranslationInput(project.Name, _options.TargetLanguage, _options.SourceLanguage, "project-name", BuildIdentifier(project.Id, "name")));
            }

            if (!string.IsNullOrWhiteSpace(project.Blurb) && string.IsNullOrWhiteSpace(project.BlurbCn))
            {
                list.Add(new TranslationInput(project.Blurb, _options.TargetLanguage, _options.SourceLanguage, "project-blurb", BuildIdentifier(project.Id, "blurb")));
            }
        }

        return list;
    }

    private static string BuildIdentifier(long projectId, string field) => $"{projectId}:{field}";

    private static string TrimPreview(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "<空>";
        }

        const int maxLength = 160;
        return content.Length <= maxLength
            ? content
            : content[..maxLength] + "...";
    }

    private static void PrintHelp()
    {
        Console.WriteLine("用法: translate [--batch-size <n>] [--max-projects <n>] [--dry-run]");
        Console.WriteLine("  --batch-size     每批处理的项目数量，默认读取配置中的 Translation:BatchSize");
        Console.WriteLine("  --max-projects   限制本次处理的项目总数，默认不限");
        Console.WriteLine("  --dry-run        仅执行翻译调用，不写入数据库，用于验证输出");
    }
}
