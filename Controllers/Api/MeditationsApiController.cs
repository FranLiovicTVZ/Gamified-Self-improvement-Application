using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za Meditation - meditacijske sesije sa CRUD operacijama
/// </summary>
[ApiController]
[Route("api/meditations")]
public class MeditationsApiController : BaseApiController
{
    public MeditationsApiController(GamefiedSelfImprovementDbContext dbContext) 
        : base(dbContext)
    {
    }

    /// <summary>
    /// GET: /api/meditations - Dohvati sve meditacije
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MeditationDTO>>> GetAll(
        [FromQuery] MeditationType? type = null,
        [FromQuery] DifficultyLevel? difficulty = null)
    {
        try
        {
            var query = _dbContext.Meditations.AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(m => m.MeditationType == type);
            }

            if (difficulty.HasValue)
            {
                query = query.Where(m => m.Difficulty == difficulty);
            }

            var meditations = await query
                .OrderByDescending(m => m.CompletedDate)
                .ToListAsync();

            return Ok(meditations.Select(m => MapMeditationToDTO(m)).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju meditacija", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/meditations/{id} - Dohvati jednu meditaciju
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MeditationDTO>> GetById(int id)
    {
        try
        {
            var meditation = await _dbContext.Meditations.FindAsync(id);

            if (meditation == null)
            {
                return NotFound(new { message = $"Meditacija sa ID {id} nije pronađena" });
            }

            return Ok(MapMeditationToDTO(meditation));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju meditacije", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: /api/meditations - Kreiraj novu meditaciju
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MeditationDTO>> Create([FromBody] CreateMeditationDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var meditation = new Meditation
            {
                AppUserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                MeditationType = dto.MeditationType,
                DurationMinutes = dto.DurationMinutes,
                AudioFilePath = dto.AudioFilePath,
                FocusArea = dto.FocusArea,
                StressReliefScore = dto.StressReliefScore,
                MentalClarity = dto.MentalClarity,
                Notes = dto.Notes,
                Difficulty = dto.Difficulty,
                CompletedDate = DateTime.UtcNow
            };

            meditation.XpReward = meditation.CalculateXP();

            _dbContext.Meditations.Add(meditation);
            await _dbContext.SaveChangesAsync();

            // Ažuriraj streak korisnika
            if (!string.IsNullOrEmpty(userId))
            {
                await UpdateStreakForUserAsync(userId);
            }

            return CreatedAtAction(nameof(GetById), new { id = meditation.Id }, MapMeditationToDTO(meditation));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri kreiranju meditacije", error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: /api/meditations/{id} - Ažuriraj meditaciju
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<MeditationDTO>> Update(int id, [FromBody] CreateMeditationDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var meditation = await _dbContext.Meditations.FindAsync(id);

            if (meditation == null)
            {
                return NotFound(new { message = $"Meditacija sa ID {id} nije pronađena" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (meditation.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            meditation.Title = dto.Title;
            meditation.Description = dto.Description;
            meditation.MeditationType = dto.MeditationType;
            meditation.DurationMinutes = dto.DurationMinutes;
            meditation.AudioFilePath = dto.AudioFilePath;
            meditation.FocusArea = dto.FocusArea;
            meditation.StressReliefScore = dto.StressReliefScore;
            meditation.MentalClarity = dto.MentalClarity;
            meditation.Notes = dto.Notes;
            meditation.Difficulty = dto.Difficulty;
            meditation.XpReward = meditation.CalculateXP();

            await _dbContext.SaveChangesAsync();

            return Ok(MapMeditationToDTO(meditation));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju meditacije", error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: /api/meditations/{id} - Obriši meditaciju
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var meditation = await _dbContext.Meditations.FindAsync(id);

            if (meditation == null)
            {
                return NotFound(new { message = $"Meditacija sa ID {id} nije pronađena" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (meditation.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _dbContext.Meditations.Remove(meditation);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri brisanju meditacije", error = ex.Message });
        }
    }
}
