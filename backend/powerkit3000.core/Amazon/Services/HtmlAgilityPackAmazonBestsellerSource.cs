using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using powerkit3000.core.Amazon.Contracts;
using powerkit3000.core.Amazon.Options;

namespace powerkit3000.core.Amazon.Services;

/// <summary>
/// 基于 HtmlAgilityPack 的 Amazon 榜单抓取器，负责发起请求并解析页面结构。
/// </summary>
public class HtmlAgilityPackAmazonBestsellerSource : IAmazonBestsellerSource
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HtmlAgilityPackAmazonBestsellerSource> _logger;
    private readonly AmazonModuleOptions _options;
    private readonly Random _random = new();

    private static readonly Regex AsinRegex = new("/dp/([A-Z0-9]{10})", RegexOptions.Compiled);
    private static readonly Regex PriceRegex = new("[$,]", RegexOptions.Compiled);
    private static readonly Regex RatingRegex = new("^(\\d+(?:.\\d+)?)", RegexOptions.Compiled);

    public HtmlAgilityPackAmazonBestsellerSource(
        HttpClient httpClient,
        IOptions<AmazonModuleOptions> options,
        ILogger<HtmlAgilityPackAmazonBestsellerSource> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// 抓取指定类目与榜单类型的商品列表。
    /// </summary>
    public async Task<IReadOnlyList<AmazonBestsellerEntry>> FetchAsync(string amazonCategoryId, Amazon.AmazonBestsellerType bestsellerType, CancellationToken cancellationToken)
    {
        var url = BuildUrl(amazonCategoryId, bestsellerType);
        await ApplyDelayAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.Clear();
        var userAgent = SelectUserAgent();
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
        }

        _logger.LogInformation("Fetching Amazon bestseller page {Url}", url);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var document = new HtmlDocument();
        document.LoadHtml(content);

        // 榜单页面存在多个布局，这里兼容常见的两种容器结构。
        var productNodes = document.DocumentNode
            .SelectNodes("//div[contains(@class, 'zg-grid-general-faceout')]")
            ?? document.DocumentNode.SelectNodes("//div[contains(@data-testid, 'grid-deal-card')]");

        if (productNodes == null)
        {
            _logger.LogWarning("No product nodes found for url {Url}", url);
            return Array.Empty<AmazonBestsellerEntry>();
        }

        var entries = new List<AmazonBestsellerEntry>(productNodes.Count);
        foreach (var node in productNodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var asin = ExtractAsin(node);
            if (asin == null)
            {
                continue;
            }

            var title = ExtractTitle(node);
            var rank = ExtractRank(node);
            var price = ExtractPrice(node);
            var rating = ExtractRating(node);
            var reviews = ExtractReviewsCount(node);
            var brand = ExtractBrand(node);
            var imageUrl = ExtractImageUrl(node);

            entries.Add(new AmazonBestsellerEntry(
                asin,
                title ?? "Unknown Title",
                brand,
                imageUrl,
                rank,
                price,
                rating,
                reviews,
                null));
        }

        return entries;
    }

    /// <summary>
    /// 根据榜单类型拼接 Amazon 榜单页地址。
    /// </summary>
    private string BuildUrl(string amazonCategoryId, Amazon.AmazonBestsellerType bestsellerType)
    {
        var path = bestsellerType switch
        {
            Amazon.AmazonBestsellerType.BestSellers => "Best-Sellers/zgbs",
            Amazon.AmazonBestsellerType.NewReleases => "new-releases/zgbs",
            Amazon.AmazonBestsellerType.MoversAndShakers => "movers-and-shakers/zgbs",
            _ => "Best-Sellers/zgbs"
        };

        return $"https://www.amazon.com/{path}/{amazonCategoryId}";
    }

    /// <summary>
    /// 依据配置的随机延迟，避免短时间内连续访问触发反爬虫。
    /// </summary>
    private async Task ApplyDelayAsync(CancellationToken cancellationToken)
    {
        var min = Math.Max(0, _options.MinDelayMilliseconds);
        var max = Math.Max(min, _options.MaxDelayMilliseconds);
        var delay = _random.Next(min, max + 1);
        if (delay > 0)
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    private string? SelectUserAgent()
    {
        if (_options.UserAgentPool is { Count: > 0 })
        {
            return _options.UserAgentPool[_random.Next(_options.UserAgentPool.Count)];
        }

        return _options.UserAgent;
    }

    /// <summary>
    /// 从节点的链接中提取 ASIN。
    /// </summary>
    private static string? ExtractAsin(HtmlNode node)
    {
        var linkNode = node.SelectSingleNode(".//a[contains(@class, 'a-link-normal')]")
            ?? node.SelectSingleNode(".//a[contains(@href, '/dp/')]");

        var href = linkNode?.GetAttributeValue("href", null);
        if (string.IsNullOrWhiteSpace(href))
        {
            return null;
        }

        var match = AsinRegex.Match(href);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// 提取商品标题，兼容不同版本的 HTML 结构。
    /// </summary>
    private static string? ExtractTitle(HtmlNode node)
    {
        var titleNode = node.SelectSingleNode(".//div[contains(@class, '_cDEzb_p13n-sc-css-line-clamp-')]")
            ?? node.SelectSingleNode(".//img[contains(@class, 'p13n-sc-dynamic-image')]/@alt")
            ?? node.SelectSingleNode(".//span[contains(@class, 'a-size-small')]");

        if (titleNode == null)
        {
            return null;
        }

        return HtmlEntity.DeEntitize(titleNode.InnerText?.Trim());
    }

    /// <summary>
    /// 解析榜单排名，失败时返回 0。
    /// </summary>
    private static int ExtractRank(HtmlNode node)
    {
        var rankNode = node.SelectSingleNode(".//span[contains(@class, 'zg-bdg-text')]")
            ?? node.SelectSingleNode(".//span[contains(@class, 'zg-badge-text')]");

        var text = rankNode?.InnerText?.Trim().TrimStart('#');
        return int.TryParse(text, out var rank) ? rank : 0;
    }

    /// <summary>
    /// 解析价格并转换为 decimal。
    /// </summary>
    private static decimal? ExtractPrice(HtmlNode node)
    {
        var priceNode = node.SelectSingleNode(".//span[contains(@class, 'a-color-price')]")
            ?? node.SelectSingleNode(".//span[contains(@class, '_cDEzb_p13n-sc-price_')]");

        var raw = priceNode?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        raw = PriceRegex.Replace(raw, string.Empty);
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
            ? price
            : null;
    }

    /// <summary>
    /// 解析评分值，兼容包含说明文本的格式。
    /// </summary>
    private static float? ExtractRating(HtmlNode node)
    {
        var ratingNode = node.SelectSingleNode(".//span[contains(@class, 'a-icon-alt')]");
        var raw = ratingNode?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var match = RatingRegex.Match(raw);
        return match.Success && float.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rating)
            ? rating
            : null;
    }

    /// <summary>
    /// 解析评论数量并移除格式字符。
    /// </summary>
    private static int? ExtractReviewsCount(HtmlNode node)
    {
        var reviewsNode = node.SelectSingleNode(".//span[contains(@class, 'a-size-small') and contains(@aria-label, 'rating')]/../span")
            ?? node.SelectNodes(".//span[contains(@class, 'a-size-small')]")?.LastOrDefault();

        var raw = reviewsNode?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        raw = raw.Replace(",", string.Empty);
        return int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var count) ? count : null;
    }

    /// <summary>
    /// 解析品牌信息。
    /// </summary>
    private static string? ExtractBrand(HtmlNode node)
    {
        var brandNode = node.SelectSingleNode(".//span[contains(@class, 'zg-brand-name')]");
        return HtmlEntity.DeEntitize(brandNode?.InnerText?.Trim());
    }

    /// <summary>
    /// 解析商品图片 URL。
    /// </summary>
    private static string? ExtractImageUrl(HtmlNode node)
    {
        var imgNode = node.SelectSingleNode(".//img");
        return imgNode?.GetAttributeValue("src", null);
    }
}
