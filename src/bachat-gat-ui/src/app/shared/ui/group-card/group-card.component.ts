// GroupCard — gradient-top-bar card for Group list screen

import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InrPipe } from '../../pipes/inr.pipe';

export interface Group {
  id: number;
  name: string;
  memberCount: number;
  monthlyAmount: number;
  interestRate: number;
  description?: string;
}

@Component({
  selector: 'bg-group-card',
  standalone: true,
  imports: [CommonModule, InrPipe],
  template: `
    @let g = group();
    <button class="group-card" type="button" (click)="open.emit(g.id)">
      <div class="accent"></div>
      <div class="body">
        <div class="name">{{ g.name }}</div>
        <div class="members">{{ g.memberCount }} members</div>
        <p class="line"><strong>Monthly:</strong> ₹{{ g.monthlyAmount | inr }}</p>
        <p class="line"><strong>Interest:</strong> {{ g.interestRate }}% / month</p>
        @if (g.description) {
          <p class="desc">{{ g.description }}</p>
        }
      </div>
    </button>
  `,
  styleUrl: './group-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GroupCardComponent {
  group = input.required<Group>();
  open = output<number>();
}
