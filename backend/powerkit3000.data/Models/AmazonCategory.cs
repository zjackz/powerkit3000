using System.Collections.Generic;

namespace powerkit3000.data.Models;

/// <summary>
/// Amazon 类目实体，支持父子结构以映射站内层级。
/// </summary>
public class AmazonCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string AmazonCategoryId { get; set; } = null!;
    public int? ParentCategoryId { get; set; }

    public AmazonCategory? ParentCategory { get; set; }

    public ICollection<AmazonCategory> SubCategories { get; set; } = new List<AmazonCategory>();
    public ICollection<AmazonProduct> Products { get; set; } = new List<AmazonProduct>();
}
