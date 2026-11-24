import { Injectable, signal } from '@angular/core';
import { Genre } from '@domain/models';

/**
 * The single source of truth for the selected genre, shared by sibling components:
 * the Header writes it, the book list (rendered by the Body) reads it.
 *
 * A root-provided signal store keeps Header and Body fully decoupled — neither knows the other
 * exists — without routing state through the root component or a URL. Signals give synchronous
 * reads in templates and automatic dependency tracking for derived state.
 */
@Injectable({ providedIn: 'root' })
export class GenreFilterStore {
  private readonly selectedGenre = signal<Genre | null>(null);

  /** `null` means "All genres". */
  readonly selected = this.selectedGenre.asReadonly();

  select(genre: Genre | null): void {
    this.selectedGenre.set(genre);
  }
}
