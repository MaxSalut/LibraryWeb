using Microsoft.EntityFrameworkCore;
namespace LibraryWeb.Data;
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Genre> Genres { get; set; } // Додано
    public DbSet<Publisher> Publishers { get; set; } // Додано
    public DbSet<User> Users { get; set; } // Додано
    public DbSet<BorrowingRecord> BorrowingRecords { get; set; } // Додано
    public DbSet<Review> Reviews { get; set; } // Додано
    public DbSet<BookGenre> BookGenres { get; set; } // Додано для зв'язку багато-до-багатьох

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Налаштування композитного ключа для BookGenre
        modelBuilder.Entity<BookGenre>()
            .HasKey(bg => new { bg.BookId, bg.GenreId });

        // Налаштування зв'язку багато-до-багатьох: Book до Genre через BookGenre
        modelBuilder.Entity<BookGenre>()
            .HasOne(bg => bg.Book)
            .WithMany(b => b.BookGenres)
            .HasForeignKey(bg => bg.BookId);

        modelBuilder.Entity<BookGenre>()
            .HasOne(bg => bg.Genre)
            .WithMany(g => g.BookGenres)
            .HasForeignKey(bg => bg.GenreId);

        // Налаштування зв'язку один-до-багатьох: Publisher до Book
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.SetNull); // Приклад: якщо видаляємо видавництво, PublisherId у книгах стане null

        // Налаштування зв'язку один-до-багатьох: Author до Book (вже мало б бути, але перевір)
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Cascade); // Приклад: якщо видаляємо автора, видаляються і його книги (обережно з цим)

        // Зв'язки для User, BorrowingRecord, Review
        // User -> BorrowingRecords
        modelBuilder.Entity<User>()
            .HasMany(u => u.BorrowingRecords)
            .WithOne(br => br.User)
            .HasForeignKey(br => br.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Якщо видаляємо користувача, видаляємо його записи про видачу

        // Book -> BorrowingRecords
        modelBuilder.Entity<Book>()
            .HasMany(b => b.BorrowingRecords)
            .WithOne(br => br.Book)
            .HasForeignKey(br => br.BookId)
            .OnDelete(DeleteBehavior.Restrict); // Заборонити видалення книги, якщо на неї є записи про видачу (або Cascade, якщо потрібно)

        // User -> Reviews
        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Якщо видаляємо користувача, видаляємо його відгуки

        // Book -> Reviews
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Reviews)
            .WithOne(r => r.Book)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade); // Якщо видаляємо книгу, видаляємо її відгуки

        // Тут можна додати початкові дані (seeding) для жанрів, якщо потрібно
        // modelBuilder.Entity<Genre>().HasData(
        //     new Genre { Id = 1, Name = "Фантастика" },
        //     new Genre { Id = 2, Name = "Детектив" }
        // );
    }
}