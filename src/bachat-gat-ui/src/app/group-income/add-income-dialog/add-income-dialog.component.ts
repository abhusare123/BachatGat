import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { GroupIncomeService } from '../../core/group-income.service';
import { GroupService } from '../../core/group.service';
import { GroupMember } from '../../core/models';

export const INCOME_CATEGORIES = [
  { value: 1, label: 'Penalty / दंड' },
  { value: 2, label: 'Bank Interest / बँक व्याज' },
  { value: 3, label: 'Other / इतर' },
];

@Component({
  selector: 'app-add-income-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './add-income-dialog.component.html'
})
export class AddIncomeDialogComponent implements OnInit {
  form: FormGroup;
  saving = false;
  error = '';
  categories = INCOME_CATEGORIES;
  members: GroupMember[] = [];
  private groupId!: number;

  get isPenalty(): boolean {
    return this.form.get('category')?.value === 1;
  }

  constructor(
    private fb: FormBuilder,
    private incomeSvc: GroupIncomeService,
    private groupSvc: GroupService,
    private dialogRef: MatDialogRef<AddIncomeDialogComponent>
  ) {
    const today = new Date().toISOString().substring(0, 10);
    this.form = this.fb.group({
      category:      [null, Validators.required],
      description:   ['', [Validators.required, Validators.maxLength(500)]],
      amount:        [null, [Validators.required, Validators.min(0.01)]],
      date:          [today, Validators.required],
      groupMemberId: [null]
    });
  }

  ngOnInit() {
    this.form.get('category')!.valueChanges.subscribe(val => {
      const ctrl = this.form.get('groupMemberId')!;
      if (val === 1) {
        ctrl.setValidators(Validators.required);
      } else {
        ctrl.clearValidators();
        ctrl.setValue(null);
      }
      ctrl.updateValueAndValidity();
    });
  }

  setGroupId(id: number) {
    this.groupId = id;
    this.groupSvc.getGroup(id).subscribe(g => {
      this.members = g.members.filter(m => m.isActive);
    });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const { category, description, amount, date, groupMemberId } = this.form.value;
    this.incomeSvc.addEntry(this.groupId, {
      category, description, amount, date,
      groupMemberId: category === 1 ? groupMemberId : undefined
    }).subscribe({
      next: () => this.dialogRef.close(true),
      error: err => {
        this.error = err?.error?.message ?? 'Failed to add entry.';
        this.saving = false;
      }
    });
  }
}
