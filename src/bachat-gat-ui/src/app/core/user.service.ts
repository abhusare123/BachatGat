import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile, UpdateUserProfileRequest } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMyProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.API}/users/me`);
  }

  updateMyProfile(request: UpdateUserProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.API}/users/me`, request);
  }

  getUserProfile(userId: number): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.API}/users/${userId}`);
  }

  updateUserProfile(userId: number, request: UpdateUserProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.API}/users/${userId}`, request);
  }

  updatePin(currentPin: string | null, newPin: string): Observable<void> {
    return this.http.put<void>(`${this.API}/users/me/pin`, { currentPin, newPin });
  }
}
