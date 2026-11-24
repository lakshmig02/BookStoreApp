import { TestBed } from '@angular/core/testing';
import { Book, BookQuery, DataError, Page } from '@domain/models';
import { BookRepository } from '@domain/repositories';
import { BookListStore } from './book-list.store';
import { GenreFilterStore } from './genre-filter.store';

function book(title: string): Book {
  return {
    id: title,
    title,
    author: 'Author',
    isbn: null,
    publishedYear: 2000,
    coverUrl: null,
    openLibraryKey: `/works/${title}`,
    description: null,
    genre: 'Fiction',
    price: 10,
    stock: 1,
    createdAt: new Date(),
    updatedAt: new Date(),
  };
}

/** In-memory fake of the port: the store never knows HTTP exists. */
class FakeBookRepository implements Partial<BookRepository> {
  queries: BookQuery[] = [];
  next: () => Promise<Page<Book>> = async () => ({ items: [book('Dune')], page: 1, pageSize: 10, totalCount: 1 });

  list(query: BookQuery): Promise<Page<Book>> {
    this.queries.push(query);
    return this.next();
  }
}

describe('BookListStore', () => {
  let repository: FakeBookRepository;
  let store: BookListStore;
  let filter: GenreFilterStore;

  beforeEach(() => {
    repository = new FakeBookRepository();
    TestBed.configureTestingModule({ providers: [{ provide: BookRepository, useValue: repository }] });
    store = TestBed.inject(BookListStore);
    filter = TestBed.inject(GenreFilterStore);
  });

  async function settle(): Promise<void> {
    TestBed.tick();
    await Promise.resolve();
    await Promise.resolve();
  }

  it('loads the first page sorted by title on start', async () => {
    await settle();

    expect(repository.queries).toEqual([{ genre: null, page: 1, pageSize: 10, sort: { field: 'title', direction: 'asc' } }]);
    expect(store.books().map((b) => b.title)).toEqual(['Dune']);
    expect(store.status()).toBe('loaded');
  });

  it('re-queries with the genre selected elsewhere (header) and resets to page 1', async () => {
    await settle();
    store.changePage(3, 10);
    await settle();

    filter.select('SciFi');
    await settle();

    expect(repository.queries.at(-1)).toMatchObject({ genre: 'SciFi', page: 1 });
  });

  it('does not re-query when the same sort is re-applied', async () => {
    await settle();
    store.changeSort({ field: 'title', direction: 'asc' });
    await settle();

    expect(repository.queries).toHaveLength(1);
  });

  it('exposes repository failures as an error state', async () => {
    repository.next = () => Promise.reject(new DataError('network', 'API down'));

    await settle();

    expect(store.status()).toBe('error');
    expect(store.error()?.kind).toBe('network');
  });

  it('ignores a stale response that arrives after a newer one', async () => {
    let resolveSlow!: (page: Page<Book>) => void;
    repository.next = () => new Promise((resolve) => (resolveSlow = resolve));
    await settle();
    const slow = resolveSlow;

    repository.next = async () => ({ items: [book('Fresh')], page: 1, pageSize: 10, totalCount: 1 });
    filter.select('History');
    await settle();
    slow({ items: [book('Stale')], page: 1, pageSize: 10, totalCount: 1 });
    await settle();

    expect(store.books().map((b) => b.title)).toEqual(['Fresh']);
  });
});
