import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { CatalogSearchResult } from '@domain/models';
import { CatalogRepository } from '@domain/repositories';
import { firstValueFrom, fromEvent, takeUntil } from 'rxjs';
import { API_BASE_URL } from './api-base-url';
import { CatalogSearchResultDto } from './api.dtos';
import { toCatalogBook } from './api.mappers';
import { toDataError } from './http-error.mapper';

/** Searches Open Library *through our API* — the browser never calls Open Library directly. */
@Injectable()
export class HttpCatalogRepository implements CatalogRepository {
  private readonly http = inject(HttpClient);
  private readonly searchUrl = `${inject(API_BASE_URL)}/api/catalog/search`;

  async search(term: string, pageSize: number, signal?: AbortSignal): Promise<CatalogSearchResult> {
    let request = this.http.get<CatalogSearchResultDto>(this.searchUrl, { params: { q: term, pageSize } });
    if (signal) request = request.pipe(takeUntil(fromEvent(signal, 'abort')));

    try {
      const dto = await firstValueFrom(request, { defaultValue: null });
      if (!dto) throw new DOMException('Search aborted', 'AbortError');
      return { items: dto.items.map(toCatalogBook), totalCount: dto.totalCount };
    } catch (error) {
      throw error instanceof DOMException ? error : toDataError(error);
    }
  }
}
