namespace BookStore.Domain.Books;

/// <summary>
/// Controlled vocabulary for the store's genre filter. Open Library subjects are too
/// free-form to map reliably, so genre is always chosen by the store at add-time.
/// </summary>
public enum Genre
{
    Fiction,
    SciFi,
    Fantasy,
    Mystery,
    Biography,
    NonFiction,
    Children,
    History,
}
