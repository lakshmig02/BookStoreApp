import { Pipe, PipeTransform } from '@angular/core';
import { Genre, GENRE_LABELS } from '@domain/models';

@Pipe({ name: 'genreLabel' })
export class GenreLabelPipe implements PipeTransform {
  transform(genre: Genre): string {
    return GENRE_LABELS[genre];
  }
}
