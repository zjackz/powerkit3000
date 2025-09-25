using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pk.core.Diagnostics;
using pk.data;
using pk.data.Models;

namespace pk.core.services
{
    public class KickstarterDataImportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<KickstarterDataImportService> _logger;
        private const int BatchSize = 1000; // Define batch size

        public KickstarterDataImportService(AppDbContext context, ILogger<KickstarterDataImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ImportDataAsync(string filePath, IProgress<int>? progress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var sourceTag = Path.GetFileName(filePath);
            var projectsToImport = ParseKickstarterProjectsFromFile(filePath, sourceTag).ToList();
            PowerKitMetrics.KickstarterImportFiles.Add(1, new TagList
            {
                { "source", sourceTag }
            });

            await ImportProjectsToDatabaseAsync(projectsToImport, progress, sourceTag);
            stopwatch.Stop();
            _logger.LogInformation(
                "导入任务完成。来源文件: {File}, 数据量: {Count}, 耗时: {ElapsedMs} ms",
                filePath,
                projectsToImport.Count,
                stopwatch.Elapsed.TotalMilliseconds);
        }

        public async Task ImportProjectsToDatabaseAsync(
            List<KickstarterProject> projectsToImport,
            IProgress<int>? progress = null,
            string? source = null)
        {
            var sourceTag = string.IsNullOrWhiteSpace(source) ? "unknown" : source;
            _logger.LogInformation(
                "开始导入 {Count} 条项目数据，来源 {Source}",
                projectsToImport.Count,
                sourceTag);

            var overallStopwatch = Stopwatch.StartNew();
            var skippedExisting = 0;
            var skippedDuplicates = 0;

            var originalAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                if (projectsToImport.Count == 0)
                {
                    progress?.Report(100);
                    _logger.LogInformation("输入集合为空，跳过导入。");
                    return;
                }

                int importedCount = 0;
                var totalProjects = projectsToImport.Count;

                for (int i = 0; i < totalProjects; i += BatchSize)
                {
                    var batchStopwatch = Stopwatch.StartNew();
                    var batch = projectsToImport.Skip(i).Take(BatchSize).ToList();
                    if (batch.Count == 0)
                    {
                        continue;
                    }

                    var projectIdsInBatch = batch.Select(p => p.Id).ToHashSet();
                    var creatorIdsInBatch = batch.Where(p => p.Creator != null).Select(p => p.Creator!.Id).ToHashSet();
                    var categoryIdsInBatch = batch.Where(p => p.Category != null).Select(p => p.Category!.Id).ToHashSet();
                    var locationIdsInBatch = batch.Where(p => p.Location != null).Select(p => p.Location!.Id).ToHashSet();

                    var existingProjectIds = (await _context.KickstarterProjects
                            .AsNoTracking()
                            .Where(p => projectIdsInBatch.Contains(p.Id))
                            .Select(p => p.Id)
                            .ToListAsync()).ToHashSet();
                    var existingCreatorIds = (await _context.Creators
                            .AsNoTracking()
                            .Where(c => creatorIdsInBatch.Contains(c.Id))
                            .Select(c => c.Id)
                            .ToListAsync()).ToHashSet();
                    var existingCategoryIds = (await _context.Categories
                            .AsNoTracking()
                            .Where(c => categoryIdsInBatch.Contains(c.Id))
                            .Select(c => c.Id)
                            .ToListAsync()).ToHashSet();
                    var existingLocationIds = (await _context.Locations
                            .AsNoTracking()
                            .Where(l => locationIdsInBatch.Contains(l.Id))
                            .Select(l => l.Id)
                            .ToListAsync()).ToHashSet();

                    var newProjects = new List<KickstarterProject>();
                    var newCreators = new List<Creator>();
                    var newCategories = new List<Category>();
                    var newLocations = new List<Location>();
                    var stagedProjectIds = new HashSet<long>();
                    var stagedCreators = new Dictionary<long, Creator>();
                    var stagedCategories = new Dictionary<long, Category>();
                    var stagedLocations = new Dictionary<long, Location>();

                    foreach (var project in batch)
                    {
                        if (existingProjectIds.Contains(project.Id))
                        {
                            skippedExisting++;
                            PowerKitMetrics.KickstarterImportSkippedProjects.Add(1, new TagList
                            {
                                { "source", sourceTag },
                                { "reason", "already_exists" }
                            });
                            _logger.LogDebug("项目ID {ProjectId} 已存在，跳过导入或考虑更新。", project.Id);
                            continue;
                        }

                        if (!stagedProjectIds.Add(project.Id))
                        {
                            skippedDuplicates++;
                            PowerKitMetrics.KickstarterImportSkippedProjects.Add(1, new TagList
                            {
                                { "source", sourceTag },
                                { "reason", "duplicate_in_batch" }
                            });
                            _logger.LogDebug("项目ID {ProjectId} 在当前批次中重复，已忽略重复条目。", project.Id);
                            continue;
                        }

                        if (!IsValidProject(project, out var validationReason))
                        {
                            PowerKitMetrics.KickstarterImportValidationErrors.Add(1, new TagList
                            {
                                { "source", sourceTag },
                                { "reason", validationReason }
                            });
                            _logger.LogWarning("项目ID {ProjectId} 校验失败（原因: {Reason}），已跳过。", project.Id, validationReason);
                            continue;
                        }

                        if (project.Creator != null)
                        {
                            project.CreatorId = project.Creator.Id;

                            if (existingCreatorIds.Contains(project.Creator.Id))
                            {
                                project.Creator = null;
                            }
                            else if (stagedCreators.TryGetValue(project.Creator.Id, out var existingCreator))
                            {
                                project.Creator = existingCreator;
                            }
                            else
                            {
                                stagedCreators[project.Creator.Id] = project.Creator;
                                newCreators.Add(project.Creator);
                            }
                        }

                        if (project.Category != null)
                        {
                            project.CategoryId = project.Category.Id;

                            if (existingCategoryIds.Contains(project.Category.Id))
                            {
                                project.Category = null;
                            }
                            else if (stagedCategories.TryGetValue(project.Category.Id, out var existingCategory))
                            {
                                project.Category = existingCategory;
                            }
                            else
                            {
                                stagedCategories[project.Category.Id] = project.Category;
                                newCategories.Add(project.Category);
                            }
                        }

                        if (project.Location != null)
                        {
                            project.LocationId = project.Location.Id;

                            if (existingLocationIds.Contains(project.Location.Id))
                            {
                                project.Location = null;
                            }
                            else if (stagedLocations.TryGetValue(project.Location.Id, out var existingLocation))
                            {
                                project.Location = existingLocation;
                            }
                            else
                            {
                                stagedLocations[project.Location.Id] = project.Location;
                                newLocations.Add(project.Location);
                            }
                        }

                        newProjects.Add(project);
                    }

                    if (newCreators.Any()) _context.Creators.AddRange(newCreators);
                    if (newCategories.Any()) _context.Categories.AddRange(newCategories);
                    if (newLocations.Any()) _context.Locations.AddRange(newLocations);
                    if (newProjects.Any()) _context.KickstarterProjects.AddRange(newProjects);

                    bool saveFailed;
                    do
                    {
                        saveFailed = false;
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateException ex)
                        {
                            saveFailed = true;
                            PowerKitMetrics.KickstarterImportFailures.Add(1, new TagList
                            {
                                { "source", sourceTag }
                            });
                            _logger.LogError(ex, "导入批次数据时发生数据库更新错误，批次起始索引: {StartIndex}。错误: {Message}", i, ex.Message);

                            foreach (var entry in ex.Entries)
                            {
                                PowerKitMetrics.KickstarterImportSkippedProjects.Add(1, new TagList
                                {
                                    { "source", sourceTag },
                                    { "reason", "db_update_failed" }
                                });
                                _logger.LogWarning(
                                    "实体 {EntityType} (状态: {EntityState}) 插入/更新失败，将尝试跳过。原因: {Message}",
                                    entry.Entity.GetType().Name,
                                    entry.State,
                                    ex.Message);
                                entry.State = EntityState.Detached;
                            }
                        }
                        catch (Exception ex)
                        {
                            saveFailed = false;
                            PowerKitMetrics.KickstarterImportFailures.Add(1, new TagList
                            {
                                { "source", sourceTag }
                            });
                            _logger.LogError(ex, "导入批次数据时发生未知错误，批次起始索引: {StartIndex}。错误: {Message}", i, ex.Message);
                        }
                    } while (saveFailed);

                    var successfulProjects = newProjects.Count(p => _context.Entry(p).State == EntityState.Unchanged);
                    if (successfulProjects > 0)
                    {
                        PowerKitMetrics.KickstarterImportedProjects.Add(successfulProjects, new TagList
                        {
                            { "source", sourceTag }
                        });
                    }

                    importedCount += successfulProjects;
                    PowerKitMetrics.KickstarterImportBatches.Add(1, new TagList
                    {
                        { "source", sourceTag }
                    });
                    PowerKitMetrics.KickstarterImportBatchDuration.Record(batchStopwatch.Elapsed.TotalMilliseconds, new TagList
                    {
                        { "source", sourceTag }
                    });

                    var percentage = totalProjects == 0
                        ? 100
                        : (int)((double)importedCount / totalProjects * 100);
                    _logger.LogInformation(
                        "批次完成。来源 {Source}，成功导入 {Imported} 条，累计 {ImportedTotal}/{Total}，耗时 {ElapsedMs} ms",
                        sourceTag,
                        successfulProjects,
                        importedCount,
                        totalProjects,
                        batchStopwatch.Elapsed.TotalMilliseconds);
                    progress?.Report(percentage);
                }

                overallStopwatch.Stop();
                _logger.LogInformation(
                    "所有数据导入完成。来源 {Source}，成功 {ImportedCount}，已存在 {ExistingSkipped}，批内重复 {DuplicateSkipped}，耗时 {ElapsedMs} ms",
                    sourceTag,
                    importedCount,
                    skippedExisting,
                    skippedDuplicates,
                    overallStopwatch.Elapsed.TotalMilliseconds);
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }

        private static bool IsValidProject(KickstarterProject project, out string reason)
        {
            if (string.IsNullOrWhiteSpace(project.Name))
            {
                reason = "missing_name";
                return false;
            }

            if (project.Goal < 0)
            {
                reason = "negative_goal";
                return false;
            }

            if (project.Pledged < 0)
            {
                reason = "negative_pledged";
                return false;
            }

            if (project.LaunchedAt == default)
            {
                reason = "missing_launched_at";
                return false;
            }

            if (project.Deadline == default)
            {
                reason = "missing_deadline";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private List<KickstarterProject> ParseKickstarterProjectsFromFile(string filePath, string sourceTag)
        {
            var parsedProjects = new List<KickstarterProject>();

            try
            {
                using var fileReader = File.OpenText(filePath);
                using var jsonReader = new JsonTextReader(fileReader)
                {
                    SupportMultipleContent = true
                };

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var jsonObject = JObject.Load(jsonReader);
                        var project = TryParseKickstarterRecord(jsonObject, sourceTag);
                        if (project != null)
                        {
                            parsedProjects.Add(project);
                        }
                        else
                        {
                            PowerKitMetrics.KickstarterImportParseErrors.Add(1, new TagList
                            {
                                { "source", sourceTag },
                                { "reason", "record_invalid" }
                            });
                        }
                    }
                }
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                _logger.LogError(ex, $"反序列化 JSON 内容时发生错误：{ex.Message}");
                PowerKitMetrics.KickstarterImportParseErrors.Add(1, new TagList
                {
                    { "source", sourceTag },
                    { "reason", "json_reader_exception" }
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"解析 Kickstarter 数据时发生错误：{ex.Message}");
                PowerKitMetrics.KickstarterImportParseErrors.Add(1, new TagList
                {
                    { "source", sourceTag },
                    { "reason", "json_parse_exception" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取或解析文件时发生未知错误：{ex.Message}");
                PowerKitMetrics.KickstarterImportParseErrors.Add(1, new TagList
                {
                    { "source", sourceTag },
                    { "reason", "unknown_exception" }
                });
            }

            _logger.LogInformation("从文件 {FilePath} 解析出 {Count} 条 Kickstarter 记录。", filePath, parsedProjects.Count);
            return parsedProjects;
        }

        private KickstarterProject? TryParseKickstarterRecord(JObject jsonObject, string sourceTag)
        {
            if (jsonObject["data"] is not JObject data)
            {
                _logger.LogWarning("跳过缺少 data 字段的记录。");
                PowerKitMetrics.KickstarterImportParseErrors.Add(1, new TagList
                {
                    { "source", sourceTag },
                    { "reason", "missing_data" }
                });
                return null;
            }

            var projectId = data.Value<long?>("id");
            if (projectId == null)
            {
                _logger.LogWarning("跳过缺少项目ID的记录。");
                PowerKitMetrics.KickstarterImportParseErrors.Add(1, new TagList
                {
                    { "source", sourceTag },
                    { "reason", "missing_id" }
                });
                return null;
            }

            var project = new KickstarterProject
            {
                Id = projectId.Value,
                Name = data.Value<string>("name"),
                Blurb = data.Value<string>("blurb"),
                Goal = GetDecimalValue(data["goal"]),
                Pledged = GetDecimalValue(data["pledged"]),
                State = data.Value<string>("state"),
                Country = data.Value<string>("country"),
                Currency = data.Value<string>("currency"),
                Deadline = GetDateTimeFromUnix(data["deadline"]),
                CreatedAt = GetDateTimeFromUnix(data["created_at"]),
                LaunchedAt = GetDateTimeFromUnix(data["launched_at"]),
                BackersCount = data.Value<int?>("backers_count") ?? 0,
                UsdPledged = GetDecimalValue(data["usd_pledged"]),
                Photo = data["photo"]?.ToString(Formatting.None),
                Urls = data["urls"]?.ToString(Formatting.None),
                StateChangedAt = GetDateTimeFromUnix(data["state_changed_at"]),
                Slug = data.Value<string>("slug"),
                CountryDisplayableName = data.Value<string>("country_displayable_name"),
                CurrencySymbol = data.Value<string>("currency_symbol"),
                CurrencyTrailingCode = data.Value<bool?>("currency_trailing_code"),
                IsInPostCampaignPledgingPhase = data.Value<bool?>("is_in_post_campaign_pledging_phase"),
                StaffPick = data.Value<bool?>("staff_pick"),
                IsStarrable = data.Value<bool?>("is_starrable"),
                DisableCommunication = data.Value<bool?>("disable_communication"),
                StaticUsdRate = GetDecimalValue(data["static_usd_rate"]),
                ConvertedPledgedAmount = GetDecimalValue(data["converted_pledged_amount"]),
                FxRate = GetDecimalValue(data["fx_rate"]),
                UsdExchangeRate = GetDecimalValue(data["usd_exchange_rate"]),
                CurrentCurrency = data.Value<string>("current_currency"),
                UsdType = data.Value<string>("usd_type"),
                Spotlight = data.Value<bool?>("spotlight"),
                PercentFunded = GetDecimalValue(data["percent_funded"]),
                IsLiked = data.Value<bool?>("is_liked"),
                IsDisliked = data.Value<bool?>("is_disliked"),
                IsLaunched = data.Value<bool?>("is_launched"),
                PrelaunchActivated = data.Value<bool?>("prelaunch_activated"),
                SourceUrl = data.Value<string>("source_url"),
            };

            if (data["creator"] is JObject creatorData)
            {
                var creatorId = creatorData.Value<long?>("id");
                if (creatorId != null)
                {
                    var creator = new Creator
                    {
                        Id = creatorId.Value,
                        Name = creatorData.Value<string>("name"),
                    };
                    project.Creator = creator;
                    project.CreatorId = creator.Id;
                }
            }

            if (data["category"] is JObject categoryData)
            {
                var categoryId = categoryData.Value<long?>("id");
                if (categoryId != null)
                {
                    var category = new Category
                    {
                        Id = categoryId.Value,
                        Name = categoryData.Value<string>("name"),
                        Slug = categoryData.Value<string>("slug"),
                        ParentId = categoryData.Value<long?>("parent_id"),
                        ParentName = categoryData.Value<string>("parent_name"),
                    };
                    project.Category = category;
                    project.CategoryId = category.Id;
                }
            }

            if (data["location"] is JObject locationData && locationData.Type != JTokenType.Null)
            {
                var locationId = locationData.Value<long?>("id");
                if (locationId != null)
                {
                    var location = new Location
                    {
                        Id = locationId.Value,
                        Name = locationData.Value<string>("name"),
                        DisplayableName = locationData.Value<string>("displayable_name"),
                        Country = locationData.Value<string>("country"),
                        State = locationData.Value<string>("state"),
                        Type = locationData.Value<string>("type"),
                    };
                    project.Location = location;
                    project.LocationId = location.Id;
                }
            }

            return project;
        }

        private static decimal GetDecimalValue(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return 0;
            }

            return token.Type switch
            {
                JTokenType.Float or JTokenType.Integer => token.Value<decimal>(),
                JTokenType.String => decimal.TryParse(token.Value<string>(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0,
                _ => 0,
            };
        }

        private static DateTime GetDateTimeFromUnix(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return DateTime.UnixEpoch;
            }

            var seconds = token.Value<long>();
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }
    }
}
