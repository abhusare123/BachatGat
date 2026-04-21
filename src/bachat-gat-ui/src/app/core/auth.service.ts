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

  firebaseLogin(idToken: string) {
    return this.http.post<AuthResponse>(`${this.API}/auth/firebase`, { idToken }).pipe(
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
