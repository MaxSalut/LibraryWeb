using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryWeb.Data;     // Для DbContext
using LibraryWeb.DTOs;     // Для DTO
// Якщо моделі сутностей (Book, BookGenre etc.) у глобальному неймспейсі, using не потрібен.
// Якщо в LibraryWeb.Models, додай: using LibraryWeb.Models;

namespace LibraryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            if (_context.Books == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Books' is null.");
            }

            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    PublicationDate = b.PublicationDate,
                    ISBN = b.ISBN,
                    Author = b.Author != null ? new AuthorDto { Id = b.Author.Id, FirstName = b.Author.FirstName, LastName = b.Author.LastName } : null,
                    Publisher = b.Publisher != null ? new PublisherDto { Id = b.Publisher.Id, Name = b.Publisher.Name } : null,
                    Genres = b.BookGenres.Select(bg => new GenreDto { Id = bg.Genre.Id, Name = bg.Genre.Name }).ToList()
                })
                .ToListAsync();

            return Ok(books);
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            if (_context.Books == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Books' is null.");
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Where(b => b.Id == id)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    PublicationDate = b.PublicationDate,
                    ISBN = b.ISBN,
                    Author = b.Author != null ? new AuthorDto { Id = b.Author.Id, FirstName = b.Author.FirstName, LastName = b.Author.LastName } : null,
                    Publisher = b.Publisher != null ? new PublisherDto { Id = b.Publisher.Id, Name = b.Publisher.Name } : null,
                    Genres = b.BookGenres.Select(bg => new GenreDto { Id = bg.Genre.Id, Name = bg.Genre.Name }).ToList()
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            return Ok(book);
        }

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<BookDto>> PostBook(BookCreateDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.Authors.AnyAsync(a => a.Id == bookDto.AuthorId))
            {
                ModelState.AddModelError(nameof(bookDto.AuthorId), $"Author with ID {bookDto.AuthorId} not found.");
            }
            if (bookDto.PublisherId.HasValue && !await _context.Publishers.AnyAsync(p => p.Id == bookDto.PublisherId.Value))
            {
                ModelState.AddModelError(nameof(bookDto.PublisherId), $"Publisher with ID {bookDto.PublisherId.Value} not found.");
            }
            if (bookDto.GenreIds != null)
            {
                foreach (var genreId in bookDto.GenreIds)
                {
                    if (!await _context.Genres.AnyAsync(g => g.Id == genreId))
                    {
                        ModelState.AddModelError(nameof(bookDto.GenreIds), $"Genre with ID {genreId} not found.");
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = new Book // Створюємо сутність Book
            {
                Title = bookDto.Title,
                Description = bookDto.Description,
                PublicationDate = bookDto.PublicationDate,
                ISBN = bookDto.ISBN,
                AuthorId = bookDto.AuthorId,
                PublisherId = bookDto.PublisherId
            };

            if (bookDto.GenreIds != null && bookDto.GenreIds.Any())
            {
                foreach (var genreId in bookDto.GenreIds)
                {
                    book.BookGenres.Add(new BookGenre { GenreId = genreId });
                }
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var createdBookResult = await GetBook(book.Id);
            if (createdBookResult.Result is OkObjectResult okResult && okResult.Value is BookDto createdBookDto)
            {
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, createdBookDto);
            }
            return Problem("Failed to retrieve created book details for the response.");
        }

        // PUT: api/Books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, BookUpdateDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookToUpdate = await _context.Books
                .Include(b => b.BookGenres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bookToUpdate == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            if (bookToUpdate.AuthorId != bookDto.AuthorId && !await _context.Authors.AnyAsync(a => a.Id == bookDto.AuthorId))
            {
                ModelState.AddModelError(nameof(bookDto.AuthorId), $"Author with ID {bookDto.AuthorId} not found.");
            }
            if (bookDto.PublisherId.HasValue && bookToUpdate.PublisherId != bookDto.PublisherId && !await _context.Publishers.AnyAsync(p => p.Id == bookDto.PublisherId.Value))
            {
                ModelState.AddModelError(nameof(bookDto.PublisherId), $"Publisher with ID {bookDto.PublisherId.Value} not found.");
            }
            if (bookDto.GenreIds != null)
            {
                foreach (var genreId in bookDto.GenreIds)
                {
                    if (!await _context.Genres.AnyAsync(g => g.Id == genreId))
                    {
                        ModelState.AddModelError(nameof(bookDto.GenreIds), $"Genre with ID {genreId} not found.");
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bookToUpdate.Title = bookDto.Title;
            bookToUpdate.Description = bookDto.Description;
            bookToUpdate.PublicationDate = bookDto.PublicationDate;
            bookToUpdate.ISBN = bookDto.ISBN;
            bookToUpdate.AuthorId = bookDto.AuthorId;
            bookToUpdate.PublisherId = bookDto.PublisherId;

            _context.BookGenres.RemoveRange(bookToUpdate.BookGenres); // Видаляємо старі зв'язки
            bookToUpdate.BookGenres.Clear(); // Очищаємо колекцію в самій сутності

            if (bookDto.GenreIds != null && bookDto.GenreIds.Any())
            {
                foreach (var genreId in bookDto.GenreIds)
                {
                    bookToUpdate.BookGenres.Add(new BookGenre { BookId = bookToUpdate.Id, GenreId = genreId });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound($"Book with ID {id} not found during concurrency check.");
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            if (await _context.BorrowingRecords.AnyAsync(br => br.BookId == id && br.ReturnDate == null))
            {
                return BadRequest("Cannot delete book. It is currently borrowed and not returned.");
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}