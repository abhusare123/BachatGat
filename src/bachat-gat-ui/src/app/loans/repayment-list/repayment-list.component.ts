import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoanService } from '../../core/loan.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { GroupMemberRole, LoanRepayment } from '../../core/models';

@Component({
  selector: 'app-repayment-list',
  imports: [
    CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule,
    MatCardModule, MatChipsModule, MatProgressSpinnerModule, CurrencyPipe, DatePipe
  ],
  templateUrl: './repayment-list.component.html',
  styleUrl: './repayment-list.component.scss'
})
export class RepaymentListComponent implements OnInit {
  loanId!: number;
  groupId!: number;
  repayments: LoanRepayment[] = [];
  loading = true;
  payingId: number | null = null;
  undoingId: number | null = null;
  isAdminOrTreasurer = false;
  displayedColumns = ['period', 'emiAmount', 'principalAmount', 'interestAmount', 'status', 'action'];

  constructor(
    private route: ActivatedRoute,
    private loanSvc: LoanService,
    private groupSvc: GroupService,
    private authSvc: AuthService
  ) {}

  ngOnInit() {
    // traverse route tree for group id
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);
    this.loanId = +this.route.snapshot.paramMap.get('loanId')!;

    const currentUserId = this.authSvc.currentUser()!.userId;
    this.groupSvc.getGroup(this.groupId).subscribe(g => {
      const me = g.members.find(m => m.userId === currentUserId);
      this.isAdminOrTreasurer = !!me && (me.role === GroupMemberRole.Admin || me.role === GroupMemberRole.Treasurer);
      this.displayedColumns = this.isAdminOrTreasurer
        ? ['period', 'emiAmount', 'principalAmount', 'interestAmount', 'status', 'action']
        : ['period', 'emiAmount', 'principalAmount', 'interestAmount', 'status'];
    });

    this.load();
  }

  load() {
    this.loading = true;
    this.loanSvc.getRepayments(this.loanId).subscribe({
      next: data => { this.repayments = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  markPaid(repaymentId: number) {
    this.payingId = repaymentId;
    this.loanSvc.markRepaymentPaid(this.loanId, repaymentId).subscribe({
      next: () => { this.payingId = null; this.load(); },
      error: () => { this.payingId = null; }
    });
  }

  undoMarkPaid(repaymentId: number) {
    this.undoingId = repaymentId;
    this.loanSvc.unmarkRepaymentPaid(this.loanId, repaymentId).subscribe({
      next: () => { this.undoingId = null; this.load(); },
      error: () => { this.undoingId = null; }
    });
  }

  get outstandingBalance(): number {
    return this.repayments
      .filter(r => !r.isPaid)
      .reduce((sum, r) => sum + r.principalAmount, 0);
  }

  get paidCount(): number { return this.repayments.filter(r => r.isPaid).length; }
  get totalInterestPaid(): number { return this.repayments.filter(r => r.isPaid).reduce((s, r) => s + r.interestAmount, 0); }
  get isForeclosed(): boolean { return this.repayments.some(r => r.isForeclosed); }

  get totalEmi(): number {
    return this.repayments.length > 0 ? this.repayments[0].emiAmount : 0;
  }

  formatPeriod(p: string): string {
    const [y, m] = p.split('-');
    return new Date(+y, +m - 1).toLocaleString('default', { month: 'short', year: 'numeric' });
  }
}
