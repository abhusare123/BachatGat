import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../core/user.service';
import { AuthService } from '../../core/auth.service';
import { UserProfile } from '../../core/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  profile: UserProfile | null = null;
  form!: FormGroup;
  loading = true;
  saving = false;

  /** userId from route param (admin editing another user), or null = own profile */
  targetUserId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private userService: UserService,
    public auth: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('userId');
    this.targetUserId = idParam ? +idParam : null;

    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.email, Validators.maxLength(200)]],
      address: ['', Validators.maxLength(500)]
    });

    this.loadProfile();
  }

  private loadProfile(): void {
    const obs = this.targetUserId
      ? this.userService.getUserProfile(this.targetUserId)
      : this.userService.getMyProfile();

    obs.subscribe({
      next: (p) => {
        this.profile = p;
        this.form.patchValue({
          fullName: p.fullName,
          email: p.email ?? '',
          address: p.address ?? ''
        });
        this.loading = false;
      },
      error: () => {
        this.snackBar.open('Failed to load profile', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;

    const request = {
      fullName: this.form.value.fullName,
      email: this.form.value.email || null,
      address: this.form.value.address || null
    };

    const obs = this.targetUserId
      ? this.userService.updateUserProfile(this.targetUserId, request)
      : this.userService.updateMyProfile(request);

    obs.subscribe({
      next: (updated) => {
        this.profile = updated;
        this.saving = false;
        this.snackBar.open('Profile updated successfully', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.saving = false;
        const msg = err?.error?.message ?? 'Failed to save profile';
        this.snackBar.open(msg, 'Close', { duration: 4000 });
      }
    });
  }
}
