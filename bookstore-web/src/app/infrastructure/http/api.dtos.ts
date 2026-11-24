/** Wire contracts of the Book Store API. Only the infrastructure layer knows these shapes. */

export interface BookDto {
  id: string;
  title: string;
  author: string;
  isbn: string | null;
  publishedYear: number | null;
  coverUrl: string | null;
  openLibraryKey: string;
  description: string | null;
  genre: string;
  price: number;
  stock: number;
  createdAt: string;
  updatedAt: string;
}

export interface PagedResultDto<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface AddBookRequestDto {
  openLibraryKey: string;
  title: string;
  author: string;
  isbn: string | null;
  publishedYear: number | null;
  coverUrl: string | null;
  description: string | null;
  genre: string;
  price: number;
  stock: number;
}

export interface UpdateBookRequestDto {
  genre: string;
  price: number;
  stock: number;
  description: string | null;
}

export interface CatalogBookDto {
  key: string;
  title: string;
  author: string;
  isbn: string | null;
  publishedYear: number | null;
  coverUrl: string | null;
  alreadyInStore: boolean;
}

export interface CatalogSearchResultDto {
  items: CatalogBookDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface GenreSummaryDto {
  totalBooks: number;
  genres: { genre: string; bookCount: number; totalStock: number }[];
}

/** RFC 9457 problem details as produced by the API. */
export interface ProblemDetailsDto {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  correlationId?: string;
  errors?: Record<string, string[]>;
}
