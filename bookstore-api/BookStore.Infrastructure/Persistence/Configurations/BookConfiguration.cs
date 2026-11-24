using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.Infrastructure.Persistence.Configurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.Title).IsRequired().HasMaxLength(Book.TitleMaxLength);
        builder.Property(b => b.Author).IsRequired().HasMaxLength(Book.AuthorMaxLength);
        builder.Property(b => b.Isbn).HasMaxLength(Book.IsbnMaxLength);
        builder.Property(b => b.CoverUrl).HasMaxLength(Book.CoverUrlMaxLength);
        builder.Property(b => b.OpenLibraryKey).IsRequired().HasMaxLength(Book.OpenLibraryKeyMaxLength);
        builder.Property(b => b.Description).HasMaxLength(Book.DescriptionMaxLength);

        // Stored by name so the column is readable in raw SQL and survives enum reordering.
        builder.Property(b => b.Genre).HasConversion<string>().HasMaxLength(20);

        // SQLite has no decimal type and EF cannot ORDER BY a TEXT-backed decimal, so money is
        // stored exactly as integer cents. The domain already rounds prices to 2 decimals.
        builder.Property(b => b.Price)
            .HasColumnName("PriceCents")
            .HasConversion(price => (long)(price * 100m), cents => cents / 100m);

        // Duplicate prevention: the final guard behind the handler's pre-check.
        builder.HasIndex(b => b.OpenLibraryKey).IsUnique();
        builder.HasIndex(b => b.Isbn).IsUnique().HasFilter("\"Isbn\" IS NOT NULL");
        builder.HasIndex(b => b.Genre);
    }
}
