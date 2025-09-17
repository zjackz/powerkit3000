namespace HappyTools.WebApp.DTOs
{
    public class KickstarterQueryDto
    {
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? CategoryName { get; set; }
        public string? ProjectName { get; set; }
        public decimal? MinGoal { get; set; }
        public decimal? MaxGoal { get; set; }
        public decimal? MinPledged { get; set; }
        public decimal? MaxPledged { get; set; }
        public int? MinBackersCount { get; set; }
        public int? MaxBackersCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
