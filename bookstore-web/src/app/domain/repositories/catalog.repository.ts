import { CatalogSearchResult } from '../models';

/** Port for book discovery (Open Library, proxied through our API). Results are transient. */
export abstract class CatalogRepository {
  /** @param signal aborts an in-flight search when a newer one supersedes it. */
  abstract search(term: string, pageSize: number, signal?: AbortSignal): Promise<CatalogSearchResult>;
}
