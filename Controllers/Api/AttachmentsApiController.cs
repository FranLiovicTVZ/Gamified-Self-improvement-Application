using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za Attachment (datoteke) sa CRUD operacijama i upload
/// </summary>
[ApiController]
[Route("api/attachments")]
public class AttachmentsApiController : BaseApiController
{
    private readonly IWebHostEnvironment _env;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip" };

    public AttachmentsApiController(GamefiedSelfImprovementDbContext dbContext, IWebHostEnvironment env) 
        : base(dbContext)
    {
        _env = env;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AttachmentDTO>>> GetAll([FromQuery] int? activityId = null)
    {
        try
        {
            var query = _dbContext.Attachments.Where(a => !a.IsDeleted);

            if (activityId.HasValue)
            {
                query = query.Where(a => a.ActivityId == activityId);
            }

            var attachments = await query.OrderByDescending(a => a.UploadedDate).ToListAsync();
            return Ok(attachments.Select(a => MapAttachmentToDTO(a)).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju datoteka", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<AttachmentDTO>> GetById(int id)
    {
        try
        {
            var attachment = await _dbContext.Attachments.FindAsync(id);

            if (attachment == null || attachment.IsDeleted)
            {
                return NotFound();
            }

            return Ok(MapAttachmentToDTO(attachment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju datoteke", error = ex.Message });
        }
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<ActionResult<AttachmentDTO>> Upload([FromForm] IFormFile file, [FromForm] int? activityId = null, [FromForm] string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Datoteka nije pronađena" });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { message = $"Datoteka je prevelika. Maksimalna veličina je {MaxFileSize / (1024 * 1024)} MB" });
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Tip datoteke nije dozvoljen" });
            }

            // Validacija ActivityId ako je proslijeđen
            if (activityId.HasValue)
            {
                var activity = await _dbContext.Activities.FindAsync(activityId);
                if (activity == null)
                {
                    return NotFound(new { message = "Aktivnost nije pronađena" });
                }
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", userId ?? "temp");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                ActivityId = activityId,
                UserId = userId,
                FileName = file.FileName,
                FilePath = $"/uploads/{userId}/{fileName}",
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                Description = description ?? string.Empty,
                UploadedDate = DateTime.UtcNow
            };

            _dbContext.Attachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = attachment.Id }, MapAttachmentToDTO(attachment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri uploadu datoteke", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<AttachmentDTO>> Update(int id, [FromBody] CreateAttachmentDTO dto)
    {
        try
        {
            var attachment = await _dbContext.Attachments.FindAsync(id);

            if (attachment == null || attachment.IsDeleted)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (attachment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            attachment.Description = dto.Description;
            if (dto.ActivityId.HasValue)
            {
                attachment.ActivityId = dto.ActivityId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(MapAttachmentToDTO(attachment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju datoteke", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var attachment = await _dbContext.Attachments.FindAsync(id);

            if (attachment == null || attachment.IsDeleted)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (attachment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Obriši fizičku datoteku
            var physicalPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            // Označi kao obrisanu (soft delete)
            attachment.IsDeleted = true;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri brisanju datoteke", error = ex.Message });
        }
    }

    [HttpPost("{id}/download")]
    [Authorize]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var attachment = await _dbContext.Attachments.FindAsync(id);

            if (attachment == null || attachment.IsDeleted)
            {
                return NotFound();
            }

            var physicalPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new { message = "Datoteka nije pronađena na disku" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            return File(fileBytes, attachment.ContentType, attachment.FileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri preuzimanju datoteke", error = ex.Message });
        }
    }
}
