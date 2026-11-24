import { Genre } from './genre';

export const BOOK_SORT_FIELDS = ['title', 'author', 'publishedYear', 'genre', 'price', 'stock', 'createdAt'] as const;
export type BookSortField = (typeof BOOK_SORT_FIELDS)[number];
export type SortDirection = 'asc' | 'desc';

export interface BookSort {
  readonly field: BookSortField;
  readonly direction: SortDirection;
}

export interface BookQuery {
  readonly genre: Genre | null;
  /** 1-based page number. */
  readonly page: number;
  readonly pageSize: number;
  readonly sort: BookSort;
}

export interface Page<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}

export function isBookSortField(value: unknown): value is BookSortField {
  return typeof value === 'string' && (BOOK_SORT_FIELDS as readonly string[]).includes(value);
}
