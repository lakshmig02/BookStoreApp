import { provideHttpClient, withFetch } from '@angular/common/http';
import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { BookRepository, CatalogRepository } from '@domain/repositories';
import { HttpBookRepository } from './http/http-book.repository';
import { HttpCatalogRepository } from './http/http-catalog.repository';

/** Binds the domain ports to their HTTP adapters. Swap these to change the data source. */
export function provideInfrastructure(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideHttpClient(withFetch()),
    { provide: BookRepository, useClass: HttpBookRepository },
    { provide: CatalogRepository, useClass: HttpCatalogRepository },
  ]);
}
