import { computed, inject, Injectable, signal } from '@angular/core';
import { GENRES, GenreSummary } from '@domain/models';
import { BookRepository } from '@domain/repositories';

/**
 * Book counts per genre, used to label the header filter. Purely informational: if it fails
 * to load, the filter still works with plain genre names.
 */
@Injectable({ providedIn: 'root' })
export class GenreSummaryStore {
  private readonly repository = inject(BookRepository);
  private readonly summaryState = signal<GenreSummary | null>(null);

  readonly summary = this.summaryState.asReadonly();
  readonly totalBooks = computed(() => this.summaryState()?.totalBooks ?? null);
  readonly totalStock = computed(() => this.summaryState()?.genres.reduce((sum, g) => sum + g.totalStock, 0) ?? null);

  /** Count per genre, in the controlled-vocabulary order. `null` count = unknown. */
  readonly genres = computed(() => {
    const counts = new Map(this.summaryState()?.genres.map((g) => [g.genre, g.bookCount]));
    return GENRES.map((genre) => ({ genre, count: counts.get(genre) ?? null }));
  });

  constructor() {
    void this.reload();
  }

  async reload(): Promise<void> {
    try {
      this.summaryState.set(await this.repository.getGenreSummary());
    } catch {
      // Non-critical: keep the last known counts.
    }
  }
}
