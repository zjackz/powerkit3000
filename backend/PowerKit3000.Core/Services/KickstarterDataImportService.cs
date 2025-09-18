using PowerKit3000.Data;
using PowerKit3000.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace PowerKit3000.Core.Services
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

            // Disable change tracking for performance during bulk inserts
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            int importedCount = 0;
            for (int i = 0; i < projectsToImport.Count; i += BatchSize)
            {
                var batch = projectsToImport.Skip(i).Take(BatchSize).ToList();

                var projectIdsInBatch = batch.Select(p => p.Id).ToHashSet();
                var creatorIdsInBatch = batch.Select(p => p.Creator!.Id).ToHashSet();
                var categoryIdsInBatch = batch.Select(p => p.Category!.Id).ToHashSet();
                var locationIdsInBatch = batch.Where(p => p.Location != null).Select(p => p.Location!.Id).ToHashSet();

                var existingProjectIds = (await _context.KickstarterProjects
                    .Where(p => projectIdsInBatch.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync()).ToHashSet();
                var existingCreatorIds = (await _context.Creators
                    .Where(c => creatorIdsInBatch.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync()).ToHashSet();
                var existingCategoryIds = (await _context.Categories
                    .Where(c => categoryIdsInBatch.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync()).ToHashSet();
                var existingLocationIds = (await _context.Locations
                    .Where(l => locationIdsInBatch.Contains(l.Id))
                    .Select(l => l.Id)
                    .ToListAsync()).ToHashSet();

                var newProjects = new List<KickstarterProject>();
                var newCreators = new List<Creator>();
                var newCategories = new List<Category>();
                var newLocations = new List<Location>();

                foreach (var project in batch)
                {
                    if (!existingProjectIds.Contains(project.Id))
                    {
                        newProjects.Add(project);
                    }
                    else
                    {
                        _logger.LogWarning($"项目ID {project.Id} 已存在，跳过导入或考虑更新。");
                    }

                    if (project.Creator != null && !existingCreatorIds.Contains(project.Creator.Id))
                    {
                        newCreators.Add(project.Creator);
                    }

                    if (project.Category != null && !existingCategoryIds.Contains(project.Category.Id))
                    {
                        newCategories.Add(project.Category);
                    }

                    if (project.Location != null && !existingLocationIds.Contains(project.Location.Id))
                    {
                        newLocations.Add(project.Location);
                    }
                }

                // Add new entities in bulk
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
                            // Log the specific entity that failed
                            _logger.LogWarning($"实体 {entry.Entity.GetType().Name} (状态: {entry.State}) 插入/更新失败，将尝试跳过。原因: {ex.Message}");
                            // Detach the failed entity from the context
                            entry.State = EntityState.Detached;
                        }
                    }
                    catch (Exception ex)
                    {
                        saveFailed = false; // For other exceptions, we don't retry saving this batch
                        _logger.LogError(ex, $"导入批次数据时发生未知错误，批次起始索引: {i}。错误: {ex.Message}");
                    }
                } while (saveFailed);

                // Count only the newly imported projects that were successfully saved
                importedCount += newProjects.Count(p => _context.Entry(p).State == EntityState.Unchanged);

                var percentage = (int)((double)importedCount / projectsToImport.Count * 100);
                _logger.LogInformation($"已成功导入 {importedCount} / {projectsToImport.Count} 条数据。进度: {percentage}%");
                progress?.Report(percentage); // Report progress
            }
            _logger.LogInformation("所有数据导入完成。");

            // Re-enable change tracking
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        private List<KickstarterProject> ParseKickstarterProjectsFromFile(string filePath)
        {
            var parsedProjects = new List<KickstarterProject>();
            string jsonContent = File.ReadAllText(filePath);

            try
            {
                JArray jsonArray = JArray.Parse(jsonContent);
                foreach (JObject jsonObject in jsonArray.Children<JObject>())
                {
                    using (System.Text.Json.JsonDocument jsonDocument = System.Text.Json.JsonDocument.Parse(jsonObject.ToString(Newtonsoft.Json.Formatting.None)))
                    {
                        if (jsonDocument.RootElement.TryGetProperty("data", out System.Text.Json.JsonElement data))
                        {
                            var project = new KickstarterProject
                            {
                                Id = data.GetProperty("id").GetInt64(),
                                Name = data.GetProperty("name").GetString(),
                                Blurb = data.GetProperty("blurb").GetString(),
                                Goal = data.GetProperty("goal").GetDecimal(),
                                Pledged = data.GetProperty("pledged").GetDecimal(),
                                State = data.GetProperty("state").GetString(),
                                Country = data.GetProperty("country").GetString(),
                                Currency = data.GetProperty("currency").GetString(),
                                Deadline = DateTimeOffset.FromUnixTimeSeconds(data.GetProperty("deadline").GetInt64()).UtcDateTime,
                                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(data.GetProperty("created_at").GetInt64()).UtcDateTime,
                                LaunchedAt = DateTimeOffset.FromUnixTimeSeconds(data.GetProperty("launched_at").GetInt64()).UtcDateTime,
                                BackersCount = data.GetProperty("backers_count").GetInt32(),
                                UsdPledged = decimal.TryParse(data.GetProperty("usd_pledged").GetString(), out var usdPledged) ? usdPledged : 0,
                                Photo = data.GetProperty("photo").GetRawText(),
                                Urls = data.GetProperty("urls").GetRawText(),
                                StateChangedAt = DateTimeOffset.FromUnixTimeSeconds(data.GetProperty("state_changed_at").GetInt64()).UtcDateTime,
                                Slug = data.GetProperty("slug").GetString(),
                                CountryDisplayableName = data.GetProperty("country_displayable_name").GetString(),
                                CurrencySymbol = data.GetProperty("currency_symbol").GetString(),
                                CurrencyTrailingCode = data.GetProperty("currency_trailing_code").GetBoolean(),
                                IsInPostCampaignPledgingPhase = data.TryGetProperty("is_in_post_campaign_pledging_phase", out var isPledgingPhase) && isPledgingPhase.ValueKind != System.Text.Json.JsonValueKind.Null ? isPledgingPhase.GetBoolean() : null,
                                StaffPick = data.GetProperty("staff_pick").GetBoolean(),
                                IsStarrable = data.GetProperty("is_starrable").GetBoolean(),
                                DisableCommunication = data.GetProperty("disable_communication").GetBoolean(),
                                StaticUsdRate = data.GetProperty("static_usd_rate").GetDecimal(),
                                ConvertedPledgedAmount = data.TryGetProperty("converted_pledged_amount", out var convertedPledgedAmountElement) && convertedPledgedAmountElement.ValueKind != System.Text.Json.JsonValueKind.Null ? convertedPledgedAmountElement.GetDecimal() : 0,
                                FxRate = data.GetProperty("fx_rate").GetDecimal(),
                                UsdExchangeRate = data.GetProperty("usd_exchange_rate").GetDecimal(),
                                CurrentCurrency = data.GetProperty("current_currency").GetString(),
                                UsdType = data.GetProperty("usd_type").GetString(),
                                Spotlight = data.GetProperty("spotlight").GetBoolean(),
                                PercentFunded = data.GetProperty("percent_funded").GetDecimal(),
                                IsLiked = data.GetProperty("is_liked").GetBoolean(),
                                IsDisliked = data.TryGetProperty("is_disliked", out var isDisliked) && isDisliked.ValueKind != System.Text.Json.JsonValueKind.Null ? isDisliked.GetBoolean() : null,
                                IsLaunched = data.TryGetProperty("is_launched", out var isLaunched) && isLaunched.ValueKind != System.Text.Json.JsonValueKind.Null ? isLaunched.GetBoolean() : null,
                                PrelaunchActivated = data.TryGetProperty("prelaunch_activated", out var prelaunchActivated) && prelaunchActivated.ValueKind != System.Text.Json.JsonValueKind.Null ? prelaunchActivated.GetBoolean() : null,
                                SourceUrl = data.GetProperty("source_url").GetString(),
                            };

                            var creatorData = data.GetProperty("creator");
                            var creatorId = creatorData.GetProperty("id").GetInt64();
                            var creator = _context.Creators.Find(creatorId);
                            if (creator == null)
                            {
                                creator = new Creator
                                {
                                    Id = creatorId,
                                    Name = creatorData.GetProperty("name").GetString(),
                                };
                                _context.Creators.Add(creator);
                            }
                            project.Creator = creator;

                            var categoryData = data.GetProperty("category");
                            var categoryId = categoryData.GetProperty("id").GetInt64();
                            var category = _context.Categories.Find(categoryId);
                            if (category == null)
                            {
                                category = new Category
                                {
                                    Id = categoryId,
                                    Name = categoryData.GetProperty("name").GetString(),
                                    Slug = categoryData.GetProperty("slug").GetString(),
                                    ParentId = categoryData.TryGetProperty("parent_id", out var parentId) ? parentId.GetInt64() : null,
                                    ParentName = categoryData.TryGetProperty("parent_name", out var parentName) ? parentName.GetString() : null,
                                };
                                _context.Categories.Add(category);
                            }
                            project.Category = category;

                            var locationData = data.GetProperty("location");
                            if (locationData.ValueKind != System.Text.Json.JsonValueKind.Null)
                            {
                                var locationId = locationData.GetProperty("id").GetInt64();
                                var location = _context.Locations.Find(locationId);
                                if (location == null)
                                {
                                    location = new Location
                                    {
                                        Id = locationId,
                                        Name = locationData.GetProperty("name").GetString(),
                                        DisplayableName = locationData.GetProperty("displayable_name").GetString(),
                                        Country = locationData.GetProperty("country").GetString(),
                                        State = locationData.GetProperty("state").GetString(),
                                        Type = locationData.GetProperty("type").GetString(),
                                    };
                                    _context.Locations.Add(location);
                                }
                                project.Location = location;
                            }

                            parsedProjects.Add(project);
                        }
                    }
                }
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                _logger.LogError(ex, $"反序列化 JSON 数组时发生错误：{ex.Message}");
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, $"解析 JSON 数组时发生错误：{ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取或解析文件时发生未知错误：{ex.Message}");
            }
            return parsedProjects;
        }
    }
}
