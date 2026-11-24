import { ChangeDetectionStrategy, Component, effect, inject, input, output, signal } from '@angular/core';
import { BookCommands } from '@application/books/book-commands';
import { Book, DataError } from '@domain/models';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { BookCover } from '../../shared/book-cover';
import { describeError } from '../../shared/data-error-message';
import { StoreDetailsFields } from '../store-details-fields/store-details-fields';
import { applyServerErrors, createStoreDetailsForm, readStoreDetails } from '../store-details-fields/store-details-form';

/** Edits the store-owned fields of a book. Open Library fields are read-only here. */
@Component({
  selector: 'app-edit-book-dialog',
  imports: [DialogModule, ButtonModule, MessageModule, BookCover, StoreDetailsFields],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './edit-book-dialog.html',
})
export class EditBookDialog {
  private readonly commands = inject(BookCommands);

  /** The book being edited; `null` closes the dialog. */
  readonly book = input<Book | null>(null);
  readonly closed = output<void>();
  readonly saved = output<Book>();

  protected readonly visible = signal(false);
  protected readonly saving = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected form = createStoreDetailsForm();

  constructor() {
    effect(() => {
      const book = this.book();
      if (book) {
        this.form = createStoreDetailsForm(book);
        this.saveError.set(null);
      }
      this.visible.set(book !== null);
    });
  }

  protected async save(book: Book): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    try {
      this.saved.emit(await this.commands.update(book.id, readStoreDetails(this.form)));
      this.visible.set(false);
    } catch (error) {
      const dataError = DataError.from(error);
      if (!(dataError.kind === 'validation' && applyServerErrors(this.form, dataError))) {
        this.saveError.set(describeError(dataError));
      }
    } finally {
      this.saving.set(false);
    }
  }
}
