import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ContributionService } from '../../core/contribution.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { ContributionTracker, ContributionCell, MemberTrackerRow, GroupMemberRole } from '../../core/models';
import { RecordPaymentDialogComponent } from '../record-payment-dialog/record-payment-dialog.component';

@Component({
  selector: 'app-tracker',
  imports: [
    CommonModule, MatTableModule, MatButtonModule, MatIconModule,
    MatDialogModule, MatProgressSpinnerModule, MatTooltipModule, CurrencyPipe
  ],
  templateUrl: './tracker.component.html',
  styleUrl: './tracker.component.scss'
})
export class TrackerComponent implements OnInit {
  groupId!: number;
  tracker?: ContributionTracker;
  loading = true;
  currentUserRole: GroupMemberRole | null = null;
  GroupMemberRole = GroupMemberRole;
  expandedRows = new Set<number>();

  displayedColumns = ['expand', 'member', 'lastPeriod', 'total', 'nextEmi', 'action'];

  toggleExpand(memberId: number) {
    if (this.expandedRows.has(memberId)) this.expandedRows.delete(memberId);
    else this.expandedRows.add(memberId);
  }

  isExpanded(memberId: number): boolean {
    return this.expandedRows.has(memberId);
  }

  getLastCell(row: MemberTrackerRow): ContributionCell | null {
    if (!row.cells.length) return null;
    return row.cells[row.cells.length - 1];
  }

  getLastPeriod(): string {
    const periods = this.tracker?.periods;
    return periods?.length ? this.formatPeriod(periods[periods.length - 1]) : '—';
  }

  constructor(
    private route: ActivatedRoute,
    private contributionSvc: ContributionService,
    private groupSvc: GroupService,
    private authSvc: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);
    this.loadRole();
    this.load();
  }

  loadRole() {
    const currentUserId = this.authSvc.currentUser()?.userId;
    if (!currentUserId) return;
    this.groupSvc.getGroup(this.groupId).subscribe(g => {
      const me = g.members.find(m => m.userId === currentUserId);
      this.currentUserRole = me?.role ?? null;
    });
  }

  load() {
    this.loading = true;
    this.contributionSvc.getTracker(this.groupId).subscribe({
      next: data => { this.tracker = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openRecordPayment(memberId: number, memberName: string) {
    const ref = this.dialog.open(RecordPaymentDialogComponent, {
      data: { groupId: this.groupId, groupMemberId: memberId, memberName }
    });
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  openEditPayment(memberId: number, memberName: string, contributionId: number, period: string, amount: number) {
    const ref = this.dialog.open(RecordPaymentDialogComponent, {
      data: { groupId: this.groupId, groupMemberId: memberId, memberName, contributionId, existingPeriod: period, existingAmount: amount }
    });
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  approveContribution(contributionId: number) {
    this.contributionSvc.approveContribution(this.groupId, contributionId).subscribe({
      next: () => this.load(),
      error: (err) => console.error('Approve failed', err)
    });
  }

  formatPeriod(period: string): string {
    const [y, m] = period.split('-');
    return new Date(+y, +m - 1).toLocaleString('default', { month: 'short', year: '2-digit' });
  }

  get currentMonthTotal(): number {
    return this.tracker?.rows.reduce((sum, row) => {
      const cell = this.getLastCell(row);
      return sum + (cell?.isPaid ? cell.amountPaid : 0);
    }, 0) ?? 0;
  }

  get totalNextEmi(): number {
    return this.tracker?.rows.reduce((sum, row) => sum + row.nextEmi, 0) ?? 0;
  }

  emiTooltip(row: { nextEmiSaving: number; nextEmiLoanPrincipal: number; nextEmiLoanInterest: number }): string {
    let tip = `बचत: ₹${row.nextEmiSaving}`;
    if (row.nextEmiLoanPrincipal > 0) {
      tip += ` | मुद्दल: ₹${row.nextEmiLoanPrincipal.toFixed(0)} | व्याज: ₹${row.nextEmiLoanInterest.toFixed(0)}`;
    }
    return tip;
  }
}
