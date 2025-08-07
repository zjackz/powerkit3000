using System.ComponentModel.DataAnnotations;

namespace AmazonTrends.Data.Models;

/// <summary>
/// 代表一个商品分类。
/// 支持无限层级的父子结构。
/// </summary>
public class Category
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 分类名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 亚马逊分类ID或路径，用于构建抓取URL
    /// </summary>
    [MaxLength(200)]
    public string? AmazonCategoryId { get; set; }

    /// <summary>
    /// 外键：自引用，关联到父分类
    /// </summary>
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    /// <summary>
    /// 导航属性：该分类下的所有子分类
    /// </summary>
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    /// <summary>
    /// 导航属性：该分类下的所有商品
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
