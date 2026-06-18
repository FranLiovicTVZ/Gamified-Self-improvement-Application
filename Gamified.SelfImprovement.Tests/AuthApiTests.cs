using System.Net;
using System.Net.Http.Json;
using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gamified.SelfImprovement.Tests;

public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("environment", "Testing"));
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    // ─── Register ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_ReturnsOkAndSetsSession()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", ValidUser());
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.NotNull(body.User);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var client = CreateClient();
        var dto = ValidUser();
        await client.PostAsJsonAsync("/api/auth/register", dto);
        // same email again
        dto.UserName = "different_name";
        var response = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingFields_ReturnsBadRequest()
    {
        var client = CreateClient();
        // empty DTO — model validation should reject it
        var response = await client.PostAsJsonAsync("/api/auth/register", new CreateUserDTO
        {
            UserName = "",
            Email = "",
            Password = "",
            OIB = "",
            JMBG = ""
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── Login ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var client = CreateClient();
        var dto = ValidUser();
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Email = dto.Email,
            Password = dto.Password
        });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
        Assert.True(body!.Success);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        var client = CreateClient();
        var dto = ValidUser();
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Email = dto.Email,
            Password = "WrongPassword!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsBadRequest()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Email = "nobody@nowhere.com",
            Password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── Me ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_WhenAuthenticated_ReturnsCurrentUser()
    {
        var client = CreateClient();
        var dto = ValidUser();
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserDTO>();
        Assert.Equal(dto.Email, user!.Email);
    }

    [Fact]
    public async Task GetMe_WhenUnauthenticated_ReturnsUnauthorizedOrRedirect()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/auth/me");
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected 401 or 302 but got {response.StatusCode}");
    }

    // ─── Get user by ID ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_WithValidId_ReturnsUser()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", ValidUser());

        var me = await (await client.GetAsync("/api/auth/me")).Content.ReadFromJsonAsync<UserDTO>();
        Assert.NotNull(me);

        var response = await client.GetAsync($"/api/auth/users/{me!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserDTO>();
        Assert.Equal(me.Id, user!.Id);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ReturnsNotFound()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", ValidUser());

        var response = await client.GetAsync("/api/auth/users/nonexistent-id-00000000");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Update profile ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_WhenAuthenticated_UpdatesAndReturnsUser()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", ValidUser());

        var me = await (await client.GetAsync("/api/auth/me")).Content.ReadFromJsonAsync<UserDTO>();

        me!.Bio = "Ažurirana biografija";
        var response = await client.PutAsJsonAsync("/api/auth/profile", me);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<UserDTO>();
        Assert.Equal("Ažurirana biografija", updated!.Bio);
    }

    // ─── Change password ──────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_WithCorrectCurrentPassword_ReturnsOk()
    {
        var client = CreateClient();
        var dto = ValidUser();
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var response = await client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = dto.Password,
            newPassword = "NewPassword456!",
            confirmPassword = "NewPassword456!"
        });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", ValidUser());

        var response = await client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "WrongCurrent!",
            newPassword = "NewPassword456!",
            confirmPassword = "NewPassword456!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── Logout ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WhenAuthenticated_ReturnsOkAndInvalidatesSession()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", ValidUser());

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.True(logout.IsSuccessStatusCode, await logout.Content.ReadAsStringAsync());

        // after logout, /api/auth/me should no longer be accessible
        var me = await client.GetAsync("/api/auth/me");
        Assert.True(
            me.StatusCode == HttpStatusCode.Unauthorized ||
            me.StatusCode == HttpStatusCode.Redirect,
            $"Expected 401 or 302 after logout but got {me.StatusCode}");
    }

    [Fact]
    public async Task Logout_WhenUnauthenticated_ReturnsUnauthorizedOrRedirect()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/auth/logout", null);
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected 401 or 302 but got {response.StatusCode}");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static CreateUserDTO ValidUser() => new()
    {
        UserName = $"usr_{Guid.NewGuid():N}"[..16],
        Email = $"auth-{Guid.NewGuid():N}@test.com",
        Password = "Password123!",
        OIB = "12345678901",
        JMBG = "1234567890123"
    };
}
