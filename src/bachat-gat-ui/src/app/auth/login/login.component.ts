import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, MatInputModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule, MatIconModule, MatFormFieldModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10,15}$/)]]
    });
  }

  allowOnlyDigits(event: KeyboardEvent): void {
    if (event.key.length === 1 && (event.key < '0' || event.key > '9')) event.preventDefault();
  }

  login() {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.login(this.form.value.phoneNumber).subscribe({
      next: () => this.router.navigate(['/groups']),
      error: (err) => {
        this.error = err.status === 401
          ? 'Phone number not registered. Contact your group admin to be added.'
          : 'Login failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
