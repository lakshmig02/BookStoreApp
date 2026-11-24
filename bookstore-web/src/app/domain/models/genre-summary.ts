import { Genre } from './genre';

export interface GenreCount {
  readonly genre: Genre;
  readonly bookCount: number;
  readonly totalStock: number;
}

export interface GenreSummary {
  readonly totalBooks: number;
  readonly genres: readonly GenreCount[];
}
