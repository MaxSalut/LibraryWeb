using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Не забудь додати цей using

public class Author
{
    public int Id { get; set; } // Первинний ключ

    [Required] // Атрибут валідації - поле обов'язкове
    [StringLength(100)] // Максимальна довжина рядка
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; }

    public string? Biography { get; set; } // "?" означає, що поле може бути null (необов'язкове)

    // Навігаційна властивість: один автор може мати багато книг
    public virtual ICollection<Book>? Books { get; set; }
}