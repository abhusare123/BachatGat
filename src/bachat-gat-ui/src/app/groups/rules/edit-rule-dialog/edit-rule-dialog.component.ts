import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ConfigurableRule } from '../../../core/models';
import { GroupRulesService } from '../../../core/group-rules.service';

export interface EditRuleDialogData {
  groupId: number;
  rule: ConfigurableRule;
}

@Component({
  selector: 'app-edit-rule-dialog',
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule
  ],
  templateUrl: './edit-rule-dialog.component.html'
})
export class EditRuleDialogComponent {
  form: FormGroup;
  saving = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private rulesService: GroupRulesService,
    private dialogRef: MatDialogRef<EditRuleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditRuleDialogData
  ) {
    this.form = this.fb.group({
      value: [data.rule.value, [Validators.required, Validators.min(0)]]
    });
  }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    this.rulesService.updateRule(this.data.groupId, this.data.rule.key, { value: this.form.value.value.toString() }).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to update rule';
        this.saving = false;
      }
    });
  }
}
