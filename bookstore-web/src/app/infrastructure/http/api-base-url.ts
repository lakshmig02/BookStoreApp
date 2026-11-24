import { InjectionToken } from '@angular/core';

/** Root of the Book Store API. Empty = same origin (the dev server proxies `/api` to the API). */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', { factory: () => '' });
