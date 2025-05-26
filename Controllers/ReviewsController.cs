using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryWeb.Data;
using LibraryWeb.DTOs;
// using LibraryWeb.Models; // Якщо Review.cs має такий неймспейс

namespace LibraryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ReviewsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews([FromQuery] int? bookId, [FromQuery] int? userId)
        {
            if (_context.Reviews == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Reviews' is null.");
            }

            var query = _context.Reviews
                .Include(r => r.Book)  // Включаємо для отримання BookTitle
                .Include(r => r.User)  // Включаємо для отримання Username
                .AsQueryable();

            if (bookId.HasValue)
            {
                query = query.Where(r => r.BookId == bookId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(r => r.UserId == userId.Value);
            }

            var reviews = await query
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    BookId = r.BookId,
                    BookTitle = r.Book != null ? r.Book.Title : null, // Перевірка на null
                    UserId = r.UserId,
                    Username = r.User != null ? r.User.Username : null // Перевірка на null
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            if (_context.Reviews == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Reviews' is null.");
            }

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Id == id)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    BookId = r.BookId,
                    BookTitle = r.Book != null ? r.Book.Title : null,
                    UserId = r.UserId,
                    Username = r.User != null ? r.User.Username : null
                })
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }

            return Ok(review);
        }

        // POST: api/Reviews
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> PostReview(ReviewCreateDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Перевірка існування книги та користувача
            if (!await _context.Books.AnyAsync(b => b.Id == reviewDto.BookId))
            {
                ModelState.AddModelError(nameof(reviewDto.BookId), $"Book with ID {reviewDto.BookId} not found.");
            }
            if (!await _context.Users.AnyAsync(u => u.Id == reviewDto.UserId))
            {
                ModelState.AddModelError(nameof(reviewDto.UserId), $"User with ID {reviewDto.UserId} not found.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Перевірка, чи користувач вже залишав відгук на цю книгу (опціонально, залежить від бізнес-логіки)
            // if (await _context.Reviews.AnyAsync(r => r.BookId == reviewDto.BookId && r.UserId == reviewDto.UserId))
            // {
            //     return Conflict("User has already reviewed this book.");
            // }

            var review = new Review // Створюємо сутність Review
            {
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                BookId = reviewDto.BookId,
                UserId = reviewDto.UserId,
                ReviewDate = DateTime.UtcNow // Встановлюємо поточну дату відгуку
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Створюємо ReviewDto для відповіді
            var createdReviewDto = new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                BookId = review.BookId,
                // Для BookTitle та Username можемо завантажити їх, якщо потрібно для відповіді
                // Або залишити null, якщо GetReview їх завантажить при запиті на новостворений ресурс
                UserId = review.UserId
            };
            // Щоб отримати BookTitle та Username для відповіді:
            var responseReview = await GetReview(review.Id);
            if (responseReview.Result is OkObjectResult okResult)
            {
                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, okResult.Value);
            }

            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, createdReviewDto);
        }

        // PUT: api/Reviews/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, ReviewUpdateDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewToUpdate = await _context.Reviews.FindAsync(id);

            if (reviewToUpdate == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }

            // Тут можна додати логіку авторизації: чи має поточний користувач право редагувати цей відгук
            // Наприклад, if (reviewToUpdate.UserId != currentUserId) return Forbid();

            reviewToUpdate.Rating = reviewDto.Rating;
            reviewToUpdate.Comment = reviewDto.Comment;
            // ReviewDate можна оновлювати або залишати початковою
            // reviewToUpdate.ReviewDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound($"Review with ID {id} not found during concurrency check.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }

            // Тут можна додати логіку авторизації

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id)
        {
            return (_context.Reviews?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}