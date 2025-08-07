using AmazonTrends.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AmazonTrends.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CategoriesController> _logger;
    private readonly IDistributedCache _cache;

    public CategoriesController(AppDbContext dbContext, ILogger<CategoriesController> logger, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// 获取所有分类。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        _logger.LogInformation("获取所有分类。");

        string cacheKey = "AllCategories";
        string? cachedCategoriesJson = await _cache.GetStringAsync(cacheKey);

        List<object>? categories;

        if (!string.IsNullOrEmpty(cachedCategoriesJson))
        {
            _logger.LogInformation("从缓存中获取分类数据。");
            categories = JsonSerializer.Deserialize<List<object>>(cachedCategoriesJson);
        }
        else
        {
            _logger.LogInformation("从数据库中获取分类数据并写入缓存。");
            categories = await _dbContext.Categories
                .Select(c => new { c.Id, c.Name, c.AmazonCategoryId })
                .ToListAsync();

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60)); // 缓存 60 分钟
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(categories), options);
        }

        return Ok(categories);
    }
}
