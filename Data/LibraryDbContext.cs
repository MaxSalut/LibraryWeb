using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Data 
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; } // Проміжна таблиця для зв'язку Book-Genre

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Зв'язок Багато-до-Багатьох: Book та Genre через BookGenre ---
            modelBuilder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId }); // Композитний ключ

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється книга, видаляються її зв'язки з жанрами

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється жанр, видаляються його зв'язки з книгами

            // --- Зв'язок Один-до-Багатьох: Author та Book ---
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється автор, видаляються і його книги (обережно з цим правилом)

            // --- Зв'язок Один-до-Багатьох: Publisher та Book ---
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany(p => p.Books)
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.SetNull); // Якщо видаляється видавництво, PublisherId у книгах стає null (якщо PublisherId nullable)

            // --- Зв'язки для User ---
            // User -> BorrowingRecords (Один User - багато BorrowingRecord)
            modelBuilder.Entity<User>()
                .HasMany(u => u.BorrowingRecords)
                .WithOne(br => br.User)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється користувач, видаляються його записи про видачу

            // User -> Reviews (Один User - багато Review)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється користувач, видаляються його відгуки

            // --- Зв'язки для Book (продовження) ---
            // Book -> BorrowingRecords (Одна Book - багато BorrowingRecord)
            modelBuilder.Entity<Book>()
                .HasMany(b => b.BorrowingRecords)
                .WithOne(br => br.Book)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict); // Заборонити видалення книги, якщо на неї є активні записи про видачу

            // Book -> Reviews (Одна Book - багато Review)
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Reviews)
                .WithOne(r => r.Book)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}