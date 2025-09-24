using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using pk.api.Contracts;

namespace pk.api.tests;

public class FavoritesEndpointsTests
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
    public async Task FavoritesLifecycle_Should_PersistData()
    {
        var clientId = Guid.NewGuid().ToString("N");
        var projectId = 1_000_001L;

        var initialResponse = await _client.GetAsync($"/favorites?clientId={clientId}");
        initialResponse.EnsureSuccessStatusCode();
        var initialFavorites = await initialResponse.Content.ReadFromJsonAsync<List<ProjectFavoriteDto>>();
        Assert.That(initialFavorites, Is.Not.Null.And.Empty);

        var createRequest = new UpsertFavoriteRequest
        {
            ClientId = clientId,
            ProjectId = projectId,
            Note = "高达成率项目",
        };

        var createResponse = await _client.PostAsJsonAsync("/favorites", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectFavoriteDto>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Project.Id, Is.EqualTo(projectId));
        Assert.That(created.Note, Is.EqualTo("高达成率项目"));

        var listResponse = await _client.GetAsync($"/favorites?clientId={clientId}");
        listResponse.EnsureSuccessStatusCode();
        var listFavorites = await listResponse.Content.ReadFromJsonAsync<List<ProjectFavoriteDto>>();
        Assert.That(listFavorites, Is.Not.Null.And.Count.EqualTo(1));
        Assert.That(listFavorites![0].Note, Is.EqualTo("高达成率项目"));

        var updateRequest = new UpsertFavoriteRequest
        {
            ClientId = clientId,
            ProjectId = projectId,
            Note = "更新后的收藏理由",
        };

        var updateResponse = await _client.PostAsJsonAsync("/favorites", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectFavoriteDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Note, Is.EqualTo("更新后的收藏理由"));

        var deleteResponse = await _client.DeleteAsync($"/favorites/{projectId}?clientId={clientId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

        var clearedResponse = await _client.GetAsync($"/favorites?clientId={clientId}");
        clearedResponse.EnsureSuccessStatusCode();
        var clearedFavorites = await clearedResponse.Content.ReadFromJsonAsync<List<ProjectFavoriteDto>>();
        Assert.That(clearedFavorites, Is.Not.Null.And.Empty);
    }
}
