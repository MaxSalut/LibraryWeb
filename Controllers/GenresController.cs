using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <--- ВАЖЛИВО!
using LibraryWeb.Data;

// Якщо Genre.cs у глобальному неймспейсі, using не потрібен.
// Якщо в LibraryWeb.Models, додай: using LibraryWeb.Models;

namespace LibraryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public GenresController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Genres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            if (_context.Genres == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Genres' is null.");
            }
            return await _context.Genres.ToListAsync();
        }

        // GET: api/Genres/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenre(int id)
        {
            if (_context.Genres == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Genres' is null.");
            }
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return NotFound($"Genre with ID {id} not found.");
            }

            return genre;
        }

        // POST: api/Genres
        [HttpPost]
        public async Task<ActionResult<Genre>> PostGenre(Genre genre)
        {
            if (_context.Genres == null)
            {
                return Problem("Entity set 'LibraryDbContext.Genres' is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
        }

        // PUT: api/Genres/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGenre(int id, Genre genre)
        {
            if (id != genre.Id)
            {
                return BadRequest("Genre ID in URL does not match Genre ID in body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(id))
                {
                    return NotFound($"Genre with ID {id} not found.");
                }
                else
                {
                    return StatusCode(500, "A concurrency error occurred while updating the genre.");
                }
            }

            return NoContent();
        }

        // DELETE: api/Genres/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            if (_context.Genres == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Genres' is null.");
            }
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound($"Genre with ID {id} not found.");
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GenreExists(int id)
        {
            return (_context.Genres?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}