using System;

namespace powerkit3000.api.Contracts;

public class ProjectSummaryDto
{
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
    public decimal TotalPledged { get; init; }
    public int DistinctCountries { get; init; }
    public decimal SuccessRate => TotalProjects == 0 ? 0 : Math.Round((decimal)SuccessfulProjects / TotalProjects * 100, 1);
}
