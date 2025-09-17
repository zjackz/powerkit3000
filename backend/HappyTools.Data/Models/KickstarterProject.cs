using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTools.Data.Models
{
    public class KickstarterProject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Blurb { get; set; }
        public decimal Goal { get; set; }
        public decimal Pledged { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LaunchedAt { get; set; }
        public int BackersCount { get; set; }
        public decimal UsdPledged { get; set; }
        public DateTime StateChangedAt { get; set; }
        public string Slug { get; set; }
        public string CountryDisplayableName { get; set; }
        public string CurrencySymbol { get; set; }
                public bool? CurrencyTrailingCode { get; set; }
        public bool? IsInPostCampaignPledgingPhase { get; set; }
        public bool? StaffPick { get; set; }
        public bool? IsStarrable { get; set; }
        public bool? DisableCommunication { get; set; }
        public decimal StaticUsdRate { get; set; }
        public decimal ConvertedPledgedAmount { get; set; }
        public decimal FxRate { get; set; }
        public decimal UsdExchangeRate { get; set; }
        public string CurrentCurrency { get; set; }
        public string UsdType { get; set; }
        public bool? Spotlight { get; set; }
        public decimal PercentFunded { get; set; }
        public bool? IsLiked { get; set; }
        public bool? IsDisliked { get; set; }
        public bool? IsLaunched { get; set; }
        public bool? PrelaunchActivated { get; set; }
        public string SourceUrl { get; set; }

        public long CreatorId { get; set; }
        public virtual Creator Creator { get; set; }

        public long CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public long LocationId { get; set; }
        public virtual Location Location { get; set; }

        // To simplify, we can store Photo, Urls as JSON strings
        [Column(TypeName = "jsonb")]
        public string Photo { get; set; }

        [Column(TypeName = "jsonb")]
        public string Urls { get; set; }
    }

    public class Creator
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public long? ParentId { get; set; }
        public string ParentName { get; set; }
    }

    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string DisplayableName { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
    }
}
