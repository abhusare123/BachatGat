import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { AuthResponse } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = environment.apiUrl;
  currentUser = signal<AuthResponse | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  sendOtp(phoneNumber: string) {
    return this.http.post(`${this.API}/auth/send-otp`, { phoneNumber });
  }

  verifyOtp(phoneNumber: string, otp: string, fullName: string) {
    return this.http.post<AuthResponse>(`${this.API}/auth/verify-otp`, { phoneNumber, otp, fullName }).pipe(
      tap(res => {
        localStorage.setItem('auth', JSON.stringify(res));
        this.currentUser.set(res);
      })
    );
  }

  logout() {
    localStorage.removeItem('auth');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.loadUser()?.accessToken ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem('auth');
    return raw ? JSON.parse(raw) : null;
  }
}
