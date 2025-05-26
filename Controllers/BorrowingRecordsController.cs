using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryWeb.Data;
using LibraryWeb.DTOs;
// using LibraryWeb.Models; // Якщо BorrowingRecord.cs має такий неймспейс

namespace LibraryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowingRecordsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BorrowingRecordsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/BorrowingRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowingRecordDto>>> GetBorrowingRecords(
            [FromQuery] int? userId,
            [FromQuery] int? bookId,
            [FromQuery] bool? onlyNotReturned)
        {
            if (_context.BorrowingRecords == null)
            {
                return NotFound("Entity set 'LibraryDbContext.BorrowingRecords' is null.");
            }

            var query = _context.BorrowingRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(br => br.UserId == userId.Value);
            }

            if (bookId.HasValue)
            {
                query = query.Where(br => br.BookId == bookId.Value);
            }

            if (onlyNotReturned.HasValue && onlyNotReturned.Value)
            {
                query = query.Where(br => br.ReturnDate == null);
            }

            var records = await query
                .OrderByDescending(br => br.BorrowDate) // Показуємо новіші першими
                .Select(br => new BorrowingRecordDto
                {
                    Id = br.Id,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    BookId = br.BookId,
                    BookTitle = br.Book != null ? br.Book.Title : null,
                    UserId = br.UserId,
                    Username = br.User != null ? br.User.Username : null
                })
                .ToListAsync();

            return Ok(records);
        }

        // GET: api/BorrowingRecords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BorrowingRecordDto>> GetBorrowingRecord(int id)
        {
            if (_context.BorrowingRecords == null)
            {
                return NotFound("Entity set 'LibraryDbContext.BorrowingRecords' is null.");
            }

            var record = await _context.BorrowingRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Where(br => br.Id == id)
                .Select(br => new BorrowingRecordDto
                {
                    Id = br.Id,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    BookId = br.BookId,
                    BookTitle = br.Book != null ? br.Book.Title : null,
                    UserId = br.UserId,
                    Username = br.User != null ? br.User.Username : null
                })
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return NotFound($"Borrowing record with ID {id} not found.");
            }

            return Ok(record);
        }

        // POST: api/BorrowingRecords (Видача книги)
        [HttpPost]
        public async Task<ActionResult<BorrowingRecordDto>> PostBorrowingRecord(BorrowingRecordCreateDto recordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Перевірка існування книги та користувача
            var bookExists = await _context.Books.AnyAsync(b => b.Id == recordDto.BookId);
            if (!bookExists)
            {
                ModelState.AddModelError(nameof(recordDto.BookId), $"Book with ID {recordDto.BookId} not found.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == recordDto.UserId);
            if (!userExists)
            {
                ModelState.AddModelError(nameof(recordDto.UserId), $"User with ID {recordDto.UserId} not found.");
            }

            if (recordDto.DueDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(recordDto.DueDate), "Due date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Перевірка, чи книга вже не видана і не повернута
            // Це спрощена перевірка. В реальній системі може бути потрібна інвентаризація копій книг.
            var isBookCurrentlyBorrowed = await _context.BorrowingRecords
                .AnyAsync(br => br.BookId == recordDto.BookId && br.ReturnDate == null);

            if (isBookCurrentlyBorrowed)
            {
                return Conflict($"Book with ID {recordDto.BookId} is currently borrowed and not returned.");
            }

            var borrowingRecord = new BorrowingRecord
            {
                BookId = recordDto.BookId,
                UserId = recordDto.UserId,
                BorrowDate = DateTime.UtcNow,
                DueDate = recordDto.DueDate,
                ReturnDate = null // Книгу щойно взяли
            };

            _context.BorrowingRecords.Add(borrowingRecord);
            await _context.SaveChangesAsync();

            // Створюємо DTO для відповіді
            var createdRecordDto = await _context.BorrowingRecords
                .Where(br => br.Id == borrowingRecord.Id)
                .Include(br => br.Book)
                .Include(br => br.User)
                .Select(br => new BorrowingRecordDto
                {
                    Id = br.Id,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    BookId = br.BookId,
                    BookTitle = br.Book != null ? br.Book.Title : null,
                    UserId = br.UserId,
                    Username = br.User != null ? br.User.Username : null
                })
                .FirstAsync(); // FirstAsync, бо ми точно знаємо, що він існує

            return CreatedAtAction(nameof(GetBorrowingRecord), new { id = borrowingRecord.Id }, createdRecordDto);
        }

        // PUT: api/BorrowingRecords/5 (Оновлення запису - наприклад, повернення книги або зміна терміну)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBorrowingRecord(int id, BorrowingRecordUpdateDto updateDto)
        {
            if (!ModelState.IsValid) // Хоча в цьому DTO зараз немає атрибутів валідації
            {
                return BadRequest(ModelState);
            }

            var recordToUpdate = await _context.BorrowingRecords.FindAsync(id);

            if (recordToUpdate == null)
            {
                return NotFound($"Borrowing record with ID {id} not found.");
            }

            // Логіка оновлення:
            if (updateDto.DueDate.HasValue)
            {
                if (updateDto.DueDate.Value.Date < recordToUpdate.BorrowDate.Date)
                {
                    return BadRequest("Due date cannot be earlier than the borrow date.");
                }
                if (recordToUpdate.ReturnDate.HasValue && updateDto.DueDate.Value.Date > recordToUpdate.ReturnDate.Value.Date)
                {
                    return BadRequest("Due date cannot be later than the return date if the book is already returned.");
                }
                recordToUpdate.DueDate = updateDto.DueDate.Value;
            }

            if (updateDto.ReturnDate.HasValue)
            {
                if (recordToUpdate.ReturnDate.HasValue) // Якщо вже повернуто, не можна змінювати дату повернення через цей метод (або потрібна інша логіка)
                {
                    return BadRequest("Book has already been returned. Return date cannot be changed further via this method.");
                }
                if (updateDto.ReturnDate.Value.Date < recordToUpdate.BorrowDate.Date)
                {
                    return BadRequest("Return date cannot be earlier than the borrow date.");
                }
                recordToUpdate.ReturnDate = updateDto.ReturnDate.Value;
            }
            else if (updateDto.ReturnDate == null && recordToUpdate.ReturnDate.HasValue)
            {
                // Дозволити "відміну" повернення, якщо ReturnDate передано як null і книга була повернута
                // Це опціональна логіка, може бути не потрібна.
                // recordToUpdate.ReturnDate = null;
            }


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowingRecordExists(id))
                {
                    return NotFound($"Borrowing record with ID {id} not found during concurrency check.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/BorrowingRecords/5
        // Зазвичай записи про видачу не видаляють для історії. 
        // Цей ендпоінт може бути обмежений для адміністраторів або взагалі відсутній.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrowingRecord(int id)
        {
            var record = await _context.BorrowingRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound($"Borrowing record with ID {id} not found.");
            }

            // Тут можна додати перевірки, чи можна видаляти цей запис.
            // Наприклад, якщо книга ще не повернута, видалення може бути небажаним.
            if (record.ReturnDate == null)
            {
                // return BadRequest("Cannot delete an active borrowing record where the book has not been returned.");
            }

            _context.BorrowingRecords.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BorrowingRecordExists(int id)
        {
            return (_context.BorrowingRecords?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}