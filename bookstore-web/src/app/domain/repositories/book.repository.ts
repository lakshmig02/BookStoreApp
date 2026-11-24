import { Book, BookQuery, GenreSummary, Page, StoreDetails } from '../models';

/** Everything needed to add a selected catalog result to the store. */
export interface AddBookInput {
  readonly openLibraryKey: string;
  readonly title: string;
  readonly author: string;
  readonly isbn: string | null;
  readonly publishedYear: number | null;
  readonly coverUrl: string | null;
  readonly details: StoreDetails;
}

/**
 * Port for the store's own books. Declared as an abstract class rather than an interface so it
 * doubles as a DI token at runtime without importing anything from Angular.
 */
export abstract class BookRepository {
  abstract list(query: BookQuery): Promise<Page<Book>>;
  abstract add(input: AddBookInput): Promise<Book>;
  abstract update(id: string, details: StoreDetails): Promise<Book>;
  abstract delete(id: string): Promise<void>;
  abstract getGenreSummary(): Promise<GenreSummary>;
}
