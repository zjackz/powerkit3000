using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonTrends.Data.Models;

public class AnalysisResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long DataCollectionRunId { get; set; }
    public DataCollectionRun DataCollectionRun { get; set; } = null!;

    public DateTime AnalysisTime { get; set; }

    public ICollection<ProductTrend> Trends { get; set; } = new List<ProductTrend>();
}
