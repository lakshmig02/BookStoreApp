import { TestBed } from '@angular/core/testing';
import { CatalogSearchResult, DataError } from '@domain/models';
import { CatalogRepository } from '@domain/repositories';
import { CatalogSearchStore } from './catalog-search.store';

describe('CatalogSearchStore', () => {
  let search: ReturnType<typeof vi.fn<(term: string) => Promise<CatalogSearchResult>>>;
  let store: CatalogSearchStore;

  beforeEach(() => {
    search = vi.fn();
    TestBed.configureTestingModule({
      providers: [CatalogSearchStore, { provide: CatalogRepository, useValue: { search } }],
    });
    store = TestBed.inject(CatalogSearchStore);
  });

  it('distinguishes "no results" from "search failed"', async () => {
    search.mockResolvedValueOnce({ items: [], totalCount: 0 });
    await store.search('nothing here');
    expect(store.noResults()).toBe(true);

    search.mockRejectedValueOnce(new DataError('unavailable', 'Open Library is down'));
    await store.search('dune');
    expect(store.status()).toBe('error');
    expect(store.error()?.kind).toBe('unavailable');
    expect(store.noResults()).toBe(false);
  });

  it('ignores terms shorter than two characters', async () => {
    await store.search(' a ');
    expect(search).not.toHaveBeenCalled();
    expect(store.status()).toBe('idle');
  });

  it('aborts the previous in-flight search when a new one starts', async () => {
    const signals: AbortSignal[] = [];
    search.mockImplementation((_term: string, _size?: number, signal?: AbortSignal) => {
      signals.push(signal!);
      return new Promise(() => {});
    });

    void store.search('first');
    void store.search('second');

    expect(signals[0].aborted).toBe(true);
    expect(signals[1].aborted).toBe(false);
  });
});
