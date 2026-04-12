import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { LoanService } from '../../core/loan.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { GroupMemberRole, Loan, LoanStatus } from '../../core/models';

@Component({
  selector: 'app-loan-list',
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatCardModule, CurrencyPipe, DatePipe],
  templateUrl: './loan-list.component.html',
  styleUrl: './loan-list.component.scss'
})
export class LoanListComponent implements OnInit {
  groupId!: number;
  loans: Loan[] = [];
  loading = true;
  displayedColumns = ['member', 'amount', 'tenure', 'purpose', 'status', 'votes', 'actions'];
  LoanStatus = LoanStatus;
  currentUserId!: number;
  isAdminOrTreasurer = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loanSvc: LoanService,
    private groupSvc: GroupService,
    private authSvc: AuthService
  ) {}

  ngOnInit() {
    this.groupId = +this.route.snapshot.paramMap.get('id')!;
    this.currentUserId = this.authSvc.currentUser()!.userId;
    this.groupSvc.getGroup(this.groupId).subscribe(g => {
      const me = g.members.find(m => m.userId === this.currentUserId);
      this.isAdminOrTreasurer = !!me && (me.role === GroupMemberRole.Admin || me.role === GroupMemberRole.Treasurer);
    });
    this.load();
  }

  load() {
    this.loading = true;
    this.loanSvc.getLoans(this.groupId).subscribe({
      next: l => { this.loans = l; this.loading = false; },
      error: () => this.loading = false
    });
  }

  requestLoan() {
    this.router.navigate([`/groups/${this.groupId}/loans/request`]);
  }

  vote(loan: Loan, choice: number) {
    this.loanSvc.vote(loan.id, choice).subscribe(() => this.load());
  }

  approveLoan(loanId: number) {
    if (!confirm('Approve this loan?')) return;
    this.loanSvc.approveLoan(loanId).subscribe(() => this.load());
  }

  rejectLoan(loanId: number) {
    if (!confirm('Reject this loan?')) return;
    this.loanSvc.rejectLoan(loanId).subscribe(() => this.load());
  }

  disburse(loanId: number) {
    if (!confirm('Mark this loan as disbursed and generate repayment schedule?')) return;
    this.loanSvc.disburse(loanId).subscribe(() => this.load());
  }

  statusColor(status: LoanStatus): string {
    switch (status) {
      case LoanStatus.Pending: return 'accent';
      case LoanStatus.Approved: return 'primary';
      case LoanStatus.Active: return 'primary';
      case LoanStatus.Closed: return '';
      case LoanStatus.Rejected: return 'warn';
      default: return '';
    }
  }
}
