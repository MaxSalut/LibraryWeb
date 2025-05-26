using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Http тут не обов'язковий, якщо є Mvc
using Microsoft.EntityFrameworkCore; // <--- ВАЖЛИВО!
using LibraryWeb.Data;

// Якщо Publisher.cs у глобальному неймспейсі, using не потрібен.
// Якщо в LibraryWeb.Models, додай: using LibraryWeb.Models;

namespace LibraryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public PublishersController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Publishers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Publisher>>> GetPublishers()
        {
            if (_context.Publishers == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Publishers' is null.");
            }
            return await _context.Publishers.ToListAsync();
        }

        // GET: api/Publishers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Publisher>> GetPublisher(int id)
        {
            if (_context.Publishers == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Publishers' is null.");
            }
            var publisher = await _context.Publishers.FindAsync(id);

            if (publisher == null)
            {
                return NotFound($"Publisher with ID {id} not found.");
            }

            return publisher;
        }

        // POST: api/Publishers
        [HttpPost]
        public async Task<ActionResult<Publisher>> PostPublisher(Publisher publisher)
        {
            if (_context.Publishers == null)
            {
                return Problem("Entity set 'LibraryDbContext.Publishers' is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPublisher), new { id = publisher.Id }, publisher);
        }

        // PUT: api/Publishers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublisher(int id, Publisher publisher)
        {
            if (id != publisher.Id)
            {
                return BadRequest("Publisher ID in URL does not match Publisher ID in body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(publisher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublisherExists(id))
                {
                    return NotFound($"Publisher with ID {id} not found.");
                }
                else
                {
                    return StatusCode(500, "A concurrency error occurred while updating the publisher.");
                }
            }

            return NoContent();
        }

        // DELETE: api/Publishers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            if (_context.Publishers == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Publishers' is null.");
            }
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound($"Publisher with ID {id} not found.");
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PublisherExists(int id)
        {
            return (_context.Publishers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}