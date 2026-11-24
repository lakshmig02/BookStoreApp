import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { GenreSummaryStore } from '@application/genres/genre-summary.store';

@Component({
  selector: 'app-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block border-t border-slate-200 bg-white' },
  template: `
    <div class="mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-2 px-4 py-4 text-sm text-slate-500 sm:px-6">
      <span>
        @if (summary.totalBooks() !== null) {
          {{ summary.totalBooks() }} titles · {{ summary.totalStock() }} copies in stock
        }
      </span>
      <span>
        Catalog data from
        <a href="https://openlibrary.org" target="_blank" rel="noopener" class="font-medium text-indigo-600 hover:underline">
          Open Library
        </a>
      </span>
    </div>
  `,
})
export class Footer {
  protected readonly summary = inject(GenreSummaryStore);
}
