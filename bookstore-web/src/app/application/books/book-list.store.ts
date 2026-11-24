import { computed, effect, inject, Injectable, linkedSignal, signal, untracked } from '@angular/core';
import { Book, BookQuery, BookSort, DataError } from '@domain/models';
import { BookRepository } from '@domain/repositories';
import { GenreFilterStore } from './genre-filter.store';

export type LoadStatus = 'loading' | 'loaded' | 'error';

export const DEFAULT_PAGE_SIZE = 10;
const DEFAULT_SORT: BookSort = { field: 'title', direction: 'asc' };

/**
 * Use case: list and filter the store's books (server-side paging, sorting and genre filter).
 * The query is derived from signals, and any change re-queries the repository.
 */
@Injectable({ providedIn: 'root' })
export class BookListStore {
  private readonly repository = inject(BookRepository);
  private readonly genreFilter = inject(GenreFilterStore);

  /** Changing the genre resets to the first page. */
  private readonly page = linkedSignal({ source: this.genreFilter.selected, computation: () => 1 });
  private readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  private readonly sortState = signal<BookSort>(DEFAULT_SORT, {
    // The table re-emits the current sort as a new object; only a real change should re-query.
    equal: (a, b) => a.field === b.field && a.direction === b.direction,
  });

  private readonly booksState = signal<Book[]>([]);
  private readonly totalCountState = signal(0);
  private readonly statusState = signal<LoadStatus>('loading');
  private readonly errorState = signal<DataError | null>(null);
  private latestRequest = 0;

  readonly query = computed<BookQuery>(() => ({
    genre: this.genreFilter.selected(),
    page: this.page(),
    pageSize: this.pageSize(),
    sort: this.sortState(),
  }));

  readonly books = this.booksState.asReadonly();
  readonly totalCount = this.totalCountState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly isEmpty = computed(() => this.status() === 'loaded' && this.totalCount() === 0);

  constructor() {
    effect(() => {
      const query = this.query();
      untracked(() => void this.load(query));
    });
  }

  changePage(page: number, pageSize: number): void {
    this.pageSize.set(pageSize);
    this.page.set(page);
  }

  changeSort(sort: BookSort): void {
    this.sortState.set(sort);
  }

  reload(): Promise<void> {
    return this.load(this.query());
  }

  private async load(query: BookQuery): Promise<void> {
    // Only the most recent request may update state, so a slow response can't overwrite a newer one.
    const request = ++this.latestRequest;
    this.statusState.set('loading');
    this.errorState.set(null);

    try {
      const result = await this.repository.list(query);
      if (request !== this.latestRequest) return;

      // Deleting the last book on a page leaves it empty: step back instead of showing nothing.
      if (result.items.length === 0 && result.totalCount > 0 && query.page > 1) {
        this.page.set(Math.ceil(result.totalCount / query.pageSize));
        return;
      }

      this.booksState.set([...result.items]);
      this.totalCountState.set(result.totalCount);
      this.statusState.set('loaded');
    } catch (error) {
      if (request !== this.latestRequest) return;
      this.errorState.set(DataError.from(error));
      this.statusState.set('error');
    }
  }
}
