import { ChangeDetectionStrategy, Component, computed, input, linkedSignal } from '@angular/core';

/** Cover thumbnail served by Open Library's CDN, with a graceful placeholder when missing or broken. */
@Component({
  selector: 'app-book-cover',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (url() && !broken()) {
      <img
        [src]="url()"
        [alt]="'Cover of ' + title()"
        loading="lazy"
        class="h-full w-full rounded object-cover shadow-sm"
        (error)="broken.set(true)"
      />
    } @else {
      <div class="grid h-full w-full place-items-center rounded bg-slate-100 text-slate-400" aria-hidden="true">
        <i class="pi pi-book"></i>
      </div>
    }
  `,
  host: { class: 'block shrink-0', '[class]': 'sizeClass()' },
})
export class BookCover {
  readonly url = input<string | null>(null);
  readonly title = input('');
  readonly size = input<'sm' | 'md'>('sm');

  protected readonly broken = linkedSignal({ source: this.url, computation: () => false });
  protected readonly sizeClass = computed(() => (this.size() === 'md' ? 'h-24 w-16' : 'h-14 w-10'));
}
