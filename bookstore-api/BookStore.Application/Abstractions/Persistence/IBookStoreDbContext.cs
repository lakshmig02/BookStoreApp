using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Abstractions.Persistence;

/// <summary>
/// The store's unit of work as seen by the use cases. Handlers compose LINQ directly against
/// <see cref="Books"/> (no generic repository on top of EF, which is already one); the SQLite
/// provider and mappings stay in Infrastructure.
/// </summary>
public interface IBookStoreDbContext
{
    DbSet<Book> Books { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
