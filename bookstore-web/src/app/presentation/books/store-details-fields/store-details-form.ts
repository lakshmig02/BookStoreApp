import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DataError, Genre, StoreDetails } from '@domain/models';

/** Mirrors the API's rules so most mistakes are caught before a round-trip. */
export const PRICE_MAX = 100_000;
export const DESCRIPTION_MAX = 4000;

export type StoreDetailsForm = FormGroup<{
  genre: FormControl<Genre | null>;
  price: FormControl<number | null>;
  stock: FormControl<number | null>;
  description: FormControl<string>;
}>;

export function createStoreDetailsForm(initial?: StoreDetails): StoreDetailsForm {
  return new FormGroup({
    genre: new FormControl<Genre | null>(initial?.genre ?? null, Validators.required),
    price: new FormControl<number | null>(initial?.price ?? null, [
      Validators.required,
      Validators.min(0.01),
      Validators.max(PRICE_MAX),
    ]),
    stock: new FormControl<number | null>(initial?.stock ?? 0, [Validators.required, Validators.min(0)]),
    description: new FormControl(initial?.description ?? '', {
      nonNullable: true,
      validators: Validators.maxLength(DESCRIPTION_MAX),
    }),
  });
}

/** Call only when the form is valid. */
export function readStoreDetails(form: StoreDetailsForm): StoreDetails {
  const { genre, price, stock, description } = form.getRawValue();
  return { genre: genre!, price: price!, stock: stock ?? 0, description: description.trim() || null };
}

/** Puts server-side validation messages onto the matching controls. Returns true if any matched. */
export function applyServerErrors(form: StoreDetailsForm, error: DataError): boolean {
  let applied = false;
  for (const [field, messages] of Object.entries(error.fieldErrors)) {
    const control = form.get(field);
    if (control && messages.length) {
      control.setErrors({ server: messages.join(' ') });
      control.markAsTouched();
      applied = true;
    }
  }
  return applied;
}
