import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { BookCommands } from '@application/books/book-commands';
import { BookListStore } from '@application/books/book-list.store';
import { GenreFilterStore } from '@application/books/genre-filter.store';
import { Book, GENRE_LABELS } from '@domain/models';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { AddBookDialog } from '../../books/add-book-dialog/add-book-dialog';
import { BookGrid } from '../../books/book-grid/book-grid';
import { EditBookDialog } from '../../books/edit-book-dialog/edit-book-dialog';
import { describeError } from '../../shared/data-error-message';

/** Main content: the books grid plus the add / edit / delete interactions around it. */
@Component({
  selector: 'app-body',
  imports: [ButtonModule, ToastModule, ConfirmDialogModule, BookGrid, AddBookDialog, EditBookDialog],
  providers: [MessageService, ConfirmationService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './body.html',
  host: { class: 'block flex-1' },
})
export class Body {
  private readonly commands = inject(BookCommands);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly genreFilter = inject(GenreFilterStore);
  protected readonly bookList = inject(BookListStore);

  protected readonly addOpen = signal(false);
  protected readonly editing = signal<Book | null>(null);

  protected readonly heading = computed(() => {
    const genre = this.genreFilter.selected();
    return genre ? GENRE_LABELS[genre] : 'All books';
  });

  protected onAdded(book: Book): void {
    this.messages.add({ severity: 'success', summary: 'Book added', detail: `“${book.title}” is now in the store.` });
  }

  protected onSaved(book: Book): void {
    this.messages.add({ severity: 'success', summary: 'Changes saved', detail: `“${book.title}” was updated.` });
  }

  protected confirmDelete(book: Book): void {
    this.confirmation.confirm({
      header: 'Delete book?',
      message: `Remove “${book.title}” from the store? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', text: true },
      accept: () => void this.delete(book),
    });
  }

  private async delete(book: Book): Promise<void> {
    try {
      await this.commands.delete(book.id);
      this.messages.add({ severity: 'success', summary: 'Book deleted', detail: `“${book.title}” was removed.` });
    } catch (error) {
      this.messages.add({ severity: 'error', summary: 'Delete failed', detail: describeError(error), life: 8000 });
    }
  }
}
