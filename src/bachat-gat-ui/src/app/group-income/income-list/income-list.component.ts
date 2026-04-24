import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { GroupIncomeService } from '../../core/group-income.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { GroupIncomeDto, GroupMemberRole } from '../../core/models';
import { AddIncomeDialogComponent } from '../add-income-dialog/add-income-dialog.component';

@Component({
  selector: 'app-income-list',
  imports: [
    CommonModule, CurrencyPipe, DatePipe,
    MatTableModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatDialogModule, TranslateModule
  ],
  templateUrl: './income-list.component.html',
  styleUrl: './income-list.component.scss'
})
export class IncomeListComponent implements OnInit {
  groupId!: number;
  entries: GroupIncomeDto[] = [];
  loading = true;
  currentUserRole?: GroupMemberRole;
  displayedColumns = ['date', 'category', 'description', 'member', 'amount', 'recordedBy', 'actions'];

  get canAdd(): boolean {
    return this.currentUserRole !== undefined
      && this.currentUserRole <= GroupMemberRole.Treasurer;
  }

  get canDelete(): boolean {
    return this.currentUserRole === GroupMemberRole.Admin;
  }

  get totalAmount(): number {
    return this.entries.reduce((s, e) => s + e.amount, 0);
  }

  constructor(
    private route: ActivatedRoute,
    private incomeSvc: GroupIncomeService,
    private groupSvc: GroupService,
    private authSvc: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);

    this.groupSvc.getGroup(this.groupId).subscribe(g => {
      const userId = this.authSvc.currentUser()?.userId;
      const me = g.members.find(m => m.userId === userId && m.isActive);
      if (me) this.currentUserRole = me.role;
    });

    this.loadEntries();
  }

  loadEntries() {
    this.loading = true;
    this.incomeSvc.getEntries(this.groupId).subscribe({
      next: data => { this.entries = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openAddDialog() {
    const ref = this.dialog.open(AddIncomeDialogComponent, { width: '420px' });
    ref.componentInstance.setGroupId(this.groupId);
    ref.afterClosed().subscribe(added => { if (added) this.loadEntries(); });
  }

  deleteEntry(id: number) {
    if (!confirm('Delete this entry?')) return;
    this.incomeSvc.deleteEntry(this.groupId, id).subscribe(() => this.loadEntries());
  }

  categoryKey(cat: string): string {
    switch (cat) {
      case 'Penalty':      return 'income.categoryPenalty';
      case 'BankInterest': return 'income.categoryBankInterest';
      default:             return 'income.categoryOther';
    }
  }
}
