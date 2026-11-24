/** A transient search hit from the external catalog. Never persisted unless the user adds it. */
export interface CatalogBook {
  readonly key: string;
  readonly title: string;
  readonly author: string;
  readonly isbn: string | null;
  readonly publishedYear: number | null;
  readonly coverUrl: string | null;
  /** True when the store already owns a book with this Open Library key or ISBN. */
  readonly alreadyInStore: boolean;
}

export interface CatalogSearchResult {
  readonly items: readonly CatalogBook[];
  readonly totalCount: number;
}
