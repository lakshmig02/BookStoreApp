import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, output } from '@angular/core';
import { BookListStore } from '@application/books/book-list.store';
import { GenreFilterStore } from '@application/books/genre-filter.store';
import { Book, GENRE_LABELS, isBookSortField } from '@domain/models';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { BookCover } from '../../shared/book-cover';
import { describeError } from '../../shared/data-error-message';
import { GenreLabelPipe } from '../../shared/genre-label.pipe';

const LOW_STOCK = 3;

/** The store's books with server-side paging and sorting. The genre filter arrives via the store. */
@Component({
  selector: 'app-book-grid',
  imports: [TableModule, ButtonModule, TagModule, MessageModule, TooltipModule, CurrencyPipe, GenreLabelPipe, BookCover],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './book-grid.html',
})
export class BookGrid {
  protected readonly store = inject(BookListStore);
  private readonly genreFilter = inject(GenreFilterStore);

  readonly edit = output<Book>();
  readonly delete = output<Book>();
  readonly add = output<void>();

  protected readonly rowsPerPageOptions = [5, 10, 25, 50];
  protected readonly first = computed(() => (this.store.query().page - 1) * this.store.query().pageSize);
  protected readonly sortOrder = computed(() => (this.store.query().sort.direction === 'asc' ? 1 : -1));
  protected readonly errorMessage = computed(() => {
    const error = this.store.error();
    return error ? describeError(error) : null;
  });
  protected readonly emptyMessage = computed(() => {
    const genre = this.genreFilter.selected();
    return genre ? `No ${GENRE_LABELS[genre]} books in the store yet.` : 'The store has no books yet.';
  });

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.store.query().pageSize;
    this.store.changePage(Math.floor((event.first ?? 0) / rows) + 1, rows);

    const field = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    if (isBookSortField(field)) {
      this.store.changeSort({ field, direction: event.sortOrder === -1 ? 'desc' : 'asc' });
    }
  }

  protected stockSeverity(stock: number): 'danger' | 'warn' | 'success' {
    return stock === 0 ? 'danger' : stock <= LOW_STOCK ? 'warn' : 'success';
  }
}
