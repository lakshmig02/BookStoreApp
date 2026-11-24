import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideInfrastructure } from '@infrastructure/provide-infrastructure';
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';

const BookStorePreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{indigo.50}',
      100: '{indigo.100}',
      200: '{indigo.200}',
      300: '{indigo.300}',
      400: '{indigo.400}',
      500: '{indigo.500}',
      600: '{indigo.600}',
      700: '{indigo.700}',
      800: '{indigo.800}',
      900: '{indigo.900}',
      950: '{indigo.950}',
    },
  },
});

/** Composition root: the only place that knows which infrastructure backs the domain ports. */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideInfrastructure(),
    providePrimeNG({
      theme: {
        preset: BookStorePreset,
        options: {
          // Must match the @layer order declared in styles.css.
          cssLayer: { name: 'primeng', order: 'theme, base, primeng, components, utilities' },
          darkModeSelector: '.app-dark',
        },
      },
    }),
  ],
};
