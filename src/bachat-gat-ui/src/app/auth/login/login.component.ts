import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { AuthService } from '../../core/auth.service';
import { FirebaseService } from '../../core/firebase.service';

function pinMatch(group: AbstractControl): ValidationErrors | null {
  const pin = group.get('pin')?.value;
  const confirmPin = group.get('confirmPin')?.value;
  return pin && confirmPin && pin !== confirmPin ? { pinMismatch: true } : null;
}

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule, MatCardModule,
    MatProgressSpinnerModule, MatIconModule,
    MatInputModule, MatFormFieldModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  mode: 'login' | 'register' = 'login';
  loading = false;
  error = '';

  pinLoginForm: FormGroup;
  registerForm: FormGroup;

  constructor(
    private auth: AuthService,
    private firebase: FirebaseService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.pinLoginForm = this.fb.group({
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10,15}$/)]],
      pin: ['', [Validators.required, Validators.pattern(/^\d{4,6}$/)]]
    });

    this.registerForm = this.fb.group({
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10,15}$/)]],
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      pin: ['', [Validators.required, Validators.pattern(/^\d{4,6}$/)]],
      confirmPin: ['', Validators.required]
    }, { validators: pinMatch });
  }

  async signInWithGoogle() {
    this.loading = true;
    this.error = '';
    try {
      const idToken = await this.firebase.signInWithGoogle();
      this.auth.firebaseLogin(idToken).subscribe({
        next: () => this.router.navigate(['/groups']),
        error: () => {
          this.error = 'Sign-in failed. Please try again.';
          this.loading = false;
        }
      });
    } catch {
      this.error = 'Google sign-in was cancelled or failed.';
      this.loading = false;
    }
  }

  loginWithPin() {
    if (this.pinLoginForm.invalid) return;
    this.loading = true;
    this.error = '';
    const { phoneNumber, pin } = this.pinLoginForm.value;
    this.auth.loginWithPin(phoneNumber, pin).subscribe({
      next: () => this.router.navigate(['/groups']),
      error: () => {
        this.error = 'Invalid mobile number or PIN.';
        this.loading = false;
      }
    });
  }

  register() {
    if (this.registerForm.invalid) return;
    this.loading = true;
    this.error = '';
    const { phoneNumber, fullName, pin } = this.registerForm.value;
    this.auth.registerWithPin(phoneNumber, fullName, pin).subscribe({
      next: () => this.router.navigate(['/groups']),
      error: (err) => {
        this.error = err.status === 409
          ? 'This mobile number is already registered. Please sign in.'
          : 'Registration failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
