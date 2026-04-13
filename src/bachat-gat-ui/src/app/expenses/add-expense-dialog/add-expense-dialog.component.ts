import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { ExpenseService } from '../../core/expense.service';

export const EXPENSE_CATEGORIES = [
  { value: 1, label: 'Administrative' },
  { value: 2, label: 'Maintenance' },
  { value: 3, label: 'Meeting' },
  { value: 4, label: 'Stationery' },
  { value: 5, label: 'Other' },
];

@Component({
  selector: 'app-add-expense-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './add-expense-dialog.component.html'
})
export class AddExpenseDialogComponent {
  form: FormGroup;
  saving = false;
  error = '';
  categories = EXPENSE_CATEGORIES;

  constructor(
    private fb: FormBuilder,
    private expenseSvc: ExpenseService,
    private dialogRef: MatDialogRef<AddExpenseDialogComponent>,
  ) {
    const today = new Date().toISOString().substring(0, 10);
    this.form = this.fb.group({
      description: ['', [Validators.required, Validators.maxLength(500)]],
      amount:      [null, [Validators.required, Validators.min(0.01)]],
      category:    [null, Validators.required],
      date:        [today, Validators.required],
      groupId:     [null]
    });
  }

  /** Called by parent to inject groupId before opening. */
  setGroupId(id: number) {
    this.form.patchValue({ groupId: id });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const { groupId, description, amount, category, date } = this.form.value;
    this.expenseSvc.addExpense(groupId, { description, amount, category, date }).subscribe({
      next: () => this.dialogRef.close(true),
      error: err => {
        this.error = err?.error?.message ?? 'Failed to add expense.';
        this.saving = false;
      }
    });
  }
}
