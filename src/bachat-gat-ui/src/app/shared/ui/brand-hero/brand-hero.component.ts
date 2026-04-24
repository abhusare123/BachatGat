// BrandHero — reusable dark-green gradient hero block
// Use anywhere you want the signature brand treatment

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'bg-brand-hero',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="brand-hero" [class.compact]="compact()">
      <div class="hero-text">
        @if (eyebrow()) {
          <div class="eyebrow">{{ eyebrow() }}</div>
        }
        <div class="title">
          <ng-content select="[slot=title]"></ng-content>
        </div>
        @if (caption()) {
          <div class="caption">{{ caption() }}</div>
        }
      </div>
      <ng-content></ng-content>
    </div>
  `,
  styleUrl: './brand-hero.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BrandHeroComponent {
  eyebrow = input<string>('');
  caption = input<string>('');
  compact = input<boolean>(false);
}
