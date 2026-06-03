using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za Streak - praćenje broja dana aktivnosti za redom
/// </summary>
[ApiController]
[Route("api/streaks")]
public class StreaksApiController : BaseApiController
{
    public StreaksApiController(GamefiedSelfImprovementDbContext dbContext) 
        : base(dbContext)
    {
    }

    /// <summary>
    /// GET: /api/streaks - Dohvati sve streake
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<StreakDTO>>> GetAll()
    {
        try
        {
            var streaks = await _dbContext.Streaks
                .Include(s => s.AppUser)
                .ToListAsync();

            var dtos = streaks.Select(s => new StreakDTO
            {
                Id = s.Id,
                UserId = s.UserId,
                CurrentStreak = s.CurrentStreak,
                LongestStreak = s.LongestStreak,
                LastActivityDate = s.LastActivityDate,
                LastStreakResetDate = s.LastStreakResetDate,
                TotalActivitiesCompleted = s.TotalActivitiesCompleted,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvatu streaka", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/streaks/{id} - Dohvati jedan streak
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<StreakDTO>> GetById(int id)
    {
        try
        {
            var streak = await _dbContext.Streaks
                .Include(s => s.AppUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (streak == null)
                return NotFound(new { message = $"Streak sa ID {id} nije pronađen" });

            // Provjeri da li korisnik gleda svoj streak ili je admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (streak.UserId != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var dto = new StreakDTO
            {
                Id = streak.Id,
                UserId = streak.UserId,
                CurrentStreak = streak.CurrentStreak,
                LongestStreak = streak.LongestStreak,
                LastActivityDate = streak.LastActivityDate,
                LastStreakResetDate = streak.LastStreakResetDate,
                TotalActivitiesCompleted = streak.TotalActivitiesCompleted,
                CreatedDate = streak.CreatedDate,
                UpdatedDate = streak.UpdatedDate
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvatu streaka", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/streaks/user/{userId} - Dohvati streak za specifičnog korisnika
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<StreakDTO>> GetByUserId(string userId)
    {
        try
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var streak = await _dbContext.Streaks
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (streak == null)
            {
                // Kreiraj novi streak ako ne postoji
                var newStreak = new Streak(userId);
                _dbContext.Streaks.Add(newStreak);
                await _dbContext.SaveChangesAsync();
                return Created($"/api/streaks/{newStreak.Id}", MapStreakToDTO(newStreak));
            }

            return Ok(MapStreakToDTO(streak));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvatu streaka", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: /api/streaks/record-activity - Zapiši obavljenu aktivnost i ažuriraj streak
    /// </summary>
    [HttpPost("record-activity")]
    [Authorize]
    public async Task<ActionResult<StreakDTO>> RecordActivity([FromBody] UpdateStreakDTO dto)
    {
        try
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (dto.UserId != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var streak = await _dbContext.Streaks
                .FirstOrDefaultAsync(s => s.UserId == dto.UserId);

            if (streak == null)
            {
                // Kreiraj novi streak ako ne postoji
                streak = new Streak(dto.UserId);
                _dbContext.Streaks.Add(streak);
                await _dbContext.SaveChangesAsync();
            }

            // Provjeri da li trebam resetirati streak
            streak.CheckAndResetStreak();

            // Zapiši novu aktivnost
            streak.RecordActivity();
            _dbContext.Streaks.Update(streak);
            await _dbContext.SaveChangesAsync();

            return Ok(MapStreakToDTO(streak));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju streaka", error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: /api/streaks/{id} - Ažuriraj streak
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StreakDTO>> Update(int id, [FromBody] StreakDTO dto)
    {
        try
        {
            var streak = await _dbContext.Streaks.FindAsync(id);
            if (streak == null)
                return NotFound(new { message = $"Streak sa ID {id} nije pronađen" });

            streak.CurrentStreak = dto.CurrentStreak;
            streak.LongestStreak = dto.LongestStreak;
            streak.LastActivityDate = dto.LastActivityDate;
            streak.UpdatedDate = DateTime.UtcNow;

            _dbContext.Streaks.Update(streak);
            await _dbContext.SaveChangesAsync();

            return Ok(MapStreakToDTO(streak));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju streaka", error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: /api/streaks/{id} - Obriši streak
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var streak = await _dbContext.Streaks.FindAsync(id);
            if (streak == null)
                return NotFound(new { message = $"Streak sa ID {id} nije pronađen" });

            _dbContext.Streaks.Remove(streak);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Streak je uspješno obrisan" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri brisanju streaka", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/streaks/top/leaderboard - Dohvati top 10 korisnike po streaku
    /// </summary>
    [HttpGet("top/leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetLeaderboard()
    {
        try
        {
            var topStreaks = await _dbContext.Streaks
                .Include(s => s.AppUser)
                .OrderByDescending(s => s.LongestStreak)
                .ThenByDescending(s => s.CurrentStreak)
                .Take(10)
                .Select(s => new
                {
                    rank = 0, // Dinamički se postavlja u controlleru
                    userId = s.UserId,
                    userName = s.AppUser!.UserName,
                    currentStreak = s.CurrentStreak,
                    longestStreak = s.LongestStreak,
                    totalActivities = s.TotalActivitiesCompleted
                })
                .ToListAsync();

            // Postavi rank
            var rankedStreaks = topStreaks
                .Select((s, index) => new
                {
                    rank = index + 1,
                    s.userId,
                    s.userName,
                    s.currentStreak,
                    s.longestStreak,
                    s.totalActivities
                })
                .ToList();

            return Ok(rankedStreaks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvatu leaderboarda", error = ex.Message });
        }
    }
}
