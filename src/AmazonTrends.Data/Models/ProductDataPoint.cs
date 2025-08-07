using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonTrends.Data.Models;

/// <summary>
/// 商品在特定时间点的数据快照。
/// </summary>
public class ProductDataPoint
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// 外键：关联到 Product 表
    /// </summary>
    [Required]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    /// <summary>
    /// 数据采集的时间戳
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 当时的售卖价格
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// BSR (Best Sellers Rank) 最佳销售排名
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// 评论总数
    /// </summary>
    public int ReviewsCount { get; set; }

    /// <summary>
    /// 平均评分
    /// </summary>
    public float Rating { get; set; }

    /// <summary>
    /// 外键：关联到 DataCollectionRun 表，标识该数据由哪次采集任务生成
    /// </summary>
    public long DataCollectionRunId { get; set; }
    public DataCollectionRun DataCollectionRun { get; set; }
}
