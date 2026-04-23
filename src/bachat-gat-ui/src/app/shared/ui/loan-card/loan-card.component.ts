// LoanCard — status-coded left border + vote bar + action buttons
// Faithful to the Angular app's loan list pattern

import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { InrPipe } from '../../pipes/inr.pipe';

export type LoanStatus = 'pending' | 'approved' | 'active' | 'closed' | 'rejected';
export type LoanVote = 0 | 1 | 2; // 0=none, 1=approve, 2=reject

export interface Loan {
  id: number;
  memberName: string;
  status: LoanStatus;
  amount: number;
  tenureMonths: number;
  interestRate: number;
  purpose?: string;
  requestedAt: string | Date;
  closedAt?: string | Date;
  approveVotes: number;
  rejectVotes: number;
  eligibleVoters: number;
  myVote: LoanVote;
}

@Component({
  selector: 'bg-loan-card',
  standalone: true,
  imports: [CommonModule, DatePipe, MatIconModule, MatButtonModule, InrPipe],
  templateUrl: './loan-card.component.html',
  styleUrl: './loan-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoanCardComponent {
  loan = input.required<Loan>();
  /** Show admin-only actions: Approve, Reject, Disburse, Close Loan */
  isAdmin = input<boolean>(false);

  vote = output<{ id: number; vote: LoanVote }>();
  approve = output<number>();
  reject = output<number>();
  disburse = output<number>();
  close = output<number>();
  viewRepayments = output<number>();

  protected statusLabel = computed(() => {
    const s = this.loan().status;
    return s.charAt(0).toUpperCase() + s.slice(1);
  });

  onVote(v: LoanVote) {
    this.vote.emit({ id: this.loan().id, vote: v });
  }
}
