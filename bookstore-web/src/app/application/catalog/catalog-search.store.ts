import { computed, inject, Injectable, signal } from '@angular/core';
import { CatalogBook, DataError } from '@domain/models';
import { CatalogRepository } from '@domain/repositories';

export type SearchStatus = 'idle' | 'searching' | 'done' | 'error';

const PAGE_SIZE = 10;

/**
 * Use case: search the external catalog. Provided per add-book dialog (not root) because the
 * results are transient and belong to a single add flow.
 */
@Injectable()
export class CatalogSearchStore {
  private readonly repository = inject(CatalogRepository);
  private inFlight: AbortController | null = null;

  private readonly termState = signal('');
  private readonly resultsState = signal<CatalogBook[]>([]);
  private readonly totalCountState = signal(0);
  private readonly statusState = signal<SearchStatus>('idle');
  private readonly errorState = signal<DataError | null>(null);

  readonly term = this.termState.asReadonly();
  readonly results = this.resultsState.asReadonly();
  readonly totalCount = this.totalCountState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly noResults = computed(() => this.status() === 'done' && this.results().length === 0);

  async search(rawTerm: string): Promise<void> {
    const term = rawTerm.trim();
    if (term.length < 2) return;

    this.inFlight?.abort();
    const controller = (this.inFlight = new AbortController());

    this.termState.set(term);
    this.statusState.set('searching');
    this.errorState.set(null);

    try {
      const result = await this.repository.search(term, PAGE_SIZE, controller.signal);
      if (controller.signal.aborted) return;
      this.resultsState.set([...result.items]);
      this.totalCountState.set(result.totalCount);
      this.statusState.set('done');
    } catch (error) {
      if (controller.signal.aborted) return;
      this.resultsState.set([]);
      this.errorState.set(DataError.from(error));
      this.statusState.set('error');
    }
  }

  retry(): Promise<void> {
    return this.search(this.termState());
  }

  reset(): void {
    this.inFlight?.abort();
    this.termState.set('');
    this.resultsState.set([]);
    this.totalCountState.set(0);
    this.statusState.set('idle');
    this.errorState.set(null);
  }
}
