using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        public string Email { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        // Пароль тут не обробляємо, це для окремої системи автентифікації (наприклад, ASP.NET Core Identity)
        // Якщо б ми його тут додавали, то була б властивість string Password
    }
}