using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pk.core.Amazon.Contracts;
using pk.core.Amazon.Options;
using pk.data;
using pk.data.Models;

namespace pk.core.Amazon.Services;

/// <summary>
/// 负责 Amazon 榜单类目的同步与榜单快照的采集入库，是 Amazon 数据流的入口服务。
/// </summary>
public class AmazonIngestionService
{
    private readonly AppDbContext _dbContext;
    private readonly IAmazonBestsellerSource _bestsellerSource;
    private readonly AmazonModuleOptions _options;
    private readonly ILogger<AmazonIngestionService> _logger;

    public AmazonIngestionService(
        AppDbContext dbContext,
        IAmazonBestsellerSource bestsellerSource,
        IOptions<AmazonModuleOptions> options,
        ILogger<AmazonIngestionService> logger)
    {
        _dbContext = dbContext;
        _bestsellerSource = bestsellerSource;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 将配置的 Amazon 类目写入数据库，如存在则更新名称，保证本地类目表与配置保持一致。
    /// </summary>
    public async Task<int> EnsureCategoriesAsync(CancellationToken cancellationToken)
    {
        var configuredCategories = _options.Categories;
        if (configuredCategories.Count == 0)
        {
            return 0;
        }

        var affected = 0;
        foreach (var categoryOption in configuredCategories)
        {
            cancellationToken.ThrowIfCancellationRequested(); // 允许上层在长时间运行时及时取消操作

            var existing = await _dbContext.AmazonCategories
                .FirstOrDefaultAsync(c => c.AmazonCategoryId == categoryOption.AmazonCategoryId, cancellationToken)
                .ConfigureAwait(false);

            if (existing != null)
            {
                if (!string.Equals(existing.Name, categoryOption.Name, StringComparison.Ordinal))
                {
                    existing.Name = categoryOption.Name;
                    affected++;
                }
                continue;
            }

            var category = new AmazonCategory
            {
                Name = categoryOption.Name,
                AmazonCategoryId = categoryOption.AmazonCategoryId,
                ParentCategoryId = categoryOption.ParentCategoryId
            };

            _dbContext.AmazonCategories.Add(category);
            affected++;
        }

        if (affected > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return affected;
    }

    /// <summary>
    /// 抓取指定类目与榜单类型，生成一条快照记录并落库所有数据点，返回快照主键。
    /// </summary>
    public async Task<long> CaptureSnapshotAsync(int categoryId, Amazon.AmazonBestsellerType bestsellerType, CancellationToken cancellationToken)
    {
        var category = await _dbContext.AmazonCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Amazon category {categoryId} does not exist.");

        var now = DateTime.UtcNow;
        var snapshot = new AmazonSnapshot
        {
            CapturedAt = now,
            CategoryId = category.Id,
            BestsellerType = bestsellerType.ToString(),
            Status = "InProgress"
        };

        _dbContext.AmazonSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 请求 Amazon 榜单页面并解析结果，返回当前榜单的所有条目。
            var entries = await _bestsellerSource.FetchAsync(category.AmazonCategoryId, bestsellerType, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Fetched {Count} amazon entries for category {CategoryName}", entries.Count, category.Name);

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var product = await _dbContext.AmazonProducts
                    .FirstOrDefaultAsync(p => p.Id == entry.Asin, cancellationToken)
                    .ConfigureAwait(false);

                if (product == null)
                {
                    // 首次遇到该 ASIN，创建产品主数据记录。
                    product = new AmazonProduct
                    {
                        Id = entry.Asin,
                        Title = entry.Title,
                        Brand = entry.Brand,
                        CategoryId = category.Id,
                        ListingDate = entry.ListingDate,
                        ImageUrl = entry.ImageUrl
                    };

                    _dbContext.AmazonProducts.Add(product);
                }
                else
                {
                    // 已存在则更新可能发生变化的字段，确保前端读取到最新展示信息。
                    product.Title = entry.Title;
                    product.Brand = entry.Brand;
                    product.ImageUrl = entry.ImageUrl;
                    if (entry.ListingDate.HasValue && product.ListingDate == null)
                    {
                        product.ListingDate = entry.ListingDate;
                    }

                    if (product.CategoryId != category.Id)
                    {
                        product.CategoryId = category.Id;
                    }
                }

                // 关于同一商品的最新榜单数据点，以便历史趋势分析与前端展示。
                var dataPoint = new AmazonProductDataPoint
                {
                    ProductId = entry.Asin,
                    SnapshotId = snapshot.Id,
                    CapturedAt = now,
                    Rank = entry.Rank,
                    Price = entry.Price,
                    Rating = entry.Rating,
                    ReviewsCount = entry.ReviewsCount
                };

                _dbContext.AmazonProductDataPoints.Add(dataPoint);
            }

            snapshot.Status = "Completed";
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return snapshot.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture Amazon snapshot for category {CategoryName}", category.Name);
            snapshot.Status = "Failed";
            snapshot.ErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
