using System;
using System.Collections.Generic;
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

    /// <summary>
    /// 初始化 <see cref="AmazonIngestionService"/>。
    /// </summary>
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
    /// <summary>
    /// 在线抓取指定类目的榜单并生成快照记录。
    /// </summary>
    /// <param name="categoryId">内部 Amazon 类目主键。</param>
    /// <param name="bestsellerType">榜单类型。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>新生成的快照主键。</returns>
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
            var entries = await _bestsellerSource.FetchAsync(category.AmazonCategoryId, bestsellerType, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Fetched {Count} amazon entries for category {CategoryName}", entries.Count, category.Name);

            await IngestEntriesAsync(category, snapshot, entries, now, cancellationToken).ConfigureAwait(false);

            snapshot.Status = "Completed";
            snapshot.ErrorMessage = null;
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

    /// <summary>
    /// 将外部抓取工具提供的榜单数据导入为快照。
    /// </summary>
    /// <param name="importModel">封装导入数据的模型。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>新生成的快照主键。</returns>
    public async Task<long> ImportSnapshotAsync(AmazonSnapshotImportModel importModel, CancellationToken cancellationToken)
    {
        var category = await _dbContext.AmazonCategories
            .FirstOrDefaultAsync(c => c.Id == importModel.CategoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Amazon category {importModel.CategoryId} does not exist.");

        var snapshot = new AmazonSnapshot
        {
            CapturedAt = importModel.CapturedAt,
            CategoryId = category.Id,
            BestsellerType = importModel.BestsellerType.ToString(),
            Status = "InProgress"
        };

        _dbContext.AmazonSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await IngestEntriesAsync(category, snapshot, importModel.Entries, importModel.CapturedAt, cancellationToken).ConfigureAwait(false);

            snapshot.Status = "Completed";
            snapshot.ErrorMessage = null;
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return snapshot.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import Amazon snapshot for category {CategoryName}", category.Name);
            snapshot.Status = "Failed";
            snapshot.ErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 将榜单条目落库为商品主数据和数据点，复用在抓取与导入场景。
    /// </summary>
    /// <param name="category">目标类目实体。</param>
    /// <param name="snapshot">当前快照。</param>
    /// <param name="entries">榜单条目集合。</param>
    /// <param name="capturedAt">采集时间。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    private async Task IngestEntriesAsync(
        AmazonCategory category,
        AmazonSnapshot snapshot,
        IReadOnlyCollection<AmazonBestsellerEntry> entries,
        DateTime capturedAt,
        CancellationToken cancellationToken)
    {
        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var product = await _dbContext.AmazonProducts
                .FirstOrDefaultAsync(p => p.Id == entry.Asin, cancellationToken)
                .ConfigureAwait(false);

            if (product == null)
            {
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

            var dataPoint = new AmazonProductDataPoint
            {
                ProductId = entry.Asin,
                SnapshotId = snapshot.Id,
                CapturedAt = capturedAt,
                Rank = entry.Rank,
                Price = entry.Price,
                Rating = entry.Rating,
                ReviewsCount = entry.ReviewsCount
            };

            _dbContext.AmazonProductDataPoints.Add(dataPoint);
        }
    }
}
