import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoanService } from '../../core/loan.service';

@Component({
  selector: 'app-loan-request',
  imports: [CommonModule, RouterLink, ReactiveFormsModule, MatInputModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './loan-request.component.html',
  styleUrl: './loan-request.component.scss'
})
export class LoanRequestComponent implements OnInit {
  groupId!: number;
  form: FormGroup;
  saving = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private loanSvc: LoanService
  ) {
    this.form = this.fb.group({
      amount: [null, [Validators.required, Validators.min(1)]],
      tenureMonths: [12, [Validators.required, Validators.min(1), Validators.max(60)]],
      purpose: ['']
    });
  }

  ngOnInit() {
    this.groupId = +this.route.snapshot.paramMap.get('id')!;
  }

  submit() {
    if (this.form.invalid) return;
    this.saving = true;
    const { amount, tenureMonths, purpose } = this.form.value;
    this.loanSvc.requestLoan(this.groupId, amount, tenureMonths, purpose).subscribe({
      next: () => this.router.navigate([`/groups/${this.groupId}/loans`]),
      error: (err) => { this.error = err?.error?.message ?? 'Failed to submit request.'; this.saving = false; }
    });
  }
}
