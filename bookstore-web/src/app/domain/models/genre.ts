/** Controlled vocabulary shared with the API (`BookStore.Domain.Books.Genre`). */
export const GENRES = [
  'Fiction',
  'SciFi',
  'Fantasy',
  'Mystery',
  'Biography',
  'NonFiction',
  'Children',
  'History',
] as const;

export type Genre = (typeof GENRES)[number];

export const GENRE_LABELS: Readonly<Record<Genre, string>> = {
  Fiction: 'Fiction',
  SciFi: 'Sci-Fi',
  Fantasy: 'Fantasy',
  Mystery: 'Mystery',
  Biography: 'Biography',
  NonFiction: 'Non-fiction',
  Children: 'Children',
  History: 'History',
};

export function isGenre(value: unknown): value is Genre {
  return typeof value === 'string' && (GENRES as readonly string[]).includes(value);
}
