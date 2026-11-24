import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AbstractControl, ReactiveFormsModule } from '@angular/forms';
import { GENRE_LABELS, GENRES } from '@domain/models';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { DESCRIPTION_MAX, PRICE_MAX, StoreDetailsForm } from './store-details-form';

/** The store-owned fields (genre, price, stock, description), shared by the add and edit dialogs. */
@Component({
  selector: 'app-store-details-fields',
  imports: [ReactiveFormsModule, SelectModule, InputNumberModule, TextareaModule, MessageModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './store-details-fields.html',
})
export class StoreDetailsFields {
  readonly form = input.required<StoreDetailsForm>();
  readonly idPrefix = input('details');

  protected readonly genreOptions = GENRES.map((value) => ({ value, label: GENRE_LABELS[value] }));
  protected readonly priceMax = PRICE_MAX;
  protected readonly descriptionMax = DESCRIPTION_MAX;

  protected errorFor(control: AbstractControl): string | null {
    if (!control.touched || !control.errors) return null;
    const errors = control.errors;
    if (errors['server']) return errors['server'];
    if (errors['required']) return 'Required.';
    if (errors['min']) return `Must be at least ${errors['min'].min}.`;
    if (errors['max']) return `Must be at most ${errors['max'].max}.`;
    if (errors['maxlength']) return `At most ${errors['maxlength'].requiredLength} characters.`;
    return 'Invalid value.';
  }
}
