
using HappyTools.Data;
using HappyTools.Data.Models;
using System.Text;
using System.Text.Json;

namespace HappyTools.Core.Services
{
    public class KickstarterDataImportService
    {
        private readonly AppDbContext _context;

        public KickstarterDataImportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ImportDataAsync(string filePath)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            string fileContent = Encoding.UTF8.GetString(fileBytes);

            // Find the first '{' character to skip any leading garbage
            int firstBraceIndex = fileContent.IndexOf('{');
            if (firstBraceIndex == -1)
            {
                throw new InvalidOperationException("No JSON object found in the file.");
            }
            fileContent = fileContent.Substring(firstBraceIndex);

            string[] lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string processedLine = line.Trim(); // Trim whitespace

                Console.WriteLine($"DEBUG: Problematic line (after aggressive trim and first brace search): [{processedLine}]");

                // Specific workaround for the '[{' issue (if it still appears)
                if (!string.IsNullOrEmpty(processedLine) && processedLine.StartsWith("["))
                {
                    processedLine = processedLine.Substring(1); // Remove the leading '['
                }

                var jsonDocument = JsonDocument.Parse(processedLine);
                var data = jsonDocument.RootElement.GetProperty("data");

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
                        IsInPostCampaignPledgingPhase = data.TryGetProperty("is_in_post_campaign_pledging_phase", out var isPledgingPhase) && isPledgingPhase.ValueKind != JsonValueKind.Null ? isPledgingPhase.GetBoolean() : null,
                        StaffPick = data.GetProperty("staff_pick").GetBoolean(),
                        IsStarrable = data.GetProperty("is_starrable").GetBoolean(),
                        DisableCommunication = data.GetProperty("disable_communication").GetBoolean(),
                        StaticUsdRate = data.GetProperty("static_usd_rate").GetDecimal(),
                        ConvertedPledgedAmount = data.GetProperty("converted_pledged_amount").GetDecimal(),
                        FxRate = data.GetProperty("fx_rate").GetDecimal(),
                        UsdExchangeRate = data.GetProperty("usd_exchange_rate").GetDecimal(),
                        CurrentCurrency = data.GetProperty("current_currency").GetString(),
                        UsdType = data.GetProperty("usd_type").GetString(),
                        Spotlight = data.GetProperty("spotlight").GetBoolean(),
                        PercentFunded = data.GetProperty("percent_funded").GetDecimal(),
                        IsLiked = data.GetProperty("is_liked").GetBoolean(),
                        IsDisliked = data.TryGetProperty("is_disliked", out var isDisliked) && isDisliked.ValueKind != JsonValueKind.Null ? isDisliked.GetBoolean() : null,
                        IsLaunched = data.TryGetProperty("is_launched", out var isLaunched) && isLaunched.ValueKind != JsonValueKind.Null ? isLaunched.GetBoolean() : null,
                        PrelaunchActivated = data.TryGetProperty("prelaunch_activated", out var prelaunchActivated) && prelaunchActivated.ValueKind != JsonValueKind.Null ? prelaunchActivated.GetBoolean() : null,
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
                    if (locationData.ValueKind != JsonValueKind.Null)
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

                    _context.KickstarterProjects.Add(project);
                }
            await _context.SaveChangesAsync();
        }
    }
}
