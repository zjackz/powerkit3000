using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pk.data.Models;

public class ProjectFavorite
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public long ProjectId { get; set; }

    public string? Note { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public KickstarterProject Project { get; set; } = null!;
}
