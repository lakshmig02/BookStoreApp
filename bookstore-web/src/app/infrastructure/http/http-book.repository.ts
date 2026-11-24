import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Book, BookQuery, GenreSummary, Page, StoreDetails } from '@domain/models';
import { AddBookInput, BookRepository } from '@domain/repositories';
import { firstValueFrom, Observable } from 'rxjs';
import { API_BASE_URL } from './api-base-url';
import { BookDto, GenreSummaryDto, PagedResultDto } from './api.dtos';
import { toAddBookRequest, toBook, toGenreSummary, toSortParams, toUpdateBookRequest } from './api.mappers';
import { toDataError } from './http-error.mapper';

@Injectable()
export class HttpBookRepository implements BookRepository {
  private readonly http = inject(HttpClient);
  private readonly booksUrl = `${inject(API_BASE_URL)}/api/books`;
  private readonly genresUrl = `${inject(API_BASE_URL)}/api/genres`;

  async list(query: BookQuery): Promise<Page<Book>> {
    let params = new HttpParams({
      fromObject: {
        page: query.page,
        pageSize: query.pageSize,
        ...toSortParams(query.sort.field, query.sort.direction),
      },
    });
    if (query.genre) params = params.set('genre', query.genre);

    const dto = await send(this.http.get<PagedResultDto<BookDto>>(this.booksUrl, { params }));
    return { ...dto, items: dto.items.map(toBook) };
  }

  async add(input: AddBookInput): Promise<Book> {
    return toBook(await send(this.http.post<BookDto>(this.booksUrl, toAddBookRequest(input))));
  }

  async update(id: string, details: StoreDetails): Promise<Book> {
    return toBook(await send(this.http.put<BookDto>(`${this.booksUrl}/${encodeURIComponent(id)}`, toUpdateBookRequest(details))));
  }

  async delete(id: string): Promise<void> {
    await send(this.http.delete<void>(`${this.booksUrl}/${encodeURIComponent(id)}`));
  }

  async getGenreSummary(): Promise<GenreSummary> {
    return toGenreSummary(await send(this.http.get<GenreSummaryDto>(this.genresUrl)));
  }
}

async function send<T>(request: Observable<T>): Promise<T> {
  try {
    return await firstValueFrom(request);
  } catch (error) {
    throw toDataError(error);
  }
}
