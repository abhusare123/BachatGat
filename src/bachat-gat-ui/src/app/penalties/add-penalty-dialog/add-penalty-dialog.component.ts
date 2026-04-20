import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { PenaltyService } from '../../core/penalty.service';
import { GroupService } from '../../core/group.service';
import { GroupMember } from '../../core/models';

@Component({
  selector: 'app-add-penalty-dialog',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './add-penalty-dialog.component.html'
})
export class AddPenaltyDialogComponent implements OnInit {
  form: FormGroup;
  saving = false;
  error = '';
  members: GroupMember[] = [];
  private groupId!: number;

  constructor(
    private fb: FormBuilder,
    private penaltySvc: PenaltyService,
    private groupSvc: GroupService,
    private dialogRef: MatDialogRef<AddPenaltyDialogComponent>
  ) {
    const today = new Date().toISOString().substring(0, 10);
    this.form = this.fb.group({
      memberId: [null, Validators.required],
      amount:   [null, [Validators.required, Validators.min(0.01)]],
      purpose:  ['', [Validators.required, Validators.maxLength(500)]],
      date:     [today, Validators.required]
    });
  }

  ngOnInit() {}

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
    const { memberId, amount, purpose, date } = this.form.value;
    this.penaltySvc.addPenalty(this.groupId, { memberId, amount, purpose, date }).subscribe({
      next: () => this.dialogRef.close(true),
      error: err => {
        this.error = err?.error?.message ?? 'Failed to add penalty.';
        this.saving = false;
      }
    });
  }
}
