import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from './auth.store';

/**
 * Attaches the JWT to every outgoing request, and reacts to a 401 by
 * clearing the (now-invalid) stored session and sending the user back to
 * login - without this, an expired access token left the app stuck
 * showing "logged in" in the navbar while every API call silently 401'd
 * with no visible way to recover short of manually clearing localStorage.
 * A full silent-refresh-and-retry flow is a further improvement; this is
 * the minimum needed so the app never sits in that broken half-logged-in
 * state.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const token = auth.getAccessToken();

  const request = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && !req.url.includes('/auth/login')) {
        auth.clearSession();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
