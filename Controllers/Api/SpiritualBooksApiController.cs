using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za SpiritualBook sa CRUD operacijama
/// </summary>
[ApiController]
[Route("api/spiritual-books")]
public class SpiritualBooksApiController : BaseApiController
{
    public SpiritualBooksApiController(GamefiedSelfImprovementDbContext dbContext) 
        : base(dbContext)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SpiritualBookDTO>>> GetAll(
        [FromQuery] SpiritualBookType? type = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var query = _dbContext.SpiritualBooks.Where(b => b.IsAvailable);

            if (type.HasValue)
            {
                query = query.Where(b => b.BookType == type);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => 
                    b.Title.Contains(search) || 
                    b.Author.Contains(search) ||
                    b.Description.Contains(search));
            }

            var books = await query.ToListAsync();
            return Ok(books.Select(b => MapSpiritualBookToDTO(b)).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju knjiga", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SpiritualBookDTO>> GetById(int id)
    {
        try
        {
            var book = await _dbContext.SpiritualBooks.FindAsync(id);

            if (book == null || !book.IsAvailable)
            {
                return NotFound();
            }

            return Ok(MapSpiritualBookToDTO(book));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju knjige", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpiritualBookDTO>> Create([FromBody] CreateSpiritualBookDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = new SpiritualBook
            {
                Title = dto.Title,
                BookType = dto.BookType,
                TotalPages = dto.TotalPages,
                Description = dto.Description,
                Author = dto.Author,
                Language = dto.Language,
                Chapters = dto.Chapters,
                IsAvailable = true
            };

            _dbContext.SpiritualBooks.Add(book);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, MapSpiritualBookToDTO(book));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri kreiranju knjige", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpiritualBookDTO>> Update(int id, [FromBody] CreateSpiritualBookDTO dto)
    {
        try
        {
            var book = await _dbContext.SpiritualBooks.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            book.Title = dto.Title;
            book.BookType = dto.BookType;
            book.TotalPages = dto.TotalPages;
            book.Description = dto.Description;
            book.Author = dto.Author;
            book.Language = dto.Language;
            book.Chapters = dto.Chapters;

            await _dbContext.SaveChangesAsync();

            return Ok(MapSpiritualBookToDTO(book));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju knjige", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var book = await _dbContext.SpiritualBooks.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            book.IsAvailable = false;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri brisanju knjige", error = ex.Message });
        }
    }
}
