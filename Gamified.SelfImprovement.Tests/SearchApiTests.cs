using System.Net;
using System.Net.Http.Json;
using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gamified.SelfImprovement.Tests;

public class SearchApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SearchApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("environment", "Testing"));
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    // ─── /api/search ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_WithEmptyQuery_ReturnsOkWithEmptyOrDefaultResults()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/search?q=");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithShortQuery_ReturnsBadRequestOrEmptyResults()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/search?q=a");
        // Controller may return 400 for single-char queries or OK with empty list — both acceptable
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public async Task Search_WithValidQuery_ReturnsGlobalSearchResult()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/search?q=admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GlobalSearchResultDTO>();
        Assert.NotNull(result);
        Assert.NotNull(result!.Results);
    }

    [Fact]
    public async Task Search_LimitParameter_IsRespected()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/search?q=a&limit=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GlobalSearchResultDTO>();
        Assert.NotNull(result);
        Assert.True(result!.Results.Count <= 2, $"Expected at most 2 results, got {result.Results.Count}");
    }

    [Fact]
    public async Task Search_WithSpecialCharacters_DoesNotCrash()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/search?q=%27%3B+DROP+TABLE+users--");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Search_AfterCreatingExercise_FindsItInResults()
    {
        var client = CreateClient();

        // Register and create an exercise with a unique title
        await RegisterAsync(client);
        var uniqueTitle = $"PlaywrightSearch_{Guid.NewGuid():N}"[..30];
        await client.PostAsJsonAsync("/api/exercises", new CreateExerciseDTO
        {
            Title = uniqueTitle,
            Description = "Search integration test",
            ExerciseType = ExerciseType.Cardio,
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Easy
        });

        var response = await client.GetAsync($"/api/search?q={uniqueTitle[..10]}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GlobalSearchResultDTO>();
        Assert.NotNull(result);
        // Activity should appear in results
        Assert.Contains(result!.Results, r => r.Title.Contains(uniqueTitle[..10], StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Search_QueryIsReflectedInResponse()
    {
        var client = CreateClient();
        const string query = "meditacija";
        var response = await client.GetAsync($"/api/search?q={query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GlobalSearchResultDTO>();
        Assert.NotNull(result);
        Assert.Equal(query, result!.Query);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static async Task RegisterAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new CreateUserDTO
        {
            UserName = $"srch_{Guid.NewGuid():N}"[..16],
            Email = $"srch-{Guid.NewGuid():N}@test.com",
            Password = "Password123!",
            OIB = "12345678901",
            JMBG = "1234567890123"
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
    }
}
