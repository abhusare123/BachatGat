// FundSummaryCard — the "Numerals" design
// Shows a hero KPI (total collected) alongside 4 stat tiles.
// Faithful port of preview/type-numerals.html

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatTileComponent, StatTileColor } from '../stat-tile/stat-tile.component';
import { InrPipe } from '../../pipes/inr.pipe';

export interface FundSummary {
  /** Total collected so far (₹) — displayed as the hero number */
  collected: number;
  /** Loans outstanding (₹) */
  outstanding: number;
  /** Interest accrued / collected (₹) */
  interest: number;
  /** Pending (unapproved) contributions (₹) */
  pending: number;
}

type TileDef = { label: string; value: number; color: StatTileColor };

@Component({
  selector: 'bg-fund-summary-card',
  standalone: true,
  imports: [CommonModule, StatTileComponent, InrPipe],
  templateUrl: './fund-summary-card.component.html',
  styleUrl: './fund-summary-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FundSummaryCardComponent {
  /** Fund summary figures. Required. */
  summary = input.required<FundSummary>();

  /** Heading eyebrow — bilingual by default */
  eyebrow = input<string>('Total Collected · एकूण भरणा');

  /** Caption under the hero number */
  caption = input<string>('Tabular figures · Indian lakh grouping · no decimals');

  protected tiles = computed<TileDef[]>(() => {
    const s = this.summary();
    return [
      { label: 'Collected',   value: s.collected,   color: 'green' },
      { label: 'Outstanding', value: s.outstanding, color: 'blue'  },
      { label: 'Interest',    value: s.interest,    color: 'teal'  },
      { label: 'Pending',     value: s.pending,     color: 'amber' },
    ];
  });
}
