import { ChangeDetectionStrategy, Component, computed, inject, model, output, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BookCommands } from '@application/books/book-commands';
import { CatalogSearchStore } from '@application/catalog/catalog-search.store';
import { Book, CatalogBook, DataError } from '@domain/models';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { ListboxModule } from 'primeng/listbox';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { BookCover } from '../../shared/book-cover';
import { describeError } from '../../shared/data-error-message';
import { StoreDetailsFields } from '../store-details-fields/store-details-fields';
import { applyServerErrors, createStoreDetailsForm, readStoreDetails } from '../store-details-fields/store-details-form';

type Step = 'search' | 'details';

/**
 * Two-step add flow: (1) search Open Library through the API and pick a result,
 * (2) supply the store-owned fields and save. Only the picked book is persisted.
 */
@Component({
  selector: 'app-add-book-dialog',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    ListboxModule,
    MessageModule,
    SkeletonModule,
    TagModule,
    BookCover,
    StoreDetailsFields,
  ],
  providers: [CatalogSearchStore],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './add-book-dialog.html',
})
export class AddBookDialog {
  protected readonly search = inject(CatalogSearchStore);
  private readonly commands = inject(BookCommands);

  readonly visible = model(false);
  readonly added = output<Book>();

  protected readonly step = signal<Step>('search');
  protected readonly term = signal('');
  protected readonly selected = signal<CatalogBook | null>(null);
  protected readonly saving = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected form = createStoreDetailsForm();

  protected readonly searchErrorMessage = computed(() => {
    const error = this.search.error();
    return error ? describeError(error) : null;
  });
  protected readonly skeletonRows = [1, 2, 3, 4];

  protected runSearch(): void {
    void this.search.search(this.term());
  }

  protected choose(book: CatalogBook | null): void {
    if (!book || book.alreadyInStore) return;
    this.selected.set(book);
    this.form = createStoreDetailsForm();
    this.saveError.set(null);
    this.step.set('details');
  }

  protected backToResults(): void {
    this.step.set('search');
    this.selected.set(null);
  }

  protected async save(): Promise<void> {
    const book = this.selected();
    if (!book) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    try {
      const created = await this.commands.add({
        openLibraryKey: book.key,
        title: book.title,
        author: book.author,
        isbn: book.isbn,
        publishedYear: book.publishedYear,
        coverUrl: book.coverUrl,
        details: readStoreDetails(this.form),
      });
      this.added.emit(created);
      this.close();
    } catch (error) {
      const dataError = DataError.from(error);
      if (!(dataError.kind === 'validation' && applyServerErrors(this.form, dataError))) {
        this.saveError.set(describeError(dataError));
      }
    } finally {
      this.saving.set(false);
    }
  }

  protected close(): void {
    this.visible.set(false);
  }

  /** Every opening starts fresh: search results are transient by design. */
  protected onHide(): void {
    this.search.reset();
    this.term.set('');
    this.selected.set(null);
    this.saveError.set(null);
    this.step.set('search');
  }
}
