using System.Net;
using System.Net.Http.Json;
using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gamified.SelfImprovement.Tests;

public class StreaksApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StreaksApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("environment", "Testing"));
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    // ─── Leaderboard (public) ─────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaderboard_ReturnsOkWithList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/streaks/top/leaderboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ─── Get user streak ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetStreakByUserId_WhenOwnStreak_CreatesOrReturnsIt()
    {
        var client = CreateClient();
        var userId = await RegisterAndGetIdAsync(client);

        var response = await client.GetAsync($"/api/streaks/user/{userId}");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 200 or 201 but got {response.StatusCode}");

        var streak = await response.Content.ReadFromJsonAsync<StreakDTO>();
        Assert.NotNull(streak);
        Assert.Equal(userId, streak!.UserId);
    }

    [Fact]
    public async Task GetStreakByUserId_WhenOtherUser_ReturnsForbid()
    {
        // Register user A
        var clientA = CreateClient();
        var userAId = await RegisterAndGetIdAsync(clientA);

        // Register user B
        var clientB = CreateClient();
        await RegisterAndGetIdAsync(clientB);

        // B tries to read A's streak
        var response = await clientB.GetAsync($"/api/streaks/user/{userAId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetStreakById_WhenUnauthenticated_ReturnsUnauthorizedOrRedirect()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/streaks/1");
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected 401 or 302 but got {response.StatusCode}");
    }

    // ─── Record activity ──────────────────────────────────────────────────────

    [Fact]
    public async Task RecordActivity_ForOwnUser_UpdatesStreak()
    {
        var client = CreateClient();
        var userId = await RegisterAndGetIdAsync(client);

        var response = await client.PostAsJsonAsync("/api/streaks/record-activity", new UpdateStreakDTO
        {
            UserId = userId,
        });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        var streak = await response.Content.ReadFromJsonAsync<StreakDTO>();
        Assert.NotNull(streak);
        Assert.True(streak!.TotalActivitiesCompleted >= 1);
    }

    [Fact]
    public async Task RecordActivity_ForOtherUser_ReturnsForbid()
    {
        var clientA = CreateClient();
        var userAId = await RegisterAndGetIdAsync(clientA);

        var clientB = CreateClient();
        await RegisterAndGetIdAsync(clientB);

        var response = await clientB.PostAsJsonAsync("/api/streaks/record-activity", new UpdateStreakDTO
        {
            UserId = userAId,
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RecordActivity_TwiceOnSameDay_IncrementsTotal()
    {
        var client = CreateClient();
        var userId = await RegisterAndGetIdAsync(client);

        await client.PostAsJsonAsync("/api/streaks/record-activity", new UpdateStreakDTO
        {
            UserId = userId,
        });
        var second = await client.PostAsJsonAsync("/api/streaks/record-activity", new UpdateStreakDTO
        {
            UserId = userId,
        });
        Assert.True(second.IsSuccessStatusCode, await second.Content.ReadAsStringAsync());
        var streak = await second.Content.ReadFromJsonAsync<StreakDTO>();
        Assert.True(streak!.TotalActivitiesCompleted >= 2);
    }

    // ─── Admin-only endpoints ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAllStreaks_AsAdmin_ReturnsOk()
    {
        var client = CreateClient();
        await LoginAdminAsync(client);

        var response = await client.GetAsync("/api/streaks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllStreaks_AsRegularUser_ReturnsForbid()
    {
        var client = CreateClient();
        await RegisterAndGetIdAsync(client);

        var response = await client.GetAsync("/api/streaks");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStreak_AsAdmin_ReturnsOk()
    {
        var client = CreateClient();
        await LoginAdminAsync(client);

        // Dohvati sve streake i uzmi prvi dostupan
        var all = await (await client.GetAsync("/api/streaks")).Content.ReadFromJsonAsync<List<StreakDTO>>();
        if (all == null || all.Count == 0)
        {
            // stvori streak registracijom korisnika
            var userClient = CreateClient();
            var uid = await RegisterAndGetIdAsync(userClient);
            await userClient.PostAsJsonAsync("/api/streaks/record-activity", new UpdateStreakDTO
            {
                UserId = uid
            });
            all = await (await client.GetAsync("/api/streaks")).Content.ReadFromJsonAsync<List<StreakDTO>>();
        }

        Assert.NotNull(all);
        Assert.True(all!.Count > 0, "No streaks to update");

        var target = all[0];
        var response = await client.PutAsJsonAsync($"/api/streaks/{target.Id}", new StreakDTO
        {
            Id = target.Id,
            UserId = target.UserId,
            CurrentStreak = target.CurrentStreak + 1,
            LongestStreak = target.LongestStreak + 1,
            LastActivityDate = DateTime.UtcNow
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteStreak_WithNonExistentId_ReturnsNotFound()
    {
        var client = CreateClient();
        await LoginAdminAsync(client);

        var response = await client.DeleteAsync("/api/streaks/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<string> RegisterAndGetIdAsync(HttpClient client)
    {
        var dto = new CreateUserDTO
        {
            UserName = $"str_{Guid.NewGuid():N}"[..16],
            Email = $"str-{Guid.NewGuid():N}@test.com",
            Password = "Password123!",
            OIB = "12345678901",
            JMBG = "1234567890123"
        };
        var reg = await client.PostAsJsonAsync("/api/auth/register", dto);
        var body = await reg.Content.ReadFromJsonAsync<LoginResponseDTO>();
        return body?.User?.Id ?? throw new InvalidOperationException("Register did not return a user ID");
    }

    private static async Task LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Email = "admin@gamified.hr",
            Password = "Admin123"
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
    }
}
