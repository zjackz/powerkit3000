using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using pk.core.Amazon.Operations;
using pk.core.Amazon.Options;
using pk.data;
using pk.data.Models;

namespace pk.consoleapp.Tests;

public class AmazonOperationalInsightServiceTests
{
    private AppDbContext _dbContext = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetSummaryAsync_Should_ReturnCountsForInventoryAndReviews()
    {
        // Arrange
        await SeedOperationalDataAsync();
        var service = CreateService();

        // Act
        var summary = await service.GetSummaryAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(summary.LowStock.Total, Is.EqualTo(1));
            Assert.That(summary.LowStock.High, Is.EqualTo(1));
            Assert.That(summary.NegativeReview.Total, Is.EqualTo(1));
            Assert.That(summary.NegativeReview.High, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetIssuesAsync_Should_ProjectIssueDetails()
    {
        // Arrange
        await SeedOperationalDataAsync();
        var service = CreateService();

        // Act
        var result = await service.GetIssuesAsync(new AmazonOperationalIssueQuery(), CancellationToken.None);

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));

        var lowStock = result.Items.FirstOrDefault(i => i.IssueType == AmazonOperationalIssueType.LowStock);
        Assert.That(lowStock, Is.Not.Null);
        Assert.That(lowStock!.Severity, Is.EqualTo(AmazonOperationalSeverity.High));
        Assert.That(lowStock.Kpi.InventoryDays, Is.EqualTo(2));

        var negativeReview = result.Items.FirstOrDefault(i => i.IssueType == AmazonOperationalIssueType.NegativeReview);
        Assert.That(negativeReview, Is.Not.Null);
        Assert.That(negativeReview!.Severity, Is.EqualTo(AmazonOperationalSeverity.High));
        Assert.That(negativeReview.Kpi.NegativeReviewCount, Is.EqualTo(3));
        Assert.That(negativeReview.Kpi.LatestNegativeReviewUrl, Is.EqualTo("https://example.com/review"));
    }

    private AmazonOperationalInsightService CreateService()
    {
        var options = Options.Create(new AmazonOperationalDashboardOptions
        {
            InventoryThresholdDays = 10,
            NegativeReviewMediumCount = 1,
            NegativeReviewHighCount = 3,
            DataStaleAfter = TimeSpan.FromHours(48),
        });

        return new AmazonOperationalInsightService(
            _dbContext,
            options,
            NullLogger<AmazonOperationalInsightService>.Instance);
    }

    private async Task SeedOperationalDataAsync()
    {
        var category = new AmazonCategory
        {
            Name = "测试类目",
            AmazonCategoryId = "TEST-CAT"
        };

        var product = new AmazonProduct
        {
            Id = "B0000001",
            Title = "测试商品",
            Category = category,
        };

        var snapshot = new AmazonOperationalSnapshot
        {
            CapturedAt = DateTime.UtcNow,
            Status = "Completed",
        };

        var lowStockMetric = new AmazonProductOperationalMetric
        {
            ProductId = product.Id,
            CapturedAt = snapshot.CapturedAt,
            InventoryDays = 2,
            InventoryQuantity = 5,
            UnitsSold7d = 30,
            NegativeReviewCount = 0,
            OperationalSnapshot = snapshot,
            Product = product,
        };

        var negativeReviewMetric = new AmazonProductOperationalMetric
        {
            ProductId = product.Id,
            CapturedAt = snapshot.CapturedAt,
            NegativeReviewCount = 3,
            LatestNegativeReviewAt = DateTime.UtcNow,
            LatestNegativeReviewExcerpt = "包装受损，希望尽快补发。",
            LatestNegativeReviewUrl = "https://example.com/review",
            OperationalSnapshot = snapshot,
            Product = product,
        };

        snapshot.ProductMetrics.Add(lowStockMetric);
        snapshot.ProductMetrics.Add(negativeReviewMetric);
        product.OperationalMetrics.Add(lowStockMetric);
        product.OperationalMetrics.Add(negativeReviewMetric);

        await _dbContext.AmazonCategories.AddAsync(category);
        await _dbContext.AmazonProducts.AddAsync(product);
        await _dbContext.AmazonOperationalSnapshots.AddAsync(snapshot);
        await _dbContext.SaveChangesAsync();
    }
}
