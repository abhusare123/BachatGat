import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { GroupService } from '../../core/group.service';
import { GroupMemberRole } from '../../core/models';

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
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      fullName: [''],
      role: [GroupMemberRole.Member, Validators.required]
    });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const { phoneNumber, fullName, role } = this.form.value;
    this.groupSvc.addMember(this.data.groupId, phoneNumber, role, fullName || undefined).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to add member.';
        this.saving = false;
      }
    });
  }
}
