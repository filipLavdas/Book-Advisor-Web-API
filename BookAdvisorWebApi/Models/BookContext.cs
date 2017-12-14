namespace BookAdvisorWebApi.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class BookContext : DbContext
    {
        public BookContext()
            : base("name=BookContext")
        {
           this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<Author> Author { get; set; }
        public virtual DbSet<Book> Book { get; set; }
        public virtual DbSet<BookUserLike> BookUserLike { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<IndustryIdentifier> IndustryIdentifier { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<Publisher> Publisher { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>()
                .HasMany(e => e.Book)
                .WithMany(e => e.Author)
                .Map(m => m.ToTable("BookAuthor").MapLeftKey("AuthorId").MapRightKey("BookId"));

            modelBuilder.Entity<Book>()
                .Property(e => e.F_Id)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .HasMany(e => e.Category)
                .WithMany(e => e.Book)
                .Map(m => m.ToTable("BookCategory").MapLeftKey("BookId").MapRightKey("CategoryId"));

            modelBuilder.Entity<IndustryIdentifier>()
                .Property(e => e.Identifier)
                .IsUnicode(false);

            modelBuilder.Entity<Profile>()
                .Property(e => e.F_Id)
                .IsUnicode(false);

            modelBuilder.Entity<Profile>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Profile>()
                .Property(e => e.Gender)
                .IsUnicode(false);

            modelBuilder.Entity<Profile>()
                .HasMany(e => e.BookUserLike)
                .WithRequired(e => e.Profile)
                .WillCascadeOnDelete(false);
        }
    }
}
