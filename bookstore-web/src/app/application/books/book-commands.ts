import { inject, Injectable } from '@angular/core';
import { Book, StoreDetails } from '@domain/models';
import { AddBookInput, BookRepository } from '@domain/repositories';
import { GenreSummaryStore } from '../genres/genre-summary.store';
import { BookListStore } from './book-list.store';

/**
 * Use cases that change the store: add a selected catalog book, edit store-owned fields, delete.
 * After each change the list and genre counts are refreshed so every view stays consistent.
 * Failures surface as `DataError` for the UI to present.
 */
@Injectable({ providedIn: 'root' })
export class BookCommands {
  private readonly repository = inject(BookRepository);
  private readonly bookList = inject(BookListStore);
  private readonly genreSummary = inject(GenreSummaryStore);

  async add(input: AddBookInput): Promise<Book> {
    const book = await this.repository.add(input);
    this.refreshViews();
    return book;
  }

  async update(id: string, details: StoreDetails): Promise<Book> {
    const book = await this.repository.update(id, details);
    this.refreshViews();
    return book;
  }

  async delete(id: string): Promise<void> {
    await this.repository.delete(id);
    this.refreshViews();
  }

  private refreshViews(): void {
    void this.bookList.reload();
    void this.genreSummary.reload();
  }
}
