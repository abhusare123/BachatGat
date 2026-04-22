import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { UserService } from '../../core/user.service';
import { AuthService } from '../../core/auth.service';
import { UserProfile } from '../../core/models';

function pinMatch(group: AbstractControl): ValidationErrors | null {
  const np = group.get('newPin')?.value;
  const cp = group.get('confirmPin')?.value;
  return np && cp && np !== cp ? { pinMismatch: true } : null;
}

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
    MatExpansionModule,
    MatChipsModule,
    MatDividerModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  profile: UserProfile | null = null;
  form!: FormGroup;
  pinForm!: FormGroup;
  loading = true;
  saving = false;
  savingPin = false;

  /** userId from route param (admin editing another user), or null = own profile */
  targetUserId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private userService: UserService,
    public auth: AuthService,
    private snackBar: MatSnackBar,
    private location: Location
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('userId');
    this.targetUserId = idParam ? +idParam : null;

    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      phoneNumber: ['', Validators.pattern(/^\d{10,15}$/)],
      email: ['', [Validators.email, Validators.maxLength(200)]],
      address: ['', Validators.maxLength(500)]
    });

    this.pinForm = this.fb.group({
      currentPin: [''],
      newPin: ['', [Validators.required, Validators.pattern(/^\d{4,6}$/)]],
      confirmPin: ['', Validators.required]
    }, { validators: pinMatch });

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
          phoneNumber: p.phoneNumber ?? '',
          email: p.email ?? '',
          address: p.address ?? ''
        });
        if (p.hasPin) {
          this.pinForm.get('currentPin')!.setValidators([Validators.required, Validators.pattern(/^\d{4,6}$/)]);
          this.pinForm.get('currentPin')!.updateValueAndValidity();
        }
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
      phoneNumber: this.form.value.phoneNumber || null,
      email: this.form.value.email || null,
      address: this.form.value.address || null
    };

    const obs = this.targetUserId
      ? this.userService.updateUserProfile(this.targetUserId, request)
      : this.userService.updateMyProfile(request);

    obs.subscribe({
      next: () => {
        this.saving = false;
        this.snackBar.open('Profile updated successfully', 'Close', { duration: 3000 });
        this.location.back();
      },
      error: (err) => {
        this.saving = false;
        const msg = err?.error?.message ?? 'Failed to save profile';
        this.snackBar.open(msg, 'Close', { duration: 4000 });
      }
    });
  }

  savePin(): void {
    if (this.pinForm.invalid) return;
    this.savingPin = true;
    const { currentPin, newPin } = this.pinForm.value;

    this.userService.updatePin(this.profile?.hasPin ? currentPin : null, newPin).subscribe({
      next: () => {
        this.savingPin = false;
        if (this.profile) this.profile = { ...this.profile, hasPin: true };
        this.pinForm.reset();
        this.snackBar.open('PIN updated successfully', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.savingPin = false;
        const msg = err?.error?.message ?? 'Failed to update PIN';
        this.snackBar.open(msg, 'Close', { duration: 4000 });
      }
    });
  }

  cancel(): void {
    this.location.back();
  }
}
