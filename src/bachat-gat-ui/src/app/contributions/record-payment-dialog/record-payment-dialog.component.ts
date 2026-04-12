import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { ContributionService } from '../../core/contribution.service';

@Component({
  selector: 'app-record-payment-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatButtonModule],
  templateUrl: './record-payment-dialog.component.html'
})
export class RecordPaymentDialogComponent {
  form: FormGroup;
  saving = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private contribSvc: ContributionService,
    private dialogRef: MatDialogRef<RecordPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { groupId: number; groupMemberId: number; memberName: string }
  ) {
    const now = new Date();
    const defaultPeriod = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    this.form = this.fb.group({
      period: [defaultPeriod, [Validators.required, Validators.pattern(/^\d{4}-\d{2}$/)]],
      amountPaid: [null, [Validators.required, Validators.min(1)]]
    });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const { period, amountPaid } = this.form.value;
    this.contribSvc.recordContribution(this.data.groupId, this.data.groupMemberId, period, amountPaid).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to record payment.';
        this.saving = false;
      }
    });
  }
}
