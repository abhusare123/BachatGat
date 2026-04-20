import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { LoanService } from '../../core/loan.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { GroupMember, GroupMemberRole } from '../../core/models';

@Component({
  selector: 'app-loan-request',
  imports: [CommonModule, RouterLink, ReactiveFormsModule, MatInputModule, MatButtonModule,
    MatCardModule, MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule, MatSelectModule],
  templateUrl: './loan-request.component.html',
  styleUrl: './loan-request.component.scss'
})
export class LoanRequestComponent implements OnInit {
  groupId!: number;
  form: FormGroup;
  saving = false;
  error = '';
  isAdminOrTreasurer = false;
  today = new Date();
  members: GroupMember[] = [];
  currentUserId!: number;
  currentUserName = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private loanSvc: LoanService,
    private groupSvc: GroupService,
    private authSvc: AuthService
  ) {
    this.form = this.fb.group({
      amount: [null, [Validators.required, Validators.min(1)]],
      tenureMonths: [12, [Validators.required, Validators.min(1), Validators.max(60)]],
      purpose: [''],
      loanDate: [null],
      borrowerId: [null]
    });
  }

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);

    this.currentUserId = this.authSvc.currentUser()!.userId;
    this.currentUserName = this.authSvc.currentUser()!.fullName;
    this.groupSvc.getGroup(this.groupId).subscribe(g => {
      const me = g.members.find(m => m.userId === this.currentUserId);
      this.isAdminOrTreasurer = !!me && (me.role === GroupMemberRole.Admin || me.role === GroupMemberRole.Treasurer);
      this.members = g.members.filter(m => m.isActive && m.role !== GroupMemberRole.Auditor && m.userId !== this.currentUserId);
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.saving = true;
    const { amount, tenureMonths, purpose, loanDate, borrowerId } = this.form.value;
    this.loanSvc.requestLoan(this.groupId, amount, tenureMonths, purpose, loanDate ?? undefined, borrowerId ?? undefined).subscribe({
      next: () => this.router.navigate([`/groups/${this.groupId}/loans`]),
      error: (err) => { this.error = err?.error?.message ?? 'Failed to submit request.'; this.saving = false; }
    });
  }
}
