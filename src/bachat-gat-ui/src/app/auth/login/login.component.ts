import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth.service';

const OTP_RESEND_SECONDS = 30;

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, MatInputModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnDestroy {
  phoneForm: FormGroup;
  otpForm: FormGroup;
  step: 'phone' | 'otp' = 'phone';
  loading = false;
  error = '';
  resendCountdown = 0;
  private countdownInterval: ReturnType<typeof setInterval> | null = null;

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
      next: () => { this.step = 'otp'; this.loading = false; this.startResendTimer(); },
      error: () => { this.error = 'Failed to send OTP. Try again.'; this.loading = false; }
    });
  }

  resendOtp() {
    if (this.resendCountdown > 0) return;
    this.loading = true;
    this.error = '';
    this.otpForm.get('otp')?.reset();
    this.auth.sendOtp(this.phoneForm.value.phoneNumber).subscribe({
      next: () => { this.loading = false; this.startResendTimer(); },
      error: () => { this.error = 'Failed to resend OTP. Try again.'; this.loading = false; }
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

  private startResendTimer() {
    this.clearTimer();
    this.resendCountdown = OTP_RESEND_SECONDS;
    this.countdownInterval = setInterval(() => {
      this.resendCountdown--;
      if (this.resendCountdown <= 0) this.clearTimer();
    }, 1000);
  }

  private clearTimer() {
    if (this.countdownInterval !== null) {
      clearInterval(this.countdownInterval);
      this.countdownInterval = null;
    }
  }

  ngOnDestroy() {
    this.clearTimer();
  }
}
