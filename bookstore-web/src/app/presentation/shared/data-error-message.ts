import { DataError } from '@domain/models';

/** User-facing text for a failed operation, with the server reference when there is one. */
export function describeError(error: unknown): string {
  const dataError = DataError.from(error);
  return dataError.correlationId ? `${dataError.message} (ref: ${dataError.correlationId})` : dataError.message;
}
