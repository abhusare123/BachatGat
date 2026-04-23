// Dashboard feature — demonstrates FundSummaryCard + LoanCard + GroupCard
// Copy into your repo at: src/bachat-gat-ui/src/app/features/dashboard/

import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FundSummaryCardComponent,
  FundSummary,
  LoanCardComponent,
  Loan,
  LoanVote,
  GroupCardComponent,
  Group,
  BrandHeroComponent,
} from '../../shared/ui';

@Component({
  selector: 'bg-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FundSummaryCardComponent,
    LoanCardComponent,
    GroupCardComponent,
    BrandHeroComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  // Replace with real service calls in your app
  protected summary = signal<FundSummary>({
    collected:   248000,
    outstanding: 42500,
    interest:    3125,
    pending:     12000,
  });

  protected groups = signal<Group[]>([
    { id: 1, name: 'Mahila Bachat Mandal / महिला बचत मंडळ', memberCount: 12, monthlyAmount: 500,  interestRate: 2,   description: 'Women\'s savings group — Shirdi chapter' },
    { id: 2, name: 'Shirdi Savings Group',                  memberCount: 8,  monthlyAmount: 1000, interestRate: 2.5, description: 'Monthly pooling, family emergencies' },
  ]);

  protected loans = signal<Loan[]>([
    { id: 1, memberName: 'Sunita Patil', status: 'pending', amount: 10000, tenureMonths: 10, interestRate: 2, purpose: 'Daughter\'s school fees', requestedAt: '2026-04-15', approveVotes: 4, rejectVotes: 1, eligibleVoters: 12, myVote: 0 },
    { id: 2, memberName: 'Manisha Jadhav', status: 'active', amount: 15000, tenureMonths: 12, interestRate: 2, purpose: 'Shop renovation',      requestedAt: '2026-01-10', approveVotes: 10, rejectVotes: 1, eligibleVoters: 12, myVote: 1 },
  ]);

  onVote(e: { id: number; vote: LoanVote }) {
    this.loans.update(list => list.map(l => {
      if (l.id !== e.id) return l;
      const prev = l.myVote;
      let { approveVotes, rejectVotes } = l;
      if (prev === 1) approveVotes -= 1;
      if (prev === 2) rejectVotes -= 1;
      if (e.vote === 1) approveVotes += 1;
      if (e.vote === 2) rejectVotes += 1;
      return { ...l, myVote: e.vote, approveVotes, rejectVotes };
    }));
  }

  setStatus(id: number, status: Loan['status']) {
    this.loans.update(list => list.map(l => l.id === id ? { ...l, status } : l));
  }
}
