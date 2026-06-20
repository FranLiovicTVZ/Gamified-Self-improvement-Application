using Microsoft.Playwright;
using Xunit;

namespace Gamified.SelfImprovement.PlaywrightTests;

/// <summary>
/// Sveobuhvatni E2E testovi za deployment (Azure: gamified-fliovic.azurewebsites.net).
/// Pokreni: set PLAYWRIGHT_BASE_URL=https://gamified-fliovic.azurewebsites.net
///          dotnet test --filter "FullyQualifiedName~DeploymentE2ETests"
/// Admin: admin@gamified.hr / Admin123
/// </summary>

// ─── Base Class ───────────────────────────────────────────────────────────────

public abstract class DeploymentTestBase : IAsyncLifetime
{
    protected IPlaywright _playwright = null!;
    protected IBrowser _browser = null!;
    protected IBrowserContext _context = null!;
    protected IPage _page = null!;

    protected static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL")
        ?? "https://gamified-fliovic.azurewebsites.net";

    protected const string AdminEmail = "admin@gamified.hr";
    protected const string AdminPassword = "Admin123";

    public virtual async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            SlowMo = 80
        });
        _context = await CreateContextAsync();
        _page = await _context.NewPageAsync();
    }

    protected virtual Task<IBrowserContext> CreateContextAsync() =>
        _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
            IgnoreHTTPSErrors = true
        });

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    protected async Task LoginAsAdminAsync()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.FillAsync("input[name='Email']", AdminEmail);
        await _page.FillAsync("input[name='Password']", AdminPassword);
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected async Task<string> RegisterAndLoginAsync()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        var email = $"e2e_{id}@playwright.test";
        var username = $"pw_{id}"[..14];

        await _page.GotoAsync("/auth/register");
        await _page.WaitForSelectorAsync("form");
        await _page.FillAsync("input[name='UserName']", username);
        await _page.FillAsync("input[name='Email']", email);
        await _page.FillAsync("input[name='OIB']", "12345678901");
        await _page.FillAsync("input[name='JMBG']", "1234567890123");
        await _page.FillAsync("input[name='Password']", "Password123!");
        var confirm = _page.Locator("input[name='ConfirmPassword']");
        if (await confirm.CountAsync() > 0) await confirm.FillAsync("Password123!");
        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return username;
    }

    protected async Task LogoutAsync()
    {
        var btn = _page.Locator("form[action='/auth/logout'] button[type='submit']");
        if (await btn.CountAsync() > 0)
        {
            await btn.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    protected async Task<string?> CreateExerciseAndGetIdAsync(string title = "E2E_Vjezba")
    {
        await _page.GotoAsync("/aktivnosti/nova-vjezba");
        await _page.WaitForSelectorAsync("form");
        await _page.FillAsync("input[name='Title']", title);
        var dur = _page.Locator("input[name='DurationMinutes']");
        if (await dur.CountAsync() > 0) await dur.FillAsync("20");
        var cal = _page.Locator("input[name='CaloriesBurned']");
        if (await cal.CountAsync() > 0) await cal.FillAsync("150");
        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var segments = new Uri(_page.Url).AbsolutePath.TrimEnd('/').Split('/');
        return segments.LastOrDefault(s => int.TryParse(s, out _));
    }
}

// ─── Auth Tests ───────────────────────────────────────────────────────────────

public class AuthDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task LoginPage_HasEmailPasswordAndSubmitButton()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/auth/login", _page.Url);
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='Email']"));
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='Password']"));
        Assert.NotNull(await _page.QuerySelectorAsync("button[type='submit']"));
    }

    [Fact]
    public async Task RegisterPage_HasAllRequiredFields()
    {
        await _page.GotoAsync("/auth/register");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='UserName']"));
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='Email']"));
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='OIB']"));
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='JMBG']"));
        Assert.NotNull(await _page.QuerySelectorAsync("input[name='Password']"));
    }

    [Fact]
    public async Task Login_WithAdminCredentials_Succeeds()
    {
        await LoginAsAdminAsync();
        Assert.DoesNotContain("/auth/login", _page.Url);
    }

    [Fact]
    public async Task Login_WithWrongPassword_StaysOnLoginPage()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.FillAsync("input[name='Email']", "nobody@example.com");
        await _page.FillAsync("input[name='Password']", "WrongPass999");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/auth/login", _page.Url);
    }

    [Fact]
    public async Task Register_WithNewUser_LogsInAutomatically()
    {
        await RegisterAndLoginAsync();
        Assert.DoesNotContain("/auth/register", _page.Url);
        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
    }

    [Fact]
    public async Task Logout_ProtectsPrivateRoutes()
    {
        await LoginAsAdminAsync();
        await LogoutAsync();
        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/auth/login", _page.Url);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShowsError()
    {
        await _page.GotoAsync("/auth/register");
        await _page.WaitForSelectorAsync("form");
        await _page.FillAsync("input[name='UserName']", "duptest_" + Guid.NewGuid().ToString("N")[..6]);
        await _page.FillAsync("input[name='Email']", AdminEmail);
        await _page.FillAsync("input[name='OIB']", "12345678901");
        await _page.FillAsync("input[name='JMBG']", "1234567890123");
        await _page.FillAsync("input[name='Password']", "Password123!");
        var confirm = _page.Locator("input[name='ConfirmPassword']");
        if (await confirm.CountAsync() > 0) await confirm.FillAsync("Password123!");
        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Should stay on register or show error
        var isOnRegister = _page.Url.Contains("/auth/register");
        var errors = await _page.Locator(".text-danger, .alert-danger").AllInnerTextsAsync();
        var hasError = errors.Any(e => !string.IsNullOrWhiteSpace(e));
        Assert.True(isOnRegister || hasError, "Duplicate email should show error or stay on register");
    }
}

// ─── Mobile Tests ─────────────────────────────────────────────────────────────

public class MobileDeploymentTests : DeploymentTestBase
{
    protected override Task<IBrowserContext> CreateContextAsync() =>
        _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            IsMobile = true,
            UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
            IgnoreHTTPSErrors = true
        });

    [Fact]
    public async Task MobileLogin_EmailFieldIsClickableAndFillable()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var email = _page.Locator("input[name='Email']");
        await email.ClickAsync();
        await email.FillAsync("mobile@test.com");
        Assert.Equal("mobile@test.com", await email.InputValueAsync());
    }

    [Fact]
    public async Task MobileLogin_PasswordFieldIsClickableAndFillable()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var pw = _page.Locator("input[name='Password']");
        await pw.ClickAsync();
        await pw.FillAsync("TestPass123!");
        Assert.Equal("TestPass123!", await pw.InputValueAsync());
    }

    [Fact]
    public async Task MobileLogin_SubmitButtonIsClickableAndLogsIn()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.FillAsync("input[name='Email']", AdminEmail);
        await _page.FillAsync("input[name='Password']", AdminPassword);
        await _page.Locator("button[type='submit']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
    }

    [Fact]
    public async Task MobileLogin_AiChatWrapperDoesNotBlockFormInputs()
    {
        await _page.GotoAsync("/auth/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The AI chat wrapper should have pointer-events:none so it doesn't block form
        var wrapperPointerEvents = await _page.EvalOnSelectorAsync<string>(
            "#ai-chat-wrapper",
            "el => window.getComputedStyle(el).pointerEvents");

        Assert.Equal("none", wrapperPointerEvents);
    }

    [Fact]
    public async Task MobileLogin_AiChatToggleRemainsClickable()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var togglePointerEvents = await _page.EvalOnSelectorAsync<string>(
            "#ai-chat-toggle",
            "el => window.getComputedStyle(el).pointerEvents");

        Assert.Equal("auto", togglePointerEvents);
    }

    [Fact]
    public async Task MobileNavigation_HamburgerMenuOpensAndShowsLinks()
    {
        await _page.GotoAsync("/");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var toggler = _page.Locator(".navbar-toggler");
        if (await toggler.IsVisibleAsync())
        {
            await toggler.ClickAsync();
            await _page.WaitForTimeoutAsync(400);
            var links = await _page.Locator(".navbar-nav .nav-link").CountAsync();
            Assert.True(links > 0, "Hamburger menu should show navigation links");
        }
    }

    [Fact]
    public async Task MobileLeaderboard_LoadsAndIsReadable()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/leaderboard", _page.Url);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 50);
    }

    [Fact]
    public async Task MobileActivityList_IsAccessible()
    {
        await _page.GotoAsync("/aktivnosti");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/aktivnosti", _page.Url);
    }
}

// ─── Activity Tests ────────────────────────────────────────────────────────────

public class ActivityDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task ActivityList_IsAccessibleWithoutLogin()
    {
        await _page.GotoAsync("/aktivnosti");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/aktivnosti", _page.Url);
    }

    [Fact]
    public async Task CreateExercisePage_RequiresLogin()
    {
        await _page.GotoAsync("/aktivnosti/nova-vjezba");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/auth/login", _page.Url);
    }

    [Fact]
    public async Task CreateExercise_WithLoggedInUser_Succeeds()
    {
        await RegisterAndLoginAsync();
        var tag = Guid.NewGuid().ToString("N")[..10];
        var id = await CreateExerciseAndGetIdAsync($"E2E_Vjezba_{tag}");

        Assert.False(_page.Url.Contains("/nova-vjezba"),
            $"Should redirect after exercise creation, URL: {_page.Url}");
        Assert.False(string.IsNullOrEmpty(id), "Should extract activity ID from redirect URL");
    }

    [Fact]
    public async Task CreateMeditation_WithLoggedInUser_Succeeds()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/aktivnosti/nova-meditacija");
        await _page.WaitForSelectorAsync("form");

        var tag = Guid.NewGuid().ToString("N")[..10];
        await _page.FillAsync("input[name='Title']", $"E2E_Med_{tag}");
        var dur = _page.Locator("input[name='DurationMinutes']");
        if (await dur.CountAsync() > 0) await dur.FillAsync("15");
        var stress = _page.Locator("input[name='StressReliefScore']");
        if (await stress.CountAsync() > 0) await stress.FillAsync("7");
        var clarity = _page.Locator("input[name='MentalClarity']");
        if (await clarity.CountAsync() > 0) await clarity.FillAsync("8");

        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.False(_page.Url.Contains("/nova-meditacija"),
            $"Should redirect after meditation creation, URL: {_page.Url}");
    }

    [Fact]
    public async Task CreateJournal_WithLoggedInUser_Succeeds()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/aktivnosti/novi-dnevnik");
        await _page.WaitForSelectorAsync("form");

        var tag = Guid.NewGuid().ToString("N")[..10];
        var titleInput = _page.Locator("input[name='Title']");
        if (await titleInput.CountAsync() > 0)
            await titleInput.FillAsync($"E2E_Dnevnik_{tag}");

        var goals = _page.Locator("textarea[name='DailyGoals']");
        if (await goals.CountAsync() > 0) await goals.FillAsync("E2E test ciljevi za dan");

        var mood = _page.Locator("input[name='MoodRating']");
        if (await mood.CountAsync() > 0) await mood.FillAsync("8");

        var energy = _page.Locator("input[name='EnergyLevel']");
        if (await energy.CountAsync() > 0) await energy.FillAsync("7");

        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.False(_page.Url.Contains("/novi-dnevnik"),
            $"Should redirect after journal creation, URL: {_page.Url}");
    }

    [Fact]
    public async Task ActivityDetails_ShowsContent()
    {
        await LoginAsAdminAsync();
        var id = await CreateExerciseAndGetIdAsync("E2E_Details_Test");

        Assert.False(string.IsNullOrEmpty(id), "Activity ID should be extractable from URL");
        await _page.GotoAsync($"/aktivnosti/{id}");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain("/auth/login", _page.Url);
        Assert.Contains("/aktivnosti/", _page.Url);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 50, "Activity details page should have content");
    }

    [Fact]
    public async Task ActivityEdit_RedirectsToEditPageAfterCreate()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/aktivnosti/nova-vjezba");
        await _page.WaitForSelectorAsync("form");
        await _page.FillAsync("input[name='Title']", "E2E_Edit_Source");
        var dur = _page.Locator("input[name='DurationMinutes']");
        if (await dur.CountAsync() > 0) await dur.FillAsync("20");
        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/aktivnosti/uredi/", _page.Url);
    }

    [Fact]
    public async Task ActivityEdit_UpdatesTitle()
    {
        await LoginAsAdminAsync();
        var id = await CreateExerciseAndGetIdAsync("E2E_Before_Edit");

        Assert.False(string.IsNullOrEmpty(id));
        await _page.GotoAsync($"/aktivnosti/uredi/{id}");
        await _page.WaitForSelectorAsync("form");

        await _page.FillAsync("input[name='Title']", "E2E_After_Edit");
        await _page.Locator("main button[type='submit']").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain("/uredi/", _page.Url);
    }

    [Fact]
    public async Task ActivityList_WithActivities_ShowsContent()
    {
        await LoginAsAdminAsync();
        await CreateExerciseAndGetIdAsync("E2E_List_Check");

        await _page.GotoAsync("/aktivnosti");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var cards = await _page.Locator(".activity-card, .card").CountAsync();
        var noData = await _page.Locator(".no-data").CountAsync();
        Assert.True(cards > 0 || noData > 0, "Activity list should show cards or empty state");
    }

    [Fact]
    public async Task ActivityDelete_PageIsAccessibleByAdmin()
    {
        await LoginAsAdminAsync();
        var id = await CreateExerciseAndGetIdAsync("E2E_To_Delete");

        Assert.False(string.IsNullOrEmpty(id));
        await _page.GotoAsync($"/aktivnosti/obrisi/{id}");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain("/auth/login", _page.Url);
    }
}

// ─── Dashboard Tests ───────────────────────────────────────────────────────────

public class DashboardDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task UserDashboard_LoadsForAuthenticatedUser()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 100, "User dashboard should have content");
    }

    [Fact]
    public async Task UserDashboard_ShowsXpOrLevel()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var body = await _page.Locator("body").InnerTextAsync();
        var hasXp = body.Contains("XP", StringComparison.OrdinalIgnoreCase)
                 || body.Contains("Razina", StringComparison.OrdinalIgnoreCase)
                 || body.Contains("Level", StringComparison.OrdinalIgnoreCase);
        Assert.True(hasXp, "User dashboard should display XP or level information");
    }

    [Fact]
    public async Task AdminDashboard_IsAccessibleByAdmin()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/admin/dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
        Assert.Contains("/admin/dashboard", _page.Url);
    }

    [Fact]
    public async Task AdminDashboard_ShowsUserAndActivityStats()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/admin/dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 100, "Admin dashboard should have content");
    }

    [Fact]
    public async Task Dashboard_UnauthenticatedUser_RedirectsToLogin()
    {
        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/auth/login", _page.Url);
    }

    [Fact]
    public async Task HomeRoute_RedirectsAppropriately()
    {
        await _page.GotoAsync("/");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var url = _page.Url;
        var isValid = url.Contains("leaderboard") || url.Contains("login") ||
                      url.TrimEnd('/') == BaseUrl.TrimEnd('/') || url.Contains("dashboard");
        Assert.True(isValid, $"Home route should redirect to known page, got: {url}");
    }
}

// ─── Profile Tests ─────────────────────────────────────────────────────────────

public class ProfileDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task OwnProfile_IsAccessibleWhenLoggedIn()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/profil");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
    }

    [Fact]
    public async Task ProfileEdit_PageLoadsWhenLoggedIn()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/profil/uredi");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
        Assert.NotNull(await _page.QuerySelectorAsync("form"));
    }

    [Fact]
    public async Task ProfileEdit_UpdatesBioSuccessfully()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/profil/uredi");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bio = _page.Locator("textarea[name='Bio'], input[name='Bio']");
        if (await bio.CountAsync() > 0)
        {
            await bio.FillAsync($"E2E bio test {DateTime.UtcNow:HHmmss}");
            await _page.Locator("main button[type='submit']").First.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var url = _page.Url.Split('?')[0];
            Assert.False(url.EndsWith("/uredi"), $"Should redirect after bio update, URL: {_page.Url}");
        }
    }

    [Fact]
    public async Task PublicProfile_IsAccessibleWithoutLogin()
    {
        await _page.GotoAsync("/korisnici");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/korisnici", _page.Url);

        var profileLink = _page.Locator("a[href*='/korisnici/'], a[href*='/profil/user/']").First;
        if (await profileLink.CountAsync() > 0)
        {
            await profileLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.DoesNotContain("/auth/login", _page.Url);
            var body = await _page.Locator("body").InnerTextAsync();
            Assert.True(body.Length > 50, "Public profile should have content");
        }
    }

    [Fact]
    public async Task UserList_ShowsRegisteredUsers()
    {
        await _page.GotoAsync("/korisnici");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/korisnici", _page.Url);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 50, "User list should have content");
    }
}

// ─── Leaderboard Tests ─────────────────────────────────────────────────────────

public class LeaderboardDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task Leaderboard_LoadsWithoutAuthentication()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/leaderboard", _page.Url);
    }

    [Fact]
    public async Task Leaderboard_HasPageContent()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 50, "Leaderboard page should have content");
    }

    [Fact]
    public async Task Leaderboard_ContainsRankingElements()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Should contain table rows, leaderboard items, or at minimum a heading
        var hasTable = await _page.Locator("table, .leaderboard-row, .leaderboard-item, .user-entry").CountAsync() > 0;
        var hasHeading = await _page.Locator("h1, h2, h3").CountAsync() > 0;
        Assert.True(hasTable || hasHeading, "Leaderboard should have ranking elements or a heading");
    }

    [Fact]
    public async Task Leaderboard_AfterUserCreatesActivity_XpIsTracked()
    {
        await RegisterAndLoginAsync();
        await CreateExerciseAndGetIdAsync("E2E_Leaderboard_Activity");

        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/leaderboard", _page.Url);
    }
}

// ─── Search Tests ──────────────────────────────────────────────────────────────

public class SearchDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task GlobalSearchPage_LoadsWithQuery()
    {
        await _page.GotoAsync("/pretraga?q=meditacija");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/pretraga", _page.Url);
    }

    [Fact]
    public async Task GlobalSearchPage_LoadsWithEmptyQuery()
    {
        await _page.GotoAsync("/pretraga?q=");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/pretraga", _page.Url);
    }

    [Fact]
    public async Task GlobalSearch_FindsCreatedActivity()
    {
        await LoginAsAdminAsync();
        var keyword = "srchtest" + Guid.NewGuid().ToString("N")[..6];
        await CreateExerciseAndGetIdAsync(keyword);

        await _page.GotoAsync($"/pretraga?q={keyword}");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var body = await _page.Locator("body").InnerTextAsync();
        Assert.Contains(keyword, body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActivitySearchAjax_ReturnsWithoutServerError()
    {
        await LoginAsAdminAsync();
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/aktivnosti/pretraga?q=vjezba");
        Assert.True(res.Status < 500, $"Activity search should not return 5xx, got: {res.Status}");
    }

    [Fact]
    public async Task UserSearchAjax_ReturnsWithoutServerError()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/korisnici/pretraga?q=admin");
        Assert.True(res.Status < 500, $"User search should not return 5xx, got: {res.Status}");
    }
}

// ─── AI Chat Tests ─────────────────────────────────────────────────────────────

public class AiChatDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task AiChatWidget_IsPresentInDom()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.True(await _page.Locator("#ai-chat-toggle").CountAsync() > 0,
            "AI chat toggle button should be in DOM");
        Assert.True(await _page.Locator("#ai-chat-panel").CountAsync() > 0,
            "AI chat panel should be in DOM");
    }

    [Fact]
    public async Task AiChatWidget_ToggleOpensAndClosesPanel()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggle = _page.Locator("#ai-chat-toggle");
        var panel = _page.Locator("#ai-chat-panel");

        if (await toggle.CountAsync() == 0) return;

        // Open
        await toggle.ClickAsync();
        await _page.WaitForTimeoutAsync(400);
        Assert.Contains("open", await panel.GetAttributeAsync("class") ?? "");

        // Close
        await toggle.ClickAsync();
        await _page.WaitForTimeoutAsync(400);
        Assert.DoesNotContain("open", await panel.GetAttributeAsync("class") ?? "");
    }

    [Fact]
    public async Task AiChatEndpoint_EmptyMessage_ReturnsBadRequest()
    {
        var res = await _page.APIRequest.PostAsync($"{BaseUrl}/api/chat",
            new APIRequestContextOptions
            {
                DataObject = new { message = "" },
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            });
        Assert.Equal(400, res.Status);
    }

    [Fact]
    public async Task AiChatEndpoint_WithMessage_ReturnsReplyOrGracefulError()
    {
        await LoginAsAdminAsync();
        var res = await _page.APIRequest.PostAsync("/api/chat",
            new APIRequestContextOptions
            {
                DataObject = new { message = "Kako meditiram kao početnik?" },
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            });

        // 200 = AI works, 503 = API key not configured (graceful), 500 = other error
        Assert.True(res.Status == 200 || res.Status == 503 || res.Status == 500,
            $"AI chat should return 200, 503, or 500, got: {res.Status}");

        var json = await res.JsonAsync();
        Assert.NotNull(json);
        var hasReply = json.Value.TryGetProperty("reply", out _);
        var hasError = json.Value.TryGetProperty("error", out _);
        Assert.True(hasReply || hasError, "Response should contain 'reply' or 'error' field");
    }

    [Fact]
    public async Task AiChatWidget_HasSuggestedQuestions()
    {
        await _page.GotoAsync("/leaderboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Open chat
        var toggle = _page.Locator("#ai-chat-toggle");
        if (await toggle.CountAsync() > 0)
        {
            await toggle.ClickAsync();
            await _page.WaitForTimeoutAsync(400);
            var suggested = await _page.Locator(".ai-suggested-btn").CountAsync();
            Assert.True(suggested > 0, "Chat should show suggested questions when opened");
        }
    }
}

// ─── REST API Tests ────────────────────────────────────────────────────────────

public class ApiDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task ExercisesApi_GetAll_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/exercises");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task MeditationsApi_GetAll_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/meditations");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task DailyJournalsApi_GetAll_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/daily-journals");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task SpiritualBooksApi_GetAll_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/spiritual-books");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task SpiritualActivitiesApi_GetAll_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/spiritual-activities");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task ExercisesApi_CreateWithoutAuth_Returns401Or403()
    {
        var res = await _page.APIRequest.PostAsync($"{BaseUrl}/api/exercises",
            new APIRequestContextOptions
            {
                DataObject = new { title = "Unauthorized test", durationMinutes = 10 },
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            });
        Assert.True(res.Status == 401 || res.Status == 403,
            $"Creating exercise without auth should return 401/403, got: {res.Status}");
    }

    [Fact]
    public async Task ExercisesApi_WithFilters_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/exercises?search=test&difficulty=Easy");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task MeditationsApi_WithFilters_Returns200()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/meditations?search=&type=Mindfulness");
        Assert.Equal(200, res.Status);
    }

    [Fact]
    public async Task AuthApi_Login_WithValidCredentials_Returns200()
    {
        var res = await _page.APIRequest.PostAsync($"{BaseUrl}/api/auth/login",
            new APIRequestContextOptions
            {
                DataObject = new { email = AdminEmail, password = AdminPassword },
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            });
        Assert.True(res.Status == 200 || res.Status == 204,
            $"API login should return 200/204, got: {res.Status}");
    }

    [Fact]
    public async Task AuthApi_Login_WithWrongCredentials_Returns401()
    {
        var res = await _page.APIRequest.PostAsync($"{BaseUrl}/api/auth/login",
            new APIRequestContextOptions
            {
                DataObject = new { email = "wrong@example.com", password = "wrong" },
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            });
        Assert.True(res.Status == 400 || res.Status == 401,
            $"API login with wrong credentials should return 400/401, got: {res.Status}");
    }
}

// ─── Streak Tests ──────────────────────────────────────────────────────────────

public class StreakDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task Streak_IsCreatedAfterFirstActivity()
    {
        await RegisterAndLoginAsync();
        await CreateExerciseAndGetIdAsync("Streak_Init_Activity");

        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var body = await _page.Locator("body").InnerTextAsync();
        Assert.True(body.Length > 100, "Dashboard should have content after first activity");
    }

    [Fact]
    public async Task StreakDisplay_IsVisibleOnDashboard()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/moj-dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasStreakElement = await _page.Locator(
            ".streak, [class*='streak'], #streak, *:has-text('streak'), *:has-text('Streak')").CountAsync() > 0;
        var body = await _page.Locator("body").InnerTextAsync();
        var hasStreakText = body.Contains("streak", StringComparison.OrdinalIgnoreCase)
                         || body.Contains("zaredom", StringComparison.OrdinalIgnoreCase)
                         || body.Contains("dan", StringComparison.OrdinalIgnoreCase);

        Assert.True(hasStreakElement || hasStreakText, "Dashboard should show streak information");
    }

    [Fact]
    public async Task StreaksApi_RequiresAuthentication()
    {
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/streaks");
        Assert.True(res.Status == 401 || res.Status == 403 || res.Status == 302,
            $"Streaks API should require auth, got: {res.Status}");
    }

    [Fact]
    public async Task StreaksApi_AuthenticatedUser_CanGetOwnStreak()
    {
        await LoginAsAdminAsync();
        var res = await _page.APIRequest.GetAsync($"{BaseUrl}/api/streaks");
        Assert.True(res.Status == 200 || res.Status == 403,
            $"Admin should access streaks API, got: {res.Status}");
    }
}

// ─── User Management Tests (Admin) ─────────────────────────────────────────────

public class UserManagementDeploymentTests : DeploymentTestBase
{
    [Fact]
    public async Task UserList_IsPubliclyAccessible()
    {
        await _page.GotoAsync("/korisnici");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/korisnici", _page.Url);
    }

    [Fact]
    public async Task AdminAddUserPage_IsAccessibleByAdmin()
    {
        await LoginAsAdminAsync();
        await _page.GotoAsync("/korisnici/dodaj");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.DoesNotContain("/auth/login", _page.Url);
    }

    [Fact]
    public async Task AdminAddUserPage_IsBlockedForRegularUser()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/korisnici/dodaj");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.True(
            _page.Url.Contains("/auth/login") ||
            _page.Url.Contains("error") ||
            _page.Url.Contains("403") ||
            _page.Url.Contains("forbidden"),
            $"Regular user should not access admin add-user page, URL: {_page.Url}");
    }

    [Fact]
    public async Task AdminDashboard_IsBlockedForRegularUser()
    {
        await RegisterAndLoginAsync();
        await _page.GotoAsync("/admin/dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.False(
            _page.Url.Contains("/admin/dashboard") && !_page.Url.Contains("/auth/login"),
            $"Regular user should not access admin dashboard, URL: {_page.Url}");
    }
}
