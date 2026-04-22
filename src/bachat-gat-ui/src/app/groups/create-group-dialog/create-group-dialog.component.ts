import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { GroupService } from '../../core/group.service';
import { InterestRateType } from '../../core/models';

@Component({
  selector: 'app-create-group-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './create-group-dialog.component.html'
})
export class CreateGroupDialogComponent {
  form: FormGroup;
  saving = false;
  InterestRateType = InterestRateType;

  rateTypes = [
    { value: InterestRateType.Reducing, label: 'Reducing Balance (Fixed EMI)', hint: 'Fixed EMI; interest on outstanding balance — lower total cost' },
    { value: InterestRateType.EqualPrincipal, label: 'Equal Principal (Decreasing EMI)', hint: 'Fixed principal each month; interest on outstanding — EMI reduces over time' },
    { value: InterestRateType.Fixed, label: 'Flat Rate (Fixed Interest)', hint: 'Interest on original principal every month — same EMI' }
  ];

  constructor(
    private fb: FormBuilder,
    private groupSvc: GroupService,
    private dialogRef: MatDialogRef<CreateGroupDialogComponent>
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      monthlyAmount: [2000, [Validators.required, Validators.min(1)]],
      interestRatePercent: [2, [Validators.required, Validators.min(0)]],
      interestRateType: [InterestRateType.Reducing, Validators.required]
    });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.groupSvc.createGroup(this.form.value).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => this.saving = false
    });
  }
}
