import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { AuthResponse } from './models';

// Shared refresh state — prevents multiple concurrent 401s from each
// triggering their own refresh call (only one refresh runs at a time).
let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<AuthResponse | null>(null);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authSvc = inject(AuthService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      // Only intercept 401s; skip auth endpoints to avoid refresh loops
      if (err.status !== 401 || isAuthEndpoint(req.url)) {
        return throwError(() => err);
      }

      if (isRefreshing) {
        // Another request already triggered a refresh — wait for it to finish
        // then retry this request with the new token.
        return refreshDone$.pipe(
          filter(res => res !== null),
          take(1),
          switchMap(res => next(withToken(req, res!.accessToken)))
        );
      }

      isRefreshing = true;
      refreshDone$.next(null);

      return authSvc.refresh().pipe(
        switchMap(res => {
          isRefreshing = false;
          refreshDone$.next(res);
          // Retry the original request with the new access token
          return next(withToken(req, res.accessToken));
        }),
        catchError(refreshErr => {
          isRefreshing = false;
          // refresh() already called logout() internally
          return throwError(() => refreshErr);
        })
      );
    })
  );
};

function isAuthEndpoint(url: string): boolean {
  return url.includes('/auth/login')
      || url.includes('/auth/refresh')
      || url.includes('/auth/send-otp')
      || url.includes('/auth/verify-otp');
}

function withToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
