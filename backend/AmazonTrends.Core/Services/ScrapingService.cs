using AmazonTrends.Data;
using AmazonTrends.Data.Models;
using Hangfire;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace AmazonTrends.Core.Services;

public class ScrapingService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScrapingService> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

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
    /// <returns>本次数据采集运行记录的ID。</returns>
    public async Task<long> ScrapeBestsellersAsync(int categoryId)
    {
        _logger.LogInformation("开始抓取分类 {CategoryId} 的 Best Sellers 榜单。", categoryId);

        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            _logger.LogWarning("未找到分类 ID: {CategoryId}，无法开始抓取。", categoryId);
            // 返回一个无效ID，表示任务未执行
            return -1;
        }

        // 构建亚马逊 Best Sellers 页面的 URL
        string url = $"https://www.amazon.com/Best-Sellers/zgbs/{category.AmazonCategoryId}";

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
                    product = new Product
                    {
                        Id = asin,
                        Title = title,
                        CategoryId = categoryId,
                        Category = category,
                        ListingDate = DateTime.UtcNow
                    };
                    _dbContext.Products.Add(product);
                }
                else if (product.Title != title)
                {
                    product.Title = title; // 如果标题有变动，则更新
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
        var rankNode = productNode.SelectSingleNode(".//span[contains(@class, 'zg-bdg-text')]");
        string rankText = rankNode?.InnerText.Trim().Replace("#", "") ?? "0";
        int.TryParse(rankText, out int rank);
        return rank;
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
        string priceText = priceNode?.InnerText.Trim() ?? "$0.00";
        
        // 移除货币符号和逗号，然后解析
        priceText = Regex.Replace(priceText, @"[$,]", "");
        decimal.TryParse(priceText, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"), out decimal price);
        return price;
    }

    private float ExtractRating(HtmlNode productNode)
    {
        var ratingNode = productNode.SelectSingleNode(".//span[contains(@class, 'a-icon-alt')]");
        string ratingText = ratingNode?.InnerText.Trim() ?? "0.0 out of 5 stars";
        
        // 正则表达式匹配 "4.5 out of 5 stars" 中的 "4.5"
        var match = Regex.Match(ratingText, @"^(\d+(\.\d+)?)");
        if (match.Success)
        {
            float.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rating);
            return rating;
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
        string reviewsText = reviewsNode?.InnerText.Trim().Replace(",", "") ?? "0";
        int.TryParse(reviewsText, out int reviewsCount);
        return reviewsCount;
    }
}
