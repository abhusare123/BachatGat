import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { AuthResponse } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = environment.apiUrl;
  currentUser = signal<AuthResponse | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  login(phoneNumber: string) {
    return this.http.post<AuthResponse>(`${this.API}/auth/login`, { phoneNumber }).pipe(
      tap(res => this.saveUser(res))
    );
  }

  refresh(): Observable<AuthResponse> {
    const stored = this.loadUser();
    if (!stored?.refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }
    return this.http.post<AuthResponse>(`${this.API}/auth/refresh`, { refreshToken: stored.refreshToken }).pipe(
      tap(res => this.saveUser(res)),
      catchError(err => {
        // Refresh token itself is invalid/expired — must re-login
        this.logout();
        return throwError(() => err);
      })
    );
  }

  // Legacy OTP methods — kept for future SMS integration
  sendOtp(phoneNumber: string) {
    return this.http.post(`${this.API}/auth/send-otp`, { phoneNumber });
  }

  verifyOtp(phoneNumber: string, otp: string, fullName: string) {
    return this.http.post<AuthResponse>(`${this.API}/auth/verify-otp`, { phoneNumber, otp, fullName }).pipe(
      tap(res => this.saveUser(res))
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

  private saveUser(res: AuthResponse) {
    localStorage.setItem('auth', JSON.stringify(res));
    this.currentUser.set(res);
  }

  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem('auth');
    return raw ? JSON.parse(raw) : null;
  }
}
