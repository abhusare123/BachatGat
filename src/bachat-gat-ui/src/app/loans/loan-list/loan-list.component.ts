import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { LoanService } from '../../core/loan.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { GroupMemberRole, Loan, LoanStatus } from '../../core/models';
import { CloseLoanDialogComponent } from '../close-loan-dialog/close-loan-dialog.component';

@Component({
  selector: 'app-loan-list',
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatCardModule, CurrencyPipe, DatePipe,
    MatDialogModule, MatSnackBarModule],
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
    private authSvc: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);
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

  closeLoan(loan: Loan) {
    this.loanSvc.getForeclosurePreview(loan.id).subscribe({
      next: summary => {
        const ref = this.dialog.open(CloseLoanDialogComponent, {
          width: '420px',
          data: { borrowerName: loan.requestedByName, summary }
        });
        ref.afterClosed().subscribe(confirmed => {
          if (!confirmed) return;
          this.loanSvc.closeLoan(loan.id).subscribe({
            next: s => {
              this.snackBar.open(
                `Loan closed. Collected ₹${s.totalAmount.toFixed(2)} (Principal ₹${s.outstandingPrincipal.toFixed(2)} + Interest ₹${s.foreclosureInterest.toFixed(2)})`,
                'OK', { duration: 6000 }
              );
              this.load();
            },
            error: err => this.snackBar.open(err?.error?.message ?? 'Failed to close loan', 'Close', { duration: 4000 })
          });
        });
      },
      error: err => this.snackBar.open(err?.error?.message ?? 'Failed to load foreclosure details', 'Close', { duration: 4000 })
    });
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
