import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DataError } from '@domain/models';
import { BookDto } from './api.dtos';
import { HttpBookRepository } from './http-book.repository';

const dto: BookDto = {
  id: '1',
  title: 'Dune',
  author: 'Frank Herbert',
  isbn: '9780441172719',
  publishedYear: 1965,
  coverUrl: null,
  openLibraryKey: '/works/OL893415W',
  description: null,
  genre: 'SciFi',
  price: 9.99,
  stock: 3,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-02T00:00:00Z',
};

describe('HttpBookRepository', () => {
  let repository: HttpBookRepository;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [HttpBookRepository, provideHttpClient(), provideHttpClientTesting()] });
    repository = TestBed.inject(HttpBookRepository);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('maps the domain query to API params and the DTO to a domain Book', async () => {
    const result = repository.list({ genre: 'SciFi', page: 2, pageSize: 5, sort: { field: 'publishedYear', direction: 'desc' } });

    const request = http.expectOne((r) => r.url === '/api/books');
    expect(request.request.params.toString()).toBe('page=2&pageSize=5&sortBy=PublishedYear&sortDir=Desc&genre=SciFi');
    request.flush({ items: [dto], page: 2, pageSize: 5, totalCount: 6 });

    const page = await result;
    expect(page.totalCount).toBe(6);
    expect(page.items[0].genre).toBe('SciFi');
    expect(page.items[0].createdAt).toEqual(new Date('2026-01-01T00:00:00Z'));
  });

  it('turns validation problem details into a DataError with field errors', async () => {
    const result = repository.update('1', { genre: 'SciFi', price: 0, stock: 1, description: '  ' });

    const request = http.expectOne('/api/books/1');
    expect(request.request.body).toEqual({ genre: 'SciFi', price: 0, stock: 1, description: null });
    request.flush(
      { title: 'One or more validation errors occurred.', errors: { price: ["'Price' must be greater than '0'."] }, correlationId: 'abc' },
      { status: 400, statusText: 'Bad Request' },
    );

    const error = (await result.catch((e: unknown) => e)) as DataError;
    expect(error).toBeInstanceOf(DataError);
    expect(error.kind).toBe('validation');
    expect(error.fieldErrors['price']).toEqual(["'Price' must be greater than '0'."]);
    expect(error.correlationId).toBe('abc');
  });

  it('reports duplicates as conflicts using the API message', async () => {
    const result = repository.add({
      openLibraryKey: dto.openLibraryKey,
      title: dto.title,
      author: dto.author,
      isbn: dto.isbn,
      publishedYear: dto.publishedYear,
      coverUrl: null,
      details: { genre: 'SciFi', price: 9.99, stock: 1, description: null },
    });

    http.expectOne('/api/books').flush({ title: "'Dune' is already in the store." }, { status: 409, statusText: 'Conflict' });

    await expect(result).rejects.toMatchObject({ kind: 'conflict', message: "'Dune' is already in the store." });
  });
});
