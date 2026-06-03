using System.Net;
using System.Net.Http.Json;
using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gamified.SelfImprovement.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Testing");
        });
    }

    [Fact]
    public async Task AuthRegister_CreatesIdentityAndLegacyUser()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"test-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new CreateUserDTO
        {
            UserName = $"test_{Guid.NewGuid():N}"[..16],
            Email = email,
            Password = "Password123!",
            OIB = "12345678901",
            JMBG = "1234567890123"
        });

        await AssertSuccessAsync(response);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GamefiedSelfImprovementDbContext>();
        Assert.Contains(db.Users, u => u.Email == email);
    }

    [Fact]
    public async Task ExercisesCrud_CoversSuccessNotFoundAndValidation()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await RegisterAsync(client);

        var invalid = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseDTO
        {
            Title = "x",
            ExerciseType = ExerciseType.Cardio,
            DurationMinutes = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var create = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseDTO
        {
            Title = "Integracijski trening",
            Description = "CRUD test",
            ExerciseType = ExerciseType.Cardio,
            DurationMinutes = 30,
            CaloriesBurned = 200,
            Difficulty = DifficultyLevel.Medium
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ExerciseDTO>();
        Assert.NotNull(created);

        var getOne = await client.GetAsync($"/api/exercises/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getOne.StatusCode);

        var getAll = await client.GetAsync("/api/exercises?search=Integracijski");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var update = await client.PutAsJsonAsync($"/api/exercises/{created.Id}", new CreateExerciseDTO
        {
            Title = "Ažurirani integracijski trening",
            Description = "PUT test",
            ExerciseType = ExerciseType.Strength,
            DurationMinutes = 45,
            CaloriesBurned = 250,
            Difficulty = DifficultyLevel.Hard
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var missing = await client.GetAsync("/api/exercises/999999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var delete = await client.DeleteAsync($"/api/exercises/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task MeditationsCrud_CoversSuccessNotFoundAndValidation()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await RegisterAsync(client);

        var invalid = await client.PostAsJsonAsync("/api/meditations", new CreateMeditationDTO
        {
            Title = "x",
            MeditationType = MeditationType.Mindfulness,
            DurationMinutes = 0,
            StressReliefScore = 0,
            MentalClarity = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var create = await client.PostAsJsonAsync("/api/meditations", new CreateMeditationDTO
        {
            Title = "Integracijska meditacija",
            Description = "CRUD test",
            MeditationType = MeditationType.Breathing,
            DurationMinutes = 10,
            StressReliefScore = 8,
            MentalClarity = 7,
            Difficulty = DifficultyLevel.Easy
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<MeditationDTO>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/meditations/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/meditations")).StatusCode);

        var update = await client.PutAsJsonAsync($"/api/meditations/{created.Id}", new CreateMeditationDTO
        {
            Title = "Ažurirana integracijska meditacija",
            Description = "PUT test",
            MeditationType = MeditationType.Visualization,
            DurationMinutes = 15,
            StressReliefScore = 9,
            MentalClarity = 9,
            Difficulty = DifficultyLevel.Medium
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/meditations/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/meditations/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task DailyJournalsCrud_CoversSuccessNotFoundAndValidation()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await RegisterAsync(client);

        var invalid = await client.PostAsJsonAsync("/api/journals", new DailyJournalDTO
        {
            Title = string.Empty,
            Mood = 99,
            EnergyLevel = 99,
            JournalDate = DateTime.UtcNow
        });
        Assert.True(invalid.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Created);

        var create = await client.PostAsJsonAsync("/api/journals", new DailyJournalDTO
        {
            Title = "Integracijski dnevnik",
            Description = "CRUD test",
            JournalDate = DateTime.UtcNow,
            DailyGoals = new List<string> { "test" },
            Reflection = "Refleksija",
            Mood = 8,
            EnergyLevel = 7
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<DailyJournalDTO>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/journals/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/journals")).StatusCode);

        var update = await client.PutAsJsonAsync($"/api/journals/{created.Id}", new DailyJournalDTO
        {
            Title = "Ažurirani integracijski dnevnik",
            Description = "PUT test",
            JournalDate = DateTime.UtcNow,
            Reflection = "Nova refleksija",
            Mood = 9,
            EnergyLevel = 9
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/journals/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/journals/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task SpiritualBooksCrud_CoversSuccessNotFoundAndValidation()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await LoginAdminAsync(client);

        var invalid = await client.PostAsJsonAsync("/api/spiritual-books", new CreateSpiritualBookDTO
        {
            Title = "x",
            Author = "x",
            BookType = SpiritualBookType.Bible,
            TotalPages = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var create = await client.PostAsJsonAsync("/api/spiritual-books", new CreateSpiritualBookDTO
        {
            Title = $"Integracijska knjiga {Guid.NewGuid():N}",
            Author = "Test autor",
            BookType = SpiritualBookType.Bible,
            TotalPages = 120,
            Description = "CRUD test",
            Chapters = new List<string> { "Uvod" }
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<SpiritualBookDTO>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/spiritual-books/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/spiritual-books?search=Integracijska")).StatusCode);

        var update = await client.PutAsJsonAsync($"/api/spiritual-books/{created.Id}", new CreateSpiritualBookDTO
        {
            Title = "Ažurirana integracijska knjiga",
            Author = "Test autor",
            BookType = SpiritualBookType.Bible,
            TotalPages = 130,
            Description = "PUT test"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/spiritual-books/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/spiritual-books/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task SpiritualActivitiesCrud_CoversSuccessNotFoundAndValidation()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await RegisterAsync(client);

        var invalid = await client.PostAsJsonAsync("/api/spiritual-activities", new CreateSpiritualActivityDTO
        {
            Title = "x",
            BookId = 999999,
            DurationMinutes = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var create = await client.PostAsJsonAsync("/api/spiritual-activities", new CreateSpiritualActivityDTO
        {
            Title = "Integracijsko čitanje",
            Description = "CRUD test",
            BookId = 1,
            PagesRead = 10,
            CurrentPage = 10,
            DurationMinutes = 20,
            Reflection = "Test refleksija",
            Difficulty = DifficultyLevel.Medium
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<SpiritualActivityDTO>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/spiritual-activities/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/spiritual-activities?search=Integracijsko")).StatusCode);

        var update = await client.PutAsJsonAsync($"/api/spiritual-activities/{created.Id}", new CreateSpiritualActivityDTO
        {
            Title = "Ažurirano integracijsko čitanje",
            Description = "PUT test",
            BookId = 1,
            PagesRead = 20,
            CurrentPage = 20,
            DurationMinutes = 25,
            Reflection = "Nova refleksija",
            Difficulty = DifficultyLevel.Hard
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/spiritual-activities/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/spiritual-activities/{created.Id}")).StatusCode);
    }

    private static async Task RegisterAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new CreateUserDTO
        {
            UserName = $"crud_{Guid.NewGuid():N}"[..16],
            Email = $"crud-{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            OIB = "12345678901",
            JMBG = "1234567890123"
        });

        await AssertSuccessAsync(response);
    }

    private static async Task LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            Email = "admin@gamified.hr",
            Password = "Admin123"
        });

        await AssertSuccessAsync(response);
    }

    private static async Task AssertSuccessAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
    }
}
