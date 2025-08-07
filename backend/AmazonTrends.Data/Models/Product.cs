using System.ComponentModel.DataAnnotations;

namespace AmazonTrends.Data.Models;

/// <summary>
/// 代表系统中追踪的一个商品。
/// 使用亚马逊的 ASIN 作为主键。
/// </summary>
public class Product
{
    [Key]
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 商品标题
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 品牌名称
    /// </summary>
    [MaxLength(100)]
    public string? Brand { get; set; }

    /// <summary>
    /// 商品主图链接
    /// </summary>
    [Url]
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 商品首次上架日期
    /// </summary>
    public DateTime? ListingDate { get; set; }

    /// <summary>
    /// 外键：关联到 Category 表
    /// </summary>
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    /// <summary>
    /// 导航属性：该商品所有的数据快照点
    /// </summary>
    public ICollection<ProductDataPoint> DataPoints { get; set; } = new List<ProductDataPoint>();
}
