import { Genre } from './genre';

/** A book owned by the store. Bibliographic fields were captured from Open Library once, at add-time. */
export interface Book {
  readonly id: string;
  readonly title: string;
  readonly author: string;
  readonly isbn: string | null;
  readonly publishedYear: number | null;
  readonly coverUrl: string | null;
  readonly openLibraryKey: string;
  readonly description: string | null;
  readonly genre: Genre;
  readonly price: number;
  readonly stock: number;
  readonly createdAt: Date;
  readonly updatedAt: Date;
}

/** The fields the store owns and may edit. Editing never touches Open Library. */
export interface StoreDetails {
  readonly genre: Genre;
  readonly price: number;
  readonly stock: number;
  readonly description: string | null;
}
