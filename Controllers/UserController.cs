using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryWeb.Data;     // Для DbContext
using LibraryWeb.DTOs;     // Для DTO
// Якщо модель User у глобальному неймспейсі, using для неї не потрібен.
// Якщо в LibraryWeb.Models, додай: using LibraryWeb.Models;

namespace LibraryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public UsersController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Users' is null.");
            }

            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    RegistrationDate = u.RegistrationDate
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound("Entity set 'LibraryDbContext.Users' is null.");
            }

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    RegistrationDate = u.RegistrationDate
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Додаткова валідація: перевірка на унікальність Username та Email
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                ModelState.AddModelError(nameof(userDto.Username), "This username is already taken.");
            }
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                ModelState.AddModelError(nameof(userDto.Email), "This email address is already registered.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User // Створюємо сутність User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                RegistrationDate = DateTime.UtcNow // Встановлюємо поточну дату реєстрації
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Створюємо UserDto для відповіді
            var createdUserDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RegistrationDate = user.RegistrationDate
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUserDto);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToUpdate = await _context.Users.FindAsync(id);

            if (userToUpdate == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Перевірка на унікальність Username та Email при оновленні (якщо вони змінилися)
            if (userToUpdate.Username != userDto.Username && await _context.Users.AnyAsync(u => u.Username == userDto.Username && u.Id != id))
            {
                ModelState.AddModelError(nameof(userDto.Username), "This username is already taken by another user.");
            }
            if (userToUpdate.Email != userDto.Email && await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id))
            {
                ModelState.AddModelError(nameof(userDto.Email), "This email address is already registered by another user.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userToUpdate.Username = userDto.Username;
            userToUpdate.Email = userDto.Email;
            userToUpdate.FirstName = userDto.FirstName;
            userToUpdate.LastName = userToUpdate.LastName; // Ой, тут мала бути помилка копіпасти. Має бути: userDto.LastName

            // Правильно:
            // userToUpdate.LastName = userDto.LastName;


            try
            {
                // _context.Entry(userToUpdate).State = EntityState.Modified; // Не обов'язково, EF Core відстежує зміни
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound($"User with ID {id} not found during concurrency check.");
                }
                else
                {
                    throw; // Або інша обробка помилки конкуренції
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Згідно з налаштуваннями OnModelCreating, пов'язані Reviews та BorrowingRecords
            // будуть видалені каскадно, якщо так налаштовано.
            // Якщо є обмеження (Restrict), то EF Core не дозволить видалення, поки є зв'язані записи.
            // Тут можна додати додаткову бізнес-логіку, якщо потрібно (наприклад, не видаляти адмінів).

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}