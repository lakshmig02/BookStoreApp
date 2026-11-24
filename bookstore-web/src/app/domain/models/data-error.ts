export type DataErrorKind = 'validation' | 'not-found' | 'conflict' | 'unavailable' | 'network' | 'unknown';

/**
 * Data-source-agnostic failure raised by repositories. Callers branch on `kind`,
 * never on HTTP status codes, so the transport stays swappable.
 */
export class DataError extends Error {
  constructor(
    readonly kind: DataErrorKind,
    message: string,
    /** Field name (camelCase) -> messages, for validation failures. */
    readonly fieldErrors: Readonly<Record<string, readonly string[]>> = {},
    /** Server correlation id, useful when reporting a problem. */
    readonly correlationId: string | null = null,
  ) {
    super(message);
    this.name = 'DataError';
  }

  static from(error: unknown): DataError {
    return error instanceof DataError
      ? error
      : new DataError('unknown', error instanceof Error ? error.message : 'Something went wrong.');
  }
}
