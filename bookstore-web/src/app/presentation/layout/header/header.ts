import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GenreFilterStore } from '@application/books/genre-filter.store';
import { GenreSummaryStore } from '@application/genres/genre-summary.store';
import { Genre, GENRE_LABELS } from '@domain/models';
import { SelectModule } from 'primeng/select';

const ALL = 'all';
type FilterValue = Genre | typeof ALL;

/** Branding and the genre filter. Writes the selection to GenreFilterStore; knows nothing about the grid. */
@Component({
  selector: 'app-header',
  imports: [FormsModule, SelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './header.html',
  host: { class: 'sticky top-0 z-20 block border-b border-slate-200 bg-white/90 backdrop-blur' },
})
export class Header {
  private readonly filter = inject(GenreFilterStore);
  private readonly summary = inject(GenreSummaryStore);

  protected readonly selected = computed<FilterValue>(() => this.filter.selected() ?? ALL);

  protected readonly options = computed(() => [
    { value: ALL as FilterValue, label: 'All genres', count: this.summary.totalBooks() },
    ...this.summary.genres().map(({ genre, count }) => ({ value: genre as FilterValue, label: GENRE_LABELS[genre], count })),
  ]);

  protected select(value: FilterValue): void {
    this.filter.select(value === ALL ? null : value);
  }
}
