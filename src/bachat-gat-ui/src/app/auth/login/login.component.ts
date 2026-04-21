import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth.service';
import { FirebaseService } from '../../core/firebase.service';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    MatButtonModule, MatCardModule,
    MatProgressSpinnerModule, MatIconModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loading = false;
  error = '';

  constructor(
    private auth: AuthService,
    private firebase: FirebaseService,
    private router: Router
  ) {}

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
}
