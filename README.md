# Book Store

A small, production-shaped book store built as two projects:

| Folder | What | Stack |
| --- | --- | --- |
| [`api/`](api) | REST API | .NET 10 · EF Core 10 + SQLite · hand-rolled mediator (CQRS) · FluentValidation · Serilog · Microsoft.Extensions.Http.Resilience |
| [`web/`](web) | Web client | Angular 21 (standalone, zoneless, signals) · PrimeNG 21 · Tailwind CSS 4 · Vitest |

**The core idea:** Open Library is the *catalog* you search; the SQLite database is the *store* you own.
A user searches Open Library (always through the API), picks a result, adds store details (genre, price,
stock), and the API persists it as the store's own record. Open Library is never called again for that book.

---

## Running it from a clean checkout

### Prerequisites

| Tool | Version | Check |
| --- | --- | --- |
| .NET SDK | 10.0.x | `dotnet --version` |
| Node.js | 20.19+, 22.12+ or 24+ (Angular 21's supported ranges) | `node --version` |
| npm | 10+ | `npm --version` |

No API keys or secrets are needed: Open Library is a free, keyless API.

### 1. API: http://localhost:5080

```bash
cd api
dotnet run --project BookStore.Api
```

**Or with Visual Studio 2026:**

1. Open `api/BookStore.slnx`.
2. In Solution Explorer, right-click **BookStore.Api** and choose **Set as Startup Project**.
3. Pick the **http** launch profile and press **F5**.

The browser opens **Swagger UI** at http://localhost:5080/swagger, listing every endpoint with its request and
response models. Expand one, click **Try it out**, then **Execute**. http://localhost:5080/ redirects there.

On startup the API applies EF Core migrations and seeds 11 sample books (at least one per genre) into
`api/BookStore.Api/bookstore.db`. The file is git-ignored; delete it to start fresh.
In Development, the raw OpenAPI document is at http://localhost:5080/openapi/v1.json.

> The API serves JSON only. The store's user interface is the Angular app on port 4200 (next step),
> which must run *alongside* the API.

### 2. Web: http://localhost:4200

In a second terminal:

```bash
cd web
npm ci
npm start
```

The Angular dev server proxies `/api/*` to `http://localhost:5080` (see [`web/proxy.conf.json`](web/proxy.conf.json)),
so the browser talks to a single origin and no CORS setup is needed. Start the API first.

### Troubleshooting

| Symptom | Fix |
| --- | --- |
| Web app shows *"Cannot reach the Book Store API. Make sure it is running on http://localhost:5080."* | The API isn't running, or not on port 5080. Start it with the **http** profile and check that its console says `Now listening on: http://localhost:5080`. Then click **Retry**. |
| http://localhost:5080 shows nothing in the browser | Open http://localhost:5080/swagger, or run the **http** profile, which opens it automatically. |
| *"The current .NET SDK does not support targeting .NET 10.0"* | Install the .NET 10 SDK. In Visual Studio Installer, choose **Modify** and tick **ASP.NET and web development**. |
| *"Address already in use"* on port 5080 or 4200 | Another copy is still running. Stop it, or end it in Task Manager. |
| `npm` is not recognized | Install Node.js LTS from nodejs.org, then reopen the terminal or Visual Studio. |

### Tests

```bash
cd api && dotnet test             # 23 tests: handlers, Open Library adapter, raw SQL, layer rules, HTTP end-to-end
```

```bash
cd web && npm test                # Vitest: stores (sibling filter propagation, stale responses), HTTP repository mapping
```

```bash
cd web && npm run check:layers    # fails on any import that crosses a frontend layer boundary
```

### Configuration

Everything lives in [`api/BookStore.Api/appsettings.json`](api/BookStore.Api/appsettings.json). Override any
value with an environment variable, using `__` as the section separator:

| Setting | Default | Example override |
| --- | --- | --- |
| `ConnectionStrings:BookStore` | `Data Source=bookstore.db` (relative to the API project folder) | `ConnectionStrings__BookStore="Data Source=/tmp/store.db"` |
| `OpenLibrary:BaseUrl` | `https://openlibrary.org/` | `OpenLibrary__BaseUrl=http://localhost:5099/` (a local stub) |
| `OpenLibrary:UserAgent` | `BookStore/1.0 (technical assessment)` | Open Library asks clients to identify themselves |
| `OpenLibrary:AttemptTimeoutSeconds` / `TotalTimeoutSeconds` | `5` / `12` | |

The web client needs no configuration. `API_BASE_URL` defaults to same-origin, behind the proxy.

---

## API endpoints

| Method & route | Slice (mediator request) | Touches Open Library? |
| --- | --- | --- |
| `GET /api/catalog/search?q=&page=&pageSize=` | `SearchCatalogQuery` | **Yes** (proxied, normalized, never persisted) |
| `GET /api/books?genre=&page=&pageSize=&sortBy=&sortDir=` | `GetBooksQuery` | No |
| `GET /api/books/{id}` | `GetBookByIdQuery` | No |
| `POST /api/books` | `AddBookCommand` | No (persists the selected result plus store fields) |
| `PUT /api/books/{id}` | `UpdateBookCommand` | No (store-owned fields only) |
| `DELETE /api/books/{id}` | `DeleteBookCommand` | No |
| `GET /api/genres` | `GetGenreSummaryQuery` | No (raw SQL aggregate; powers the header filter's counts) |

- `sortBy` is one of `Title`, `Author`, `PublishedYear`, `Genre`, `Price`, `Stock` or `CreatedAt`.
- `sortDir` is `Asc` or `Desc`.
- `genre` is one of `Fiction`, `SciFi`, `Fantasy`, `Mystery`, `Biography`, `NonFiction`, `Children` or `History`.

Errors are [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) `application/problem+json`. Each carries a `correlationId`, and expected failures also carry a stable `code`:

| Status | When |
| --- | --- |
| 400 | Validation errors (with an `errors` map per field), malformed JSON, or an unknown enum value |
| 404 | `Book.NotFound` |
| 409 | `Book.Duplicate`: same Open Library key or ISBN is already in the store |
| 503 | `Catalog.Unavailable`: Open Library is down, slow or returned garbage |
| 500 | Unexpected; no details leak |

> **Where is the hand-written query?** The genre aggregate (`GROUP BY` with `COUNT(*)` and `SUM(Stock)`) is in
> [`api/BookStore.Infrastructure/Persistence/GenreSummaryReader.cs`](api/BookStore.Infrastructure/Persistence/GenreSummaryReader.cs).
> It is exposed as `GET /api/genres`, and the header's genre dropdown shows its per-genre counts.

---

## API architecture: Clean Architecture *and* Vertical Slice

```
api
├── BookStore.Domain            # Book aggregate, Genre. No dependencies at all.
├── BookStore.Application       # Use cases, organized BY FEATURE
│   ├── Abstractions/           #   ports + mediator: ISender, ICommand/IQuery, IBookStoreDbContext, IBookCatalog
│   ├── Common/                 #   Result/Error, PagedResult, pipeline behaviors (logging, validation)
│   └── Features/
│       ├── Books/
│       │   ├── AddBook/        #   AddBookCommand · AddBookValidator · AddBookHandler
│       │   ├── UpdateBook/     #   UpdateBookCommand · UpdateBookValidator · UpdateBookHandler
│       │   ├── DeleteBook/
│       │   ├── GetBookById/
│       │   ├── GetBooks/       #   GetBooksQuery · GetBooksValidator · GetBooksHandler
│       │   └── BookDto.cs, BookErrors.cs, BookRules.cs   (shared within the Books feature only)
│       ├── Catalog/SearchCatalog/
│       └── Genres/GetGenreSummary/   # includes the slice-owned port IGenreSummaryReader
├── BookStore.Infrastructure    # EF Core/SQLite (DbContext, config, migrations, seeder, raw SQL), Open Library adapter
├── BookStore.Api               # Composition root: minimal-API endpoints, problem details, correlation IDs, Serilog
└── BookStore.Tests             # xUnit: handlers, Open Library adapter, raw SQL, layer rules, HTTP end-to-end
```

**How the two ideas fit together:** the *projects* are Clean Architecture's layers, and dependencies point inward:
Api → Infrastructure → Application → Domain. Inside Application, code is grouped by *use case*, not by technical
type. A slice holds everything about one request (message, validator, handler), so a change to "add book" touches
one folder. Cross-cutting concerns (logging, validation) live once in the mediator pipeline rather than in every
slice. Ports live where they are needed: the general ones under `Abstractions/`, and `IGenreSummaryReader` inside its slice,
because only that slice uses it. `api/BookStore.Tests/Architecture/LayerDependencyTests.cs` enforces the direction.

### Decisions worth calling out

- **Hand-rolled mediator, not MediatR.** It is about 60 lines in
  [`Abstractions/Messaging/Mediator.cs`](api/BookStore.Application/Abstractions/Messaging/Mediator.cs): it resolves one handler,
  wraps it in the registered behaviors, and caches the generic dispatcher per request type. It avoids MediatR's
  commercial licence, and every line is understood.
- **CQRS is explicit in the type system.** Requests implement either `ICommand<T>` or `IQuery<T>`. Queries use
  `AsNoTracking()` and never call `SaveChanges`. Both share one SQLite database; separate read stores would be gold-plating here.
- **No generic repository over EF Core.** Handlers use `IBookStoreDbContext` (a `DbSet<Book>` plus `SaveChangesAsync`),
  because EF Core already is a repository and unit of work. The cost is that Application references
  `Microsoft.EntityFrameworkCore` (core abstractions only). The SQLite provider stays in Infrastructure, and a test asserts that.
- **Expected failures are values; invalid input is an exception.** Handlers return `Result<T>` for not-found, duplicate
  and catalog-unavailable. Validation runs once, in a pipeline behavior, and short-circuits with a `ValidationException`
  that becomes a 400. Handlers therefore only ever see valid input.
- **Rich domain model.** `Book.Create` and `UpdateStoreDetails` guard invariants. Open Library fields have private
  setters and no update path, so "never re-fetched, never re-edited" is enforced by the model itself.
- **Duplicate prevention, defense in depth:**
  1. Search results come back flagged `alreadyInStore`, and the UI disables them.
  2. `AddBook` pre-checks by Open Library key or normalized ISBN (hyphens ignored) and returns 409.
  3. Unique indexes on `OpenLibraryKey` and `Isbn` catch a concurrent race, which is also mapped to 409.
- **SQLite quirks handled explicitly.** SQLite has no decimal type, and EF cannot `ORDER BY` a TEXT-backed decimal.
  Price is therefore stored exactly as integer cents (`PriceCents` column). `DateTimeOffset` is stored as ticks so it sorts.
  Genre is stored by name so it reads well in raw SQL and survives enum reordering.
- **Open Library isolation and resilience** ([`OpenLibraryCatalog.cs`](api/BookStore.Infrastructure/Catalog/OpenLibrary/OpenLibraryCatalog.cs)):
  - The wire DTOs never leave that folder. The adapter normalizes results: it joins authors, takes the first ISBN,
    derives the cover URL from `cover_i` or the ISBN, and drops docs with no title.
  - ISBN-looking input is sent as `isbn=` rather than as free text.
  - The typed `HttpClient` sends a `User-Agent`, asks for only the needed `fields`, and caps `limit` at 20.
  - Resilience pipeline: 12 s total budget, one polite retry, a circuit breaker, and a 5 s per-attempt timeout.
  - Any upstream failure becomes `CatalogUnavailableException`, then a 503 with a friendly message. A client
    cancellation is passed through rather than reported as an outage.
- **Structured logging with correlation.** `CorrelationIdMiddleware` accepts or creates `X-Correlation-ID`, echoes it,
  and pushes it into Serilog's `LogContext`. Every log line of a request carries the same `CorrelationId`: the
  mediator's `Handling AddBookCommand`, the outbound Open Library call and its retry, and the request summary.
  Messages use `[LoggerMessage]` templates (for example `{CatalogQuery}`, `{ResultCount}` and `{ElapsedMs}`), never concatenation.

---

## Web architecture

```
web/src/app
├── domain/            # Pure TypeScript: Book, Genre, CatalogBook, BookQuery, DataError + repository PORTS
│   └── repositories/  #   BookRepository, CatalogRepository (abstract classes = DI tokens, no Angular import)
├── application/       # Use cases + state (Angular DI and signals, but no HTTP and no PrimeNG)
│   ├── books/         #   GenreFilterStore, BookListStore, BookCommands (add / update / delete)
│   ├── catalog/       #   CatalogSearchStore (scoped to the add dialog)
│   └── genres/        #   GenreSummaryStore
├── infrastructure/    # The ONLY layer that knows HttpClient, URLs and DTOs
│   └── http/          #   HttpBookRepository, HttpCatalogRepository, DTOs, mappers, problem-details → DataError
├── presentation/      # Components and PrimeNG: layout/{header,body,footer}, books/{grid,dialogs,fields}, shared/
├── app.ts             # Shell: <app-header/> <app-body/> <app-footer/> as siblings
└── app.config.ts      # Composition root: binds ports → HTTP adapters, configures the PrimeNG theme
```

Dependencies point inward: presentation → application → domain, and infrastructure → domain. Path aliases (`@domain/*` and
friends) make every import's layer obvious, and `npm run check:layers` fails the build on a wrong-way import.

- **Repository pattern.** Components and use cases inject `BookRepository` and `CatalogRepository`, never `HttpClient`.
  The ports return `Promise`s and take an `AbortSignal`, both web standards, so the domain does not even depend on RxJS.
  RxJS stays an implementation detail of the HTTP adapters. Swapping the data source means editing one provider in
  `provideInfrastructure()`. The unit tests do exactly that with in-memory fakes.
- **Header → Body communication.** `GenreFilterStore` is a root-provided service that holds a `signal<Genre | null>`.
  The Header writes to it; `BookListStore` reads it in a `computed` query, which an `effect` turns into a fetch.
  - Why not alternatives:
    - `@Input`/`@Output` through the root would turn the shell into a state relay.
    - A URL query param is a good alternative if deep-linking matters, but needs the router for one value.
    - NgRx is heavyweight for one value.
  - Why signals: synchronous reads in templates, automatic dependency tracking, and no subscriptions to manage.
  - Neither component knows the other exists. Changing the genre resets to page 1, via a `linkedSignal`.
- **Async states everywhere:**
  - The grid shows a loading overlay, an empty state with a call to action, and an error banner with Retry.
  - The add dialog has distinct idle, searching (skeletons), no-results, failed (with Retry) and results states.
  - Saves show a busy button, server validation messages on the matching fields, and conflict and unavailable
    messages that include the server's correlation ID.
  - Deletes ask for confirmation and report success or failure with a toast.
  - Out-of-order responses are dropped, and a superseded catalog search is aborted.
- **PrimeNG for components, Tailwind for layout.** The table, select, dialog, listbox, inputs, buttons, tags,
  toast and confirm dialog are all PrimeNG. Layout, spacing and typography use Tailwind utilities.
  `styles.css` declares the cascade-layer order `theme, base, primeng, components, utilities` so they coexist:
  Tailwind's reset doesn't clobber PrimeNG, and utilities can still override it.

---

## Trade-offs, known limitations, and what I'd do next

- **Description is optional and manual.** I didn't add the second Works-endpoint call at add-time. It would put a
  second network dependency on the save path for a field that is often missing or inconsistently shaped. Next step:
  fetch it opportunistically, with a short timeout, while the user fills in the details step.
- **The client sends the selected bibliographic fields on `POST /api/books`.** The API validates them (key format,
  ISBN length, https cover URL and so on), but doesn't re-verify them against Open Library. For a trusted admin UI that is
  a reasonable trade. For untrusted clients I'd post only the key, then have the server re-fetch and normalize it.
- **No optimistic concurrency.** Last write wins on `PUT`. I'd add a SQLite row version, return an `ETag`, and require `If-Match`.
- **Only paging, sorting and genre in the grid.** Free-text search over the store is a natural next slice.
- **Catalog results show the first 10 matches.** The API supports `page`; the dialog could add "load more".
- **Seed data:** work keys and ISBNs are real titles but were typed in from memory, so some keys may not resolve exactly.
- **Not done, by design (per the brief):** auth, Docker, CI, dark mode.
- **Tooling note:** npm 10 crashes (`Cannot read properties of null (reading 'edgesOut')`) when resolving a *fresh*
  dependency tree that includes `jsdom`'s optional peers. Installing from the committed lockfile with `npm ci` works on npm 10.
  To add or upgrade packages, use npm 11 (`npx npm@11 install …`).
