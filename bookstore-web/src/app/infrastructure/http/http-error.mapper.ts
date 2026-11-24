import { HttpErrorResponse } from '@angular/common/http';
import { DataError, DataErrorKind } from '@domain/models';
import { ProblemDetailsDto } from './api.dtos';

const KIND_BY_STATUS: Readonly<Record<number, DataErrorKind>> = {
  400: 'validation',
  404: 'not-found',
  409: 'conflict',
  502: 'unavailable',
  503: 'unavailable',
  504: 'unavailable',
};

const FALLBACK_MESSAGES: Readonly<Record<DataErrorKind, string>> = {
  validation: 'Some of the values are invalid.',
  'not-found': 'That book no longer exists.',
  conflict: 'That book is already in the store.',
  unavailable: 'A service we depend on is unavailable. Please try again shortly.',
  network: 'Cannot reach the Book Store API. Is it running?',
  unknown: 'Something went wrong. Please try again.',
};

/** Translates transport-level failures into the domain's `DataError`. */
export function toDataError(error: unknown): DataError {
  if (!(error instanceof HttpErrorResponse)) return DataError.from(error);

  const kind: DataErrorKind = error.status === 0 ? 'network' : (KIND_BY_STATUS[error.status] ?? 'unknown');
  const problem = isProblem(error.error) ? error.error : {};

  // Validation problems carry a generic title; the field messages are the useful part.
  const message =
    kind === 'validation' && problem.errors
      ? Object.values(problem.errors).flat().join(' ')
      : (problem.detail ?? (kind === 'unknown' ? undefined : problem.title) ?? FALLBACK_MESSAGES[kind]);

  return new DataError(kind, message, problem.errors ?? {}, problem.correlationId ?? null);
}

function isProblem(body: unknown): body is ProblemDetailsDto {
  return typeof body === 'object' && body !== null && ('title' in body || 'errors' in body);
}
