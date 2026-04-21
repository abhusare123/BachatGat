import { Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { GroupService } from '../../core/group.service';
import { GroupMemberRole } from '../../core/models';

function phoneOrEmailRequired(group: AbstractControl): ValidationErrors | null {
  const phone = group.get('phoneNumber')?.value?.trim();
  const email = group.get('email')?.value?.trim();
  return phone || email ? null : { phoneOrEmailRequired: true };
}

@Component({
  selector: 'app-add-member-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './add-member-dialog.component.html'
})
export class AddMemberDialogComponent {
  form: FormGroup;
  saving = false;
  error = '';
  roles = [
    { value: GroupMemberRole.Member, label: 'Member' },
    { value: GroupMemberRole.Treasurer, label: 'Treasurer' },
    { value: GroupMemberRole.Auditor, label: 'Auditor' },
    { value: GroupMemberRole.Admin, label: 'Admin' }
  ];

  constructor(
    private fb: FormBuilder,
    private groupSvc: GroupService,
    private dialogRef: MatDialogRef<AddMemberDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { groupId: number }
  ) {
    this.form = this.fb.group({
      phoneNumber: ['', Validators.pattern(/^\d{10,15}$/)],
      email: ['', Validators.email],
      fullName: [''],
      role: [GroupMemberRole.Member, Validators.required]
    }, { validators: phoneOrEmailRequired });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const { phoneNumber, email, fullName, role } = this.form.value;
    this.groupSvc.addMember(
      this.data.groupId,
      phoneNumber?.trim() || undefined,
      email?.trim() || undefined,
      role,
      fullName?.trim() || undefined
    ).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to add member.';
        this.saving = false;
      }
    });
  }
}
