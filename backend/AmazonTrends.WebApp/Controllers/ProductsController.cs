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
    /// 根据 ASIN 获取单个产品的完整信息。
    /// </summary>
    /// <param name="asin">产品的 ASIN。</param>
    [HttpGet("{asin}")]
    public async Task<IActionResult> GetProductByAsin(string asin)
    {
        _logger.LogInformation("获取产品 {Asin} 的详情。", asin);
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == asin);

        if (product == null)
        {
            _logger.LogWarning("未找到产品 ASIN: {Asin}。", asin);
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// 根据 ASIN 获取单个产品的历史数据点，用于绘制图表。
    /// </summary>
    /// <param name="asin">产品的 ASIN。</param>
    [HttpGet("{asin}/history")]
    public async Task<IActionResult> GetProductHistory(string asin)
    {
        _logger.LogInformation("获取产品 {Asin} 的历史数据点。", asin);
        var history = await _dbContext.ProductDataPoints
            .Where(p => p.ProductId == asin)
            .OrderBy(p => p.Timestamp)
            .Select(p => new
            {
                p.Timestamp,
                p.Rank,
                p.Price,
                p.Rating,
                p.ReviewsCount
            })
            .ToListAsync();

        if (!history.Any())
        {
            _logger.LogWarning("未找到产品 ASIN: {Asin} 的任何历史数据。", asin);
            return NotFound();
        }
        return Ok(history);
    }
}
