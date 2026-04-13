import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ExpenseService } from '../../core/expense.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { ExpenseDto, GroupMemberRole } from '../../core/models';
import { AddExpenseDialogComponent } from '../add-expense-dialog/add-expense-dialog.component';

@Component({
  selector: 'app-expense-list',
  imports: [
    CommonModule, CurrencyPipe, DatePipe,
    MatTableModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatDialogModule
  ],
  templateUrl: './expense-list.component.html',
  styleUrl: './expense-list.component.scss'
})
export class ExpenseListComponent implements OnInit {
  groupId!: number;
  expenses: ExpenseDto[] = [];
  loading = true;
  currentUserRole?: GroupMemberRole;
  displayedColumns = ['date', 'description', 'category', 'amount', 'recordedBy', 'actions'];

  get canAdd(): boolean {
    return this.currentUserRole !== undefined
      && this.currentUserRole <= GroupMemberRole.Treasurer;
  }

  get canDelete(): boolean {
    return this.currentUserRole === GroupMemberRole.Admin;
  }

  get totalAmount(): number {
    return this.expenses.reduce((s, e) => s + e.amount, 0);
  }

  constructor(
    private route: ActivatedRoute,
    private expenseSvc: ExpenseService,
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

    this.loadExpenses();
  }

  loadExpenses() {
    this.loading = true;
    this.expenseSvc.getExpenses(this.groupId).subscribe({
      next: data => { this.expenses = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openAddDialog() {
    const ref = this.dialog.open(AddExpenseDialogComponent, { width: '420px' });
    ref.componentInstance.setGroupId(this.groupId);
    ref.afterClosed().subscribe(added => { if (added) this.loadExpenses(); });
  }

  deleteExpense(id: number) {
    if (!confirm('Delete this expense?')) return;
    this.expenseSvc.deleteExpense(this.groupId, id).subscribe(() => this.loadExpenses());
  }
}
