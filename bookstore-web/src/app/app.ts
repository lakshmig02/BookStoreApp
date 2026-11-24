import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Body } from '@presentation/layout/body/body';
import { Footer } from '@presentation/layout/footer/footer';
import { Header } from '@presentation/layout/header/header';

/**
 * Application shell: Header, Body and Footer are siblings mounted directly here.
 * They share state through application-layer stores, never through this component.
 */
@Component({
  selector: 'app-root',
  imports: [Header, Body, Footer],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex min-h-screen flex-col' },
  template: `
    <app-header />
    <app-body />
    <app-footer />
  `,
})
export class App {}
