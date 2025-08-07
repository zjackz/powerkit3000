using AmazonTrends.Core.Services;
using AmazonTrends.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmazonTrends.WebApp.Controllers;

/// <summary>
/// 用于前端展示的产品数据传输对象 (DTO)。
/// </summary>
public class ProductDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public DateTime ListingDate { get; set; }
    public int? LatestRank { get; set; }
    public decimal? LatestPrice { get; set; }
    public float? LatestRating { get; set; }
    public int? LatestReviewsCount { get; set; }
    public DateTime? LastUpdated { get; set; }
}


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ScrapingService _scrapingService;
    private readonly AnalysisService _analysisService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext dbContext, ScrapingService scrapingService, AnalysisService analysisService, ILogger<ProductsController> logger)
    {
        _dbContext = dbContext;
        _scrapingService = scrapingService;
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有产品及其最新的数据点。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        _logger.LogInformation("获取所有产品及其最新数据点。");

        var productsWithLatestData = await _dbContext.Products
            .Include(p => p.Category)
            .Select(p => new {
                Product = p,
                LatestDataPoint = p.DataPoints.OrderByDescending(dp => dp.Timestamp).FirstOrDefault()
            })
            .Select(x => new ProductDto
            {
                Id = x.Product.Id,
                Title = x.Product.Title,
                CategoryName = x.Product.Category.Name,
                ListingDate = x.Product.ListingDate ?? default,
                LatestRank = x.LatestDataPoint != null ? x.LatestDataPoint.Rank : null,
                LatestPrice = x.LatestDataPoint != null ? x.LatestDataPoint.Price : null,
                LatestRating = x.LatestDataPoint != null ? x.LatestDataPoint.Rating : null,
                LatestReviewsCount = x.LatestDataPoint != null ? x.LatestDataPoint.ReviewsCount : null,
                LastUpdated = x.LatestDataPoint != null ? x.LatestDataPoint.Timestamp : null
            })
            .ToListAsync();

        return Ok(productsWithLatestData);
    }

    /// <summary>
    /// 根据 ASIN 获取单个产品详情及其历史数据点。
    /// </summary>
    /// <param name="asin">产品的 ASIN。</param>
    [HttpGet("{asin}")]
    public async Task<IActionResult> GetProductByAsin(string asin)
    {
        _logger.LogInformation("获取产品 {Asin} 的详情。", asin);
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.DataPoints.OrderBy(dp => dp.Timestamp))
            .FirstOrDefaultAsync(p => p.Id == asin);

        if (product == null)
        {
            _logger.LogWarning("未找到产品 ASIN: {Asin}。", asin);
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// 触发一次指定分类的 Best Sellers 榜单抓取。
    /// </summary>
    /// <param name="categoryId">要抓取的分类ID。</param>
    [HttpPost("scrape/{categoryId}")]
    public async Task<IActionResult> TriggerScrape(int categoryId)
    {
        _logger.LogInformation("手动触发分类 {CategoryId} 的数据抓取。", categoryId);
        try
        {
            var dataPoints = await _scrapingService.ScrapeBestsellersAsync(categoryId);
            return Ok($"成功触发抓取任务，抓取到 {dataPoints.Count} 个数据点。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "触发抓取任务失败: {Message}", ex.Message);
            return StatusCode(500, $"触发抓取任务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取指定分类的产品趋势分析结果。
    /// </summary>
    /// <param name="categoryId">要分析的分类ID。</param>
    [HttpGet("trends/{categoryId}")]
    public async Task<IActionResult> GetTrends(int categoryId)
    {
        _logger.LogInformation("获取分类 {CategoryId} 的趋势分析结果。", categoryId);
        try
        {
            var trends = await _analysisService.AnalyzeTrendsAsync(categoryId);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取趋势分析结果失败: {Message}", ex.Message);
            return StatusCode(500, $"获取趋势分析结果失败: {ex.Message}");
        }
    }
}
