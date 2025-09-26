using System;

namespace pk.data.Models;

/// <summary>
/// Amazon 商品的运营指标，用于评估库存和差评风险。
/// </summary>
public class AmazonProductOperationalMetric
{
    public long Id { get; set; }
    public long OperationalSnapshotId { get; set; }
    public string ProductId { get; set; } = null!;
    public DateTime CapturedAt { get; set; }

    public int? InventoryQuantity { get; set; }
    public decimal? InventoryDays { get; set; }
    public int? UnitsSold7d { get; set; }
    public bool? IsStockout { get; set; }

    public int NegativeReviewCount { get; set; }
    public DateTime? LatestNegativeReviewAt { get; set; }
    public string? LatestNegativeReviewExcerpt { get; set; }
    public string? LatestNegativeReviewUrl { get; set; }

    public DateTime? LatestPriceUpdatedAt { get; set; }
    public decimal? BuyBoxPrice { get; set; }

    public AmazonOperationalSnapshot OperationalSnapshot { get; set; } = null!;
    public AmazonProduct Product { get; set; } = null!;
}
