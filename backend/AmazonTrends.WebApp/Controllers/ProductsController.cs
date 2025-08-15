using AmazonTrends.Core.Services;
using AmazonTrends.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

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

/// <summary>
/// 用于表示核心指标概览的 DTO。
/// </summary>
public class CoreMetricsDto
{
    public long DataCollectionRunId { get; set; }
    public DateTime AnalysisTime { get; set; }
    public int TotalNewEntries { get; set; }
    public int TotalRankSurges { get; set; }
    public int TotalConsistentPerformers { get; set; }
    public int TotalProductsAnalyzed { get; set; }
}


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ScrapingService _scrapingService;
    private readonly AnalysisService _analysisService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IDistributedCache _cache;

    public ProductsController(AppDbContext dbContext, ScrapingService scrapingService, AnalysisService analysisService, ILogger<ProductsController> logger, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _scrapingService = scrapingService;
        _analysisService = analysisService;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// 获取产品列表，支持按分类和关键词进行筛选。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int? categoryId = null, [FromQuery] string? searchTerm = null)
    {
        _logger.LogInformation("获取产品列表，分类ID: {CategoryId}，搜索词: {SearchTerm}。", categoryId, searchTerm);

        // 只有在没有筛选条件时才使用缓存
        bool useCache = !categoryId.HasValue && string.IsNullOrEmpty(searchTerm);
        string cacheKey = $"Products_Category_{categoryId}_Search_{searchTerm}";

        if (useCache)
        {
            string? cachedProductsJson = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProductsJson))
            {
                _logger.LogInformation("从缓存中获取产品数据。");
                var cachedProducts = JsonSerializer.Deserialize<List<ProductDto>>(cachedProductsJson);
                return Ok(cachedProducts);
            }
        }

        _logger.LogInformation("从数据库中查询产品数据。");
        var query = _dbContext.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.Title.Contains(searchTerm) || p.Id.Contains(searchTerm));
        }

        var productsWithLatestData = await query
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

        // 只有在没有筛选条件时才写入缓存
        if (useCache)
        {
            _logger.LogInformation("将产品数据写入缓存。");
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productsWithLatestData), options);
        }

        return Ok(productsWithLatestData);
    }

    /// <summary>
    /// 获取核心指标概览。
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetCoreMetrics()
    {
        _logger.LogInformation("获取核心指标概览。");

        // 获取最新的分析结果
        var latestAnalysisResult = await _dbContext.AnalysisResults
            .OrderByDescending(ar => ar.AnalysisTime)
            .Include(ar => ar.Trends)
            .FirstOrDefaultAsync();

        if (latestAnalysisResult == null)
        {
            _logger.LogWarning("未找到任何分析结果。");
            return NotFound("未找到任何分析结果。");
        }

        var metrics = new CoreMetricsDto
        {
            DataCollectionRunId = latestAnalysisResult.DataCollectionRunId,
            AnalysisTime = latestAnalysisResult.AnalysisTime,
            TotalNewEntries = latestAnalysisResult.Trends.Count(t => t.TrendType == "NewEntry"),
            TotalRankSurges = latestAnalysisResult.Trends.Count(t => t.TrendType == "RankSurge"),
            TotalConsistentPerformers = latestAnalysisResult.Trends.Count(t => t.TrendType == "ConsistentPerformer"),
            TotalProductsAnalyzed = await _dbContext.ProductDataPoints
                .Where(dp => dp.DataCollectionRunId == latestAnalysisResult.DataCollectionRunId)
                .Select(dp => dp.ProductId)
                .Distinct()
                .CountAsync()
        };

        return Ok(metrics);
    }

    /// <summary>
    /// 获取最新的产品趋势（新上榜、排名飙升、持续霸榜）。
    /// </summary>
    /// <param name="trendType">可选：趋势类型过滤（NewEntry, RankSurge, ConsistentPerformer）。</param>
    [HttpGet("trends")]
    public async Task<IActionResult> GetLatestTrends([FromQuery] string? trendType = null)
    {
        _logger.LogInformation("获取最新的产品趋势，类型: {TrendType}.", trendType ?? "所有");

        var latestAnalysisResult = await _dbContext.AnalysisResults
            .OrderByDescending(ar => ar.AnalysisTime)
            .Include(ar => ar.Trends)
                .ThenInclude(t => t.Product)
            .FirstOrDefaultAsync();

        if (latestAnalysisResult == null)
        {
            _logger.LogWarning("未找到任何分析结果，无法获取趋势。");
            return NotFound("未找到任何分析结果。");
        }

        var trends = latestAnalysisResult.Trends.AsQueryable();

        if (!string.IsNullOrEmpty(trendType))
        {
            trends = trends.Where(t => t.TrendType == trendType);
        }

        var trendDtos = trends.Select(t => new
        {
            t.ProductId,
            t.Product.Title,
            t.TrendType,
            t.Description,
            t.AnalysisTime
        }).ToList();

        return Ok(trendDtos);
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
