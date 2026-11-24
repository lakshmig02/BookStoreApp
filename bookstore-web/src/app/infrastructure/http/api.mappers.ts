import { Book, BookSortField, CatalogBook, GenreSummary, isGenre, SortDirection, StoreDetails } from '@domain/models';
import { AddBookInput } from '@domain/repositories';
import { AddBookRequestDto, BookDto, CatalogBookDto, GenreSummaryDto, UpdateBookRequestDto } from './api.dtos';

function toGenre(value: string): Book['genre'] {
  if (!isGenre(value)) throw new Error(`API returned unknown genre '${value}'.`);
  return value;
}

export function toBook(dto: BookDto): Book {
  return {
    ...dto,
    genre: toGenre(dto.genre),
    createdAt: new Date(dto.createdAt),
    updatedAt: new Date(dto.updatedAt),
  };
}

export function toCatalogBook(dto: CatalogBookDto): CatalogBook {
  return { ...dto };
}

export function toGenreSummary(dto: GenreSummaryDto): GenreSummary {
  return {
    totalBooks: dto.totalBooks,
    genres: dto.genres.filter((g) => isGenre(g.genre)).map((g) => ({ ...g, genre: toGenre(g.genre) })),
  };
}

export function toAddBookRequest(input: AddBookInput): AddBookRequestDto {
  const { details, ...catalogFields } = input;
  return { ...catalogFields, ...toUpdateBookRequest(details) };
}

export function toUpdateBookRequest(details: StoreDetails): UpdateBookRequestDto {
  return {
    genre: details.genre,
    price: details.price,
    stock: details.stock,
    description: details.description?.trim() || null,
  };
}

/** The API's enums are PascalCase (`PublishedYear`, `Desc`). */
export function toSortParams(field: BookSortField, direction: SortDirection): { sortBy: string; sortDir: string } {
  const pascal = (value: string) => value.charAt(0).toUpperCase() + value.slice(1);
  return { sortBy: pascal(field), sortDir: pascal(direction) };
}
