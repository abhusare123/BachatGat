import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { GroupService } from '../../core/group.service';

@Component({
  selector: 'app-create-group-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatButtonModule],
  templateUrl: './create-group-dialog.component.html'
})
export class CreateGroupDialogComponent {
  form: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private groupSvc: GroupService,
    private dialogRef: MatDialogRef<CreateGroupDialogComponent>
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      monthlyAmount: [2000, [Validators.required, Validators.min(1)]],
      interestRatePercent: [2, [Validators.required, Validators.min(0)]]
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
