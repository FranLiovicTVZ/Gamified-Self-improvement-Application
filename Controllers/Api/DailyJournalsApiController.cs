using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za DailyJournal sa CRUD operacijama
/// </summary>
[ApiController]
[Route("api/daily-journals")]
[Route("api/journals")]
public class DailyJournalsApiController : BaseApiController
{
    public DailyJournalsApiController(GamefiedSelfImprovementDbContext dbContext) 
        : base(dbContext)
    {
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DailyJournalDTO>>> GetAll()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var journals = await _dbContext.DailyJournals
                .Where(j => j.AppUserId == userId)
                .OrderByDescending(j => j.JournalDate)
                .ToListAsync();

            return Ok(journals.Select(j => MapDailyJournalToDTO(j)).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju dnevnika", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<DailyJournalDTO>> GetById(int id)
    {
        try
        {
            var journal = await _dbContext.DailyJournals.FindAsync(id);

            if (journal == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (journal.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return Ok(MapDailyJournalToDTO(journal));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju dnevnika", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<DailyJournalDTO>> Create([FromBody] DailyJournalDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var journal = new DailyJournal
            {
                AppUserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                JournalDate = dto.JournalDate,
                DailyGoals = dto.DailyGoals,
                Ambitions = dto.Ambitions,
                Accomplishments = dto.Accomplishments,
                Reflection = dto.Reflection,
                Mood = dto.Mood,
                EnergyLevel = dto.EnergyLevel,
                Challenges = dto.Challenges,
                CompletedDate = DateTime.UtcNow
            };

            journal.XpReward = journal.CalculateXP();

            _dbContext.DailyJournals.Add(journal);
            await _dbContext.SaveChangesAsync();

            // Ažuriraj streak korisnika
            if (!string.IsNullOrEmpty(userId))
            {
                await UpdateStreakForUserAsync(userId);
            }

            return CreatedAtAction(nameof(GetById), new { id = journal.Id }, MapDailyJournalToDTO(journal));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri kreiranju dnevnika", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<DailyJournalDTO>> Update(int id, [FromBody] DailyJournalDTO dto)
    {
        try
        {
            var journal = await _dbContext.DailyJournals.FindAsync(id);

            if (journal == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (journal.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            journal.Title = dto.Title;
            journal.Description = dto.Description;
            journal.DailyGoals = dto.DailyGoals;
            journal.Ambitions = dto.Ambitions;
            journal.Accomplishments = dto.Accomplishments;
            journal.Reflection = dto.Reflection;
            journal.Mood = dto.Mood;
            journal.EnergyLevel = dto.EnergyLevel;
            journal.Challenges = dto.Challenges;
            journal.XpReward = journal.CalculateXP();

            await _dbContext.SaveChangesAsync();

            return Ok(MapDailyJournalToDTO(journal));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju dnevnika", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var journal = await _dbContext.DailyJournals.FindAsync(id);

            if (journal == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (journal.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _dbContext.DailyJournals.Remove(journal);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri brisanju dnevnika", error = ex.Message });
        }
    }
}
