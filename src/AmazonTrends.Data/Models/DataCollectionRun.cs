using System.ComponentModel.DataAnnotations;

namespace AmazonTrends.Data.Models;

/// <summary>
/// 记录一次数据采集任务的执行日志。
/// </summary>
public class DataCollectionRun
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// 任务执行时间戳
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 任务结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 任务执行状态 (例如: "Running", "Completed", "Failed")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 外键：关联到 Category 表，表示本次采集任务针对哪个分类
    /// </summary>
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    /// <summary>
    /// 本次任务成功抓取的商品数量
    /// </summary>
    public int ProductsScraped { get; set; }

    /// <summary>
    /// 导航属性：本次任务生成的所有数据点
    /// </summary>
    public ICollection<ProductDataPoint> DataPoints { get; set; } = new List<ProductDataPoint>();
}
