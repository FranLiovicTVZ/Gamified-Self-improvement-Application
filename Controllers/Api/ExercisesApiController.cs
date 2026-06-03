using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za Exercise - vježbe sa CRUD operacijama
/// </summary>
[ApiController]
[Route("api/exercises")]
public class ExercisesApiController : BaseApiController
{
    public ExercisesApiController(GamefiedSelfImprovementDbContext dbContext) 
        : base(dbContext)
    {
    }

    /// <summary>
    /// GET: /api/exercises - Dohvati sve vježbe
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ExerciseDTO>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] ExerciseType? type = null,
        [FromQuery] DifficultyLevel? difficulty = null)
    {
        try
        {
            var query = _dbContext.Exercises.AsQueryable();

            // Pretraga po naslovu ili opisu
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => 
                    e.Title.Contains(search) || 
                    e.Description.Contains(search));
            }

            // Filtriranje po tipu
            if (type.HasValue)
            {
                query = query.Where(e => e.ExerciseType == type);
            }

            // Filtriranje po težini
            if (difficulty.HasValue)
            {
                query = query.Where(e => e.Difficulty == difficulty);
            }

            var exercises = await query
                .OrderByDescending(e => e.CompletedDate)
                .ToListAsync();

            var result = exercises.Select(e => MapExerciseToDTO(e)).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju vježbi", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/exercises/{id} - Dohvati jednu vježbu po ID-u
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExerciseDTO>> GetById(int id)
    {
        try
        {
            var exercise = await _dbContext.Exercises.FindAsync(id);

            if (exercise == null)
            {
                return NotFound(new { message = $"Vježba sa ID {id} nije pronađena" });
            }

            return Ok(MapExerciseToDTO(exercise));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju vježbe", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: /api/exercises - Kreiraj novu vježbu
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ExerciseDTO>> Create([FromBody] CreateExerciseDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var exercise = new Exercise
            {
                AppUserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                ExerciseType = dto.ExerciseType,
                DurationMinutes = dto.DurationMinutes,
                CaloriesBurned = dto.CaloriesBurned,
                Sets = dto.Sets,
                Reps = dto.Reps,
                Weight = dto.Weight,
                MuscleGroups = dto.MuscleGroups,
                Location = dto.Location,
                Difficulty = dto.Difficulty,
                CompletedDate = DateTime.UtcNow
            };

            exercise.XpReward = exercise.CalculateXP();

            _dbContext.Exercises.Add(exercise);
            await _dbContext.SaveChangesAsync();

            // Ažuriraj streak korisnika
            if (!string.IsNullOrEmpty(userId))
            {
                await UpdateStreakForUserAsync(userId);
            }

            return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, MapExerciseToDTO(exercise));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri kreiranju vježbe", error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: /api/exercises/{id} - Ažuriraj vježbu
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ExerciseDTO>> Update(int id, [FromBody] CreateExerciseDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exercise = await _dbContext.Exercises.FindAsync(id);

            if (exercise == null)
            {
                return NotFound(new { message = $"Vježba sa ID {id} nije pronađena" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (exercise.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            exercise.Title = dto.Title;
            exercise.Description = dto.Description;
            exercise.ExerciseType = dto.ExerciseType;
            exercise.DurationMinutes = dto.DurationMinutes;
            exercise.CaloriesBurned = dto.CaloriesBurned;
            exercise.Sets = dto.Sets;
            exercise.Reps = dto.Reps;
            exercise.Weight = dto.Weight;
            exercise.MuscleGroups = dto.MuscleGroups;
            exercise.Location = dto.Location;
            exercise.Difficulty = dto.Difficulty;
            exercise.XpReward = exercise.CalculateXP();

            await _dbContext.SaveChangesAsync();

            return Ok(MapExerciseToDTO(exercise));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju vježbe", error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: /api/exercises/{id} - Obriši vježbu
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var exercise = await _dbContext.Exercises.FindAsync(id);

            if (exercise == null)
            {
                return NotFound(new { message = $"Vježba sa ID {id} nije pronađena" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (exercise.AppUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _dbContext.Exercises.Remove(exercise);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri brisanju vježbe", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/exercises/user/{userId} - Dohvati sve vježbe korisnika
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ExerciseDTO>>> GetByUserId(string userId)
    {
        try
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (userId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var exercises = await _dbContext.Exercises
                .Where(e => e.AppUserId == userId)
                .OrderByDescending(e => e.CompletedDate)
                .ToListAsync();

            var result = exercises.Select(e => MapExerciseToDTO(e)).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju korisnikovih vježbi", error = ex.Message });
        }
    }
}
