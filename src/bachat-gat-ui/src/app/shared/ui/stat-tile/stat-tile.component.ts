// StatTile — compact stat card with gradient top-bar accent
// Used in FundSummaryCard and standalone in dashboards

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InrPipe } from '../../pipes/inr.pipe';

export type StatTileColor = 'green' | 'blue' | 'teal' | 'amber' | 'red' | 'purple';

@Component({
  selector: 'bg-stat-tile',
  standalone: true,
  imports: [CommonModule, InrPipe],
  template: `
    <div class="stat-tile" [class]="'tile-' + color()">
      <div class="lbl">{{ label() }}</div>
      <div class="val">
        @if (isCurrency()) {
          <span class="sym">₹</span>{{ value() | inr }}
        } @else {
          {{ value() | number }}
        }
      </div>
    </div>
  `,
  styleUrl: './stat-tile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatTileComponent {
  /** Uppercase label shown above the value */
  label = input.required<string>();

  /** Numeric value */
  value = input.required<number>();

  /** Gradient accent colour (maps to semantic token ramp) */
  color = input<StatTileColor>('green');

  /** Format as ₹ currency with Indian lakh grouping — defaults to true */
  isCurrency = input<boolean>(true);
}
