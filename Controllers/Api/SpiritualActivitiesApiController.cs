using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za SpiritualActivity - čitanje duhovnih tekstova
/// </summary>
[ApiController]
[Route("api/spiritual-activities")]
public class SpiritualActivitiesApiController : BaseApiController
{
    public SpiritualActivitiesApiController(GamefiedSelfImprovementDbContext dbContext)
        : base(dbContext)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SpiritualActivityDTO>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? bookId = null)
    {
        var query = _dbContext.SpiritualActivities
            .Include(a => a.Book)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a =>
                a.Title.Contains(search) ||
                a.Description.Contains(search) ||
                a.Reflection.Contains(search));
        }

        if (bookId.HasValue)
        {
            query = query.Where(a => a.BookId == bookId);
        }

        var activities = await query
            .OrderByDescending(a => a.CompletedDate)
            .ToListAsync();

        return Ok(activities.Select(MapSpiritualActivityToDTO).ToList());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SpiritualActivityDTO>> GetById(int id)
    {
        var activity = await _dbContext.SpiritualActivities
            .Include(a => a.Book)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
        {
            return NotFound(new { message = $"Duhovna aktivnost sa ID {id} nije pronađena" });
        }

        return Ok(MapSpiritualActivityToDTO(activity));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SpiritualActivityDTO>> Create([FromBody] CreateSpiritualActivityDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var book = await _dbContext.SpiritualBooks.FindAsync(dto.BookId);
        if (book == null)
        {
            return BadRequest(new { message = "Knjiga nije pronađena" });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var activity = new SpiritualActivity
        {
            AppUserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            BookId = dto.BookId,
            PagesRead = dto.PagesRead,
            CurrentPage = dto.CurrentPage,
            DurationMinutes = dto.DurationMinutes,
            Reflection = dto.Reflection,
            IsCompleted = dto.IsCompleted,
            Difficulty = dto.Difficulty,
            CompletedDate = DateTime.UtcNow,
            StartDate = DateTime.UtcNow
        };

        activity.XpReward = activity.CalculateXP();

        _dbContext.SpiritualActivities.Add(activity);
        await _dbContext.SaveChangesAsync();

        if (!string.IsNullOrEmpty(userId))
        {
            await UpdateStreakForUserAsync(userId);
        }

        await _dbContext.Entry(activity).Reference(a => a.Book).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = activity.Id }, MapSpiritualActivityToDTO(activity));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<SpiritualActivityDTO>> Update(int id, [FromBody] CreateSpiritualActivityDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var activity = await _dbContext.SpiritualActivities
            .Include(a => a.Book)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
        {
            return NotFound(new { message = $"Duhovna aktivnost sa ID {id} nije pronađena" });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (activity.AppUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        activity.Title = dto.Title;
        activity.Description = dto.Description;
        activity.BookId = dto.BookId;
        activity.PagesRead = dto.PagesRead;
        activity.CurrentPage = dto.CurrentPage;
        activity.DurationMinutes = dto.DurationMinutes;
        activity.Reflection = dto.Reflection;
        activity.IsCompleted = dto.IsCompleted;
        activity.Difficulty = dto.Difficulty;
        activity.XpReward = activity.CalculateXP();

        await _dbContext.SaveChangesAsync();

        return Ok(MapSpiritualActivityToDTO(activity));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var activity = await _dbContext.SpiritualActivities.FindAsync(id);

        if (activity == null)
        {
            return NotFound(new { message = $"Duhovna aktivnost sa ID {id} nije pronađena" });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (activity.AppUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _dbContext.SpiritualActivities.Remove(activity);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
