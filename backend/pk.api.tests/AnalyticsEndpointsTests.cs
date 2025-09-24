using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using pk.api.Contracts;

namespace pk.api.tests;

public class AnalyticsEndpointsTests
{
    private ApiApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new ApiApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task HealthEndpoint_Should_ReturnOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.That(response.IsSuccessStatusCode, Is.True, "Health check should return success status");
    }

    [Test]
    public async Task CategoriesEndpoint_Should_ReturnAggregatedInsights()
    {
        var response = await _client.GetAsync("/analytics/categories?minProjects=1");
        var insights = await response.Content.ReadFromJsonAsync<List<CategoryInsightDto>>();

        Assert.That(response.IsSuccessStatusCode, Is.True, "Request should succeed");
        if (insights is null)
        {
            Assert.Fail("Analytics endpoint returned no category insights");
            return;
        }

        Assert.That(insights.Count, Is.EqualTo(2));

        var design = insights.Single(i => i.CategoryName == "Design");
        var games = insights.Single(i => i.CategoryName == "Games");

        Assert.That(design.TotalProjects, Is.EqualTo(3));
        Assert.That(design.SuccessfulProjects, Is.EqualTo(2));
        Assert.That(Math.Round(design.SuccessRate, 1), Is.EqualTo(66.7m));

        Assert.That(games.TotalProjects, Is.EqualTo(2));
        Assert.That(games.SuccessRate, Is.EqualTo(100m));
    }

    [Test]
    public async Task ProjectSummaryEndpoint_Should_ReturnTotals()
    {
        var response = await _client.GetAsync("/projects/summary");
        var summary = await response.Content.ReadFromJsonAsync<ProjectSummaryDto>();

        Assert.That(response.IsSuccessStatusCode, Is.True);
        if (summary is null)
        {
            Assert.Fail("Summary endpoint returned null payload");
            return;
        }

        Assert.That(summary.TotalProjects, Is.EqualTo(5));
        Assert.That(summary.SuccessfulProjects, Is.EqualTo(4));
        Assert.That(summary.DistinctCountries, Is.EqualTo(3));
        Assert.That(summary.TotalPledged, Is.EqualTo(12400m));
    }

    [Test]
    public async Task HypeEndpoint_Should_ReturnHighVelocityProjects()
    {
        var response = await _client.GetAsync("/analytics/hype?minPercentFunded=150&limit=3");
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectHighlightDto>>();

        Assert.That(response.IsSuccessStatusCode, Is.True);
        if (projects is null)
        {
            Assert.Fail("Hype endpoint returned null payload");
            return;
        }

        Assert.That(projects, Is.Not.Empty);
        Assert.That(projects.All(p => p.PercentFunded >= 150m), Is.True);

        var ordered = true;
        if (projects.Count > 1)
        {
            ordered = projects
                .Zip(projects.Skip(1), (current, next) => current.FundingVelocity >= next.FundingVelocity)
                .All(result => result);
        }

        Assert.That(ordered, Is.True, "Projects should be ordered by funding velocity desc");
        Assert.That(projects.First().FundingVelocity, Is.GreaterThan(0m));
    }

    [Test]
    public async Task CategoryKeywordsEndpoint_Should_ReturnKeywordCloud()
    {
        var response = await _client.GetAsync("/analytics/category-keywords?category=Design&top=5");
        var keywords = await response.Content.ReadFromJsonAsync<List<CategoryKeywordDto>>();

        Assert.That(response.IsSuccessStatusCode, Is.True);
        if (keywords is null)
        {
            Assert.Fail("Category keywords endpoint returned null payload");
            return;
        }

        Assert.That(keywords, Is.Not.Empty);

        var designKeyword = keywords.SingleOrDefault(k => k.Keyword == "design");
        Assert.That(designKeyword, Is.Not.Null, "Design keyword should surface");
        Assert.That(designKeyword!.ProjectCount, Is.GreaterThanOrEqualTo(2));
    }
}
