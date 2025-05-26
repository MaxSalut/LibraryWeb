using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } // Поки що без пароля, потім можна додати ASP.NET Identity

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    // Навігаційні властивості
    public virtual ICollection<BorrowingRecord>? BorrowingRecords { get; set; } = new List<BorrowingRecord>();
    public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();
}