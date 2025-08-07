using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonTrends.Data.Models;

public class ProductTrend
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string ProductId { get; set; } = null!;
    public Product Product { get; set; } = null!;

    public long AnalysisResultId { get; set; }
    public AnalysisResult AnalysisResult { get; set; } = null!;

    public string TrendType { get; set; } = null!; // 例如: RankSurge, NewEntry, ConsistentPerformer
    public string Description { get; set; } = null!;
    public DateTime AnalysisTime { get; set; }
}
