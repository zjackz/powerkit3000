using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            var projectsToImport = ParseKickstarterProjectsFromFile(filePath).ToList(); // Convert to list to enable multiple iterations
            await ImportProjectsToDatabaseAsync(projectsToImport, progress);
        }

        public async Task ImportProjectsToDatabaseAsync(List<KickstarterProject> projectsToImport, IProgress<int>? progress = null)
        {
            _logger.LogInformation($"开始导入 {projectsToImport.Count} 条项目数据。");

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
                        if (!existingProjectIds.Contains(project.Id))
                        {
                            if (stagedProjectIds.Add(project.Id))
                            {
                                newProjects.Add(project);
                            }
                            else
                            {
                                _logger.LogWarning($"项目ID {project.Id} 在当前批次中重复，已忽略重复条目。");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"项目ID {project.Id} 已存在，跳过导入或考虑更新。");
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
                            _logger.LogError(ex, $"导入批次数据时发生数据库更新错误，批次起始索引: {i}。错误: {ex.Message}");

                            foreach (var entry in ex.Entries)
                            {
                                _logger.LogWarning($"实体 {entry.Entity.GetType().Name} (状态: {entry.State}) 插入/更新失败，将尝试跳过。原因: {ex.Message}");
                                entry.State = EntityState.Detached;
                            }
                        }
                        catch (Exception ex)
                        {
                            saveFailed = false;
                            _logger.LogError(ex, $"导入批次数据时发生未知错误，批次起始索引: {i}。错误: {ex.Message}");
                        }
                    } while (saveFailed);

                    importedCount += newProjects.Count(p => _context.Entry(p).State == EntityState.Unchanged);

                    var percentage = totalProjects == 0
                        ? 100
                        : (int)((double)importedCount / totalProjects * 100);
                    _logger.LogInformation($"已成功导入 {importedCount} / {totalProjects} 条数据。进度: {percentage}%");
                    progress?.Report(percentage);
                }

                _logger.LogInformation("所有数据导入完成。");
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }

        private List<KickstarterProject> ParseKickstarterProjectsFromFile(string filePath)
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
                        var project = TryParseKickstarterRecord(jsonObject);
                        if (project != null)
                        {
                            parsedProjects.Add(project);
                        }
                    }
                }
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                _logger.LogError(ex, $"反序列化 JSON 内容时发生错误：{ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"解析 Kickstarter 数据时发生错误：{ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取或解析文件时发生未知错误：{ex.Message}");
            }

            _logger.LogInformation("从文件 {FilePath} 解析出 {Count} 条 Kickstarter 记录。", filePath, parsedProjects.Count);
            return parsedProjects;
        }

        private KickstarterProject? TryParseKickstarterRecord(JObject jsonObject)
        {
            if (jsonObject["data"] is not JObject data)
            {
                _logger.LogWarning("跳过缺少 data 字段的记录。");
                return null;
            }

            var projectId = data.Value<long?>("id");
            if (projectId == null)
            {
                _logger.LogWarning("跳过缺少项目ID的记录。");
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
