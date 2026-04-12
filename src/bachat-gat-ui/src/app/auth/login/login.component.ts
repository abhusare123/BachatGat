import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, MatInputModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  phoneForm: FormGroup;
  otpForm: FormGroup;
  step: 'phone' | 'otp' = 'phone';
  loading = false;
  error = '';

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.phoneForm = this.fb.group({
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]]
    });
    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
      fullName: ['']
    });
  }

  sendOtp() {
    if (this.phoneForm.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.sendOtp(this.phoneForm.value.phoneNumber).subscribe({
      next: () => { this.step = 'otp'; this.loading = false; },
      error: () => { this.error = 'Failed to send OTP. Try again.'; this.loading = false; }
    });
  }

  verifyOtp() {
    if (this.otpForm.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.verifyOtp(
      this.phoneForm.value.phoneNumber,
      this.otpForm.value.otp,
      this.otpForm.value.fullName
    ).subscribe({
      next: () => this.router.navigate(['/groups']),
      error: () => { this.error = 'Invalid or expired OTP.'; this.loading = false; }
    });
  }
}
