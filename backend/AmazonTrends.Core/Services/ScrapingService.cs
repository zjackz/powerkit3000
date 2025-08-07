using AmazonTrends.Data;
using AmazonTrends.Data.Models;
using Hangfire;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace AmazonTrends.Core.Services;

public enum BestsellerType
{
    BestSellers,
    NewReleases,
    MoversAndShakers
}

public class ScrapingService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScrapingService> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly Random _random = new Random();
    private readonly List<string> _userAgentList = new List<string>
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Safari/605.1.15",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36 Edg/109.0.1518.78",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36 Edg/109.0.1518.78",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0"
    };

    public ScrapingService(AppDbContext dbContext, HttpClient httpClient, ILogger<ScrapingService> logger, IBackgroundJobClient backgroundJobClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    /// <summary>
    /// 抓取亚马逊指定类目的 Best Sellers 榜单数据。
    /// </summary>
    /// <param name="categoryId">要抓取的分类ID。</param>
    /// <param name="bestsellerType">榜单类型（Best Sellers, New Releases, Movers & Shakers）。</param>
    /// <returns>本次数据采集运行记录的ID。</returns>
    public async Task<long> ScrapeBestsellersAsync(int categoryId, BestsellerType bestsellerType = BestsellerType.BestSellers)
    {
        _logger.LogInformation("开始抓取分类 {CategoryId} 的 {BestsellerType} 榜单。", categoryId, bestsellerType);

        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            _logger.LogWarning("未找到分类 ID: {CategoryId}，无法开始抓取。", categoryId);
            // 返回一个无效ID，表示任务未执行
            return -1;
        }

        // 构建亚马逊榜单页面的 URL
        string baseUrl = "https://www.amazon.com/";
        string pathSegment = bestsellerType switch
        {
            BestsellerType.BestSellers => "Best-Sellers/zgbs",
            BestsellerType.NewReleases => "new-releases/zgbs",
            BestsellerType.MoversAndShakers => "movers-and-shakers/zgbs",
            _ => "Best-Sellers/zgbs" // 默认值
        };
        string url = $"{baseUrl}{pathSegment}/{category.AmazonCategoryId}";

        var dataCollectionRun = new DataCollectionRun
        {
            Timestamp = DateTime.UtcNow,
            Status = "InProgress",
            CategoryId = categoryId,
            Category = category
        };
        _dbContext.DataCollectionRuns.Add(dataCollectionRun);
        await _dbContext.SaveChangesAsync();

        try
        {
            // User-Agent 轮换
            var userAgent = _userAgentList[_random.Next(_userAgentList.Count)];
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            // 请求节流：随机延迟 1-3 秒
            int delay = _random.Next(1000, 3000);
            _logger.LogInformation("请求 {Url} 前延迟 {Delay} 毫秒。", url, delay);
            await Task.Delay(delay);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string htmlContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            var dataPoints = new List<ProductDataPoint>();
            var productNodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'zg-grid-general-faceout')]");

            if (productNodes == null)
            {
                _logger.LogWarning("在分类 {CategoryId} 的页面上没有找到产品节点，URL: {Url}。页面结构可能已更改。", categoryId, url);
                dataCollectionRun.Status = "CompletedWithNoData";
                await _dbContext.SaveChangesAsync();
                return dataCollectionRun.Id;
            }

            foreach (var node in productNodes)
            {
                string? asin = ExtractAsin(node);
                if (string.IsNullOrEmpty(asin)) continue;

                string title = ExtractTitle(node);

                var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == asin);
                if (product == null)
                {
                    // 尝试获取准确的上市日期
                    DateTime? listingDate = await ExtractListingDateFromProductPage(asin);

                    product = new Product
                    {
                        Id = asin,
                        Title = title,
                        CategoryId = categoryId,
                        Category = category,
                        ListingDate = listingDate ?? DateTime.UtcNow // 如果无法获取，则使用当前时间
                    };
                    _dbContext.Products.Add(product);
                }
                else if (product.Title != title)
                {
                    product.Title = title; // 如果标题有变动，则更新
                }
                // 如果 ListingDate 为空，尝试再次获取
                if (product.ListingDate == null || product.ListingDate == default(DateTime))
                {
                    DateTime? newListingDate = await ExtractListingDateFromProductPage(asin);
                    if (newListingDate.HasValue)
                    {
                        product.ListingDate = newListingDate.Value;
                    }
                }

                var dataPoint = new ProductDataPoint
                {
                    ProductId = asin,
                    Timestamp = DateTime.UtcNow,
                    Rank = ExtractRank(node),
                    Price = ExtractPrice(node),
                    Rating = ExtractRating(node),
                    ReviewsCount = ExtractReviewsCount(node),
                    DataCollectionRunId = dataCollectionRun.Id
                };
                dataPoints.Add(dataPoint);
            }

            _dbContext.ProductDataPoints.AddRange(dataPoints);
            dataCollectionRun.Status = "Completed";
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("成功为分类 {CategoryId} 抓取并保存了 {Count} 个产品数据点。", categoryId, dataPoints.Count);

            // 抓取成功后，自动触发一个后台分析任务
            _backgroundJobClient.Enqueue<AnalysisService>(service => service.AnalyzeTrendsAsync(dataCollectionRun.Id));
            _logger.LogInformation("已为数据采集运行 {DataCollectionRunId} 触发了一个后台分析任务。", dataCollectionRun.Id);

            return dataCollectionRun.Id;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP 请求错误，无法抓取分类 {CategoryId} 的 Best Sellers 榜单: {Message}", categoryId, ex.Message);
            dataCollectionRun.Status = "Failed";
            await _dbContext.SaveChangesAsync();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "抓取分类 {CategoryId} 的 Best Sellers 榜单时发生未知错误: {Message}", categoryId, ex.Message);
            dataCollectionRun.Status = "Failed";
            await _dbContext.SaveChangesAsync();
            throw;
        }
    }

    private string ExtractAsin(HtmlNode productNode)
    {
        var linkNode = productNode.SelectSingleNode(".//a[contains(@class, 'a-link-normal')]");
        string? href = linkNode?.GetAttributeValue("href", "");
        if (string.IsNullOrEmpty(href)) return string.Empty;

        // ASIN 通常是 URL 中的 /dp/ 之后的部分
        var match = Regex.Match(href, @"/dp/([A-Z0-9]{10})");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private string ExtractTitle(HtmlNode productNode)
    {
        // 亚马逊的标题 class 经常变化，但通常包含 'p13n-sc-css-line-clamp'
        var titleNode = productNode.SelectSingleNode(".//div[contains(@class, '_cDEzb_p13n-sc-css-line-clamp-')]");
        if (titleNode == null)
        {
            // 备用选择器
            titleNode = productNode.SelectSingleNode(".//a/span/div");
        }
        return titleNode != null ? WebUtility.HtmlDecode(titleNode.InnerText.Trim()) : "未知标题";
    }

    private int ExtractRank(HtmlNode productNode)
    {
        // 尝试匹配排名文本，例如 "#1"
        var rankNode = productNode.SelectSingleNode(".//span[contains(@class, 'zg-bdg-text')]");
        if (rankNode == null)
        {
            // 备用选择器，有时排名可能在其他位置，例如直接在列表项的某个 div 中
            rankNode = productNode.SelectSingleNode(".//span[contains(@class, 'a-list-item')]//span[contains(@class, 'a-text-bold')]");
        }

        string rankText = rankNode?.InnerText.Trim() ?? "";
        if (string.IsNullOrEmpty(rankText))
        {
            _logger.LogWarning("未能从产品节点中提取排名文本。");
            return 0;
        }

        // 移除 "#" 符号并尝试解析
        rankText = rankText.Replace("#", "").Trim();
        if (int.TryParse(rankText, out int rank))
        {
            return rank;
        }
        _logger.LogWarning("无法将排名文本 '{RankText}' 解析为整数。", rankText);
        return 0;
    }

    private decimal ExtractPrice(HtmlNode productNode)
    {
        // 价格通常在 class 包含 'a-color-price' 的 span 中
        var priceNode = productNode.SelectSingleNode(".//span[contains(@class, 'a-color-price')]");
        if (priceNode == null)
        {
            // 备用选择器，有时价格在另一个结构里
            priceNode = productNode.SelectSingleNode(".//span[contains(@class, '_cDEzb_p13n-sc-price_')]");
        }
        if (priceNode == null)
        {
            _logger.LogWarning("未能从产品节点中提取价格节点。");
            return 0.00m;
        }

        string priceText = priceNode.InnerText.Trim();
        
        // 移除货币符号和逗号，然后解析
        priceText = Regex.Replace(priceText, @"[$,]", "");
        if (decimal.TryParse(priceText, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"), out decimal price))
        {
            return price;
        }
        _logger.LogWarning("无法将价格文本 '{PriceText}' 解析为 decimal。", priceText);
        return 0.00m;
    }

    private float ExtractRating(HtmlNode productNode)
    {
        var ratingNode = productNode.SelectSingleNode(".//span[contains(@class, 'a-icon-alt')]");
        if (ratingNode == null)
        {
            _logger.LogWarning("未能从产品节点中提取评分节点。");
            return 0.0f;
        }

        string ratingText = ratingNode.InnerText.Trim();
        
        // 正则表达式匹配 "4.5 out of 5 stars" 中的 "4.5"
        var match = Regex.Match(ratingText, @"^(\d+(\.\d+)?)");
        if (match.Success)
        {
            if (float.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rating))
            {
                return rating;
            }
            _logger.LogWarning("无法将评分文本 '{RatingText}' 解析为 float。", match.Groups[1].Value);
        }
        else
        {
            _logger.LogWarning("评分文本 '{RatingText}' 不符合预期格式。", ratingText);
        }
        return 0.0f;
    }

    private int ExtractReviewsCount(HtmlNode productNode)
    {
        // 评论数通常在链接的 span 中
        var reviewsNode = productNode.SelectSingleNode(".//a[contains(@href, 'customerReviews')]//span[@class='a-size-small']");
        if (reviewsNode == null)
        {
             reviewsNode = productNode.SelectSingleNode(".//span[contains(@class, 'a-size-small')]");
        }
        if (reviewsNode == null)
        {
            _logger.LogWarning("未能从产品节点中提取评论数节点。");
            return 0;
        }

        string reviewsText = reviewsNode.InnerText.Trim();
        // 移除逗号并尝试解析
        reviewsText = reviewsText.Replace(",", "");
        if (int.TryParse(reviewsText, out int reviewsCount))
        {
            return reviewsCount;
        }
        _logger.LogWarning("无法将评论数文本 '{ReviewsText}' 解析为整数。", reviewsText);
        return 0;
    }

    /// <summary>
    /// 从亚马逊产品详情页提取上市日期。
    /// </summary>
    /// <param name="asin">产品的 ASIN。</param>
    /// <returns>上市日期，如果未找到则为 null。</returns>
    private async Task<DateTime?> ExtractListingDateFromProductPage(string asin)
    {
        string productUrl = $"https://www.amazon.com/dp/{asin}";
        _logger.LogInformation("尝试从产品详情页 {ProductUrl} 提取上市日期。", productUrl);

        try
        {
            // User-Agent 轮换
            var userAgent = _userAgentList[_random.Next(_userAgentList.Count)];
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            // 请求节流：随机延迟 1-3 秒
            int delay = _random.Next(1000, 3000);
            _logger.LogInformation("请求 {ProductUrl} 前延迟 {Delay} 毫秒。", productUrl, delay);
            await Task.Delay(delay);

            var response = await _httpClient.GetAsync(productUrl);
            response.EnsureSuccessStatusCode();

            string htmlContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // 尝试查找上市日期，通常在产品信息部分，例如 "Date First Available" 或 "Best Sellers Rank" 附近
            // 这是一个通用的 XPath，可能需要根据实际页面结构调整
            var dateNode = htmlDocument.DocumentNode.SelectSingleNode("//th[contains(text(), 'Date First Available')]/following-sibling::td")
                           ?? htmlDocument.DocumentNode.SelectSingleNode("//li[contains(., 'Date First Available')]");

            if (dateNode != null)
            {
                string dateText = dateNode.InnerText.Trim();
                // 尝试从文本中解析日期，例如 "November 1, 2023"
                var match = Regex.Match(dateText, @"(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}");
                if (match.Success)
                {
                    if (DateTime.TryParseExact(match.Value, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime listingDate))
                    {
                        _logger.LogInformation("成功从 {ProductUrl} 提取上市日期: {ListingDate}", productUrl, listingDate);
                        return listingDate;
                    }
                }
            }
            _logger.LogWarning("未能从产品详情页 {ProductUrl} 提取到上市日期。", productUrl);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP 请求错误，无法抓取产品详情页 {ProductUrl}: {Message}", productUrl, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "抓取产品详情页 {ProductUrl} 时发生未知错误: {Message}", productUrl, ex.Message);
            return null;
        }
    }
}
