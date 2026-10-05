import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '@core/services/auth/auth.service';
import { TokenService } from '@core/services/token/token.service';

let refreshInFlight: Promise<void> | null = null;

function refreshSession(authService: AuthService): Promise<void> {
  if (!refreshInFlight) {
    refreshInFlight = authService.refresh().finally(() => {
      refreshInFlight = null;
    });
  }

  return refreshInFlight;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const authService = inject(AuthService);
  const token = tokenService.accessToken;

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && tokenService.refreshToken && !req.url.includes('/Auth/Refresh')) {
        return from(refreshSession(authService)).pipe(
          switchMap(() => {
            const newToken = tokenService.accessToken;
            const retry = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
            return next(retry);
          }),
          catchError(refreshErr => {
            tokenService.clear();
            return throwError(() => refreshErr);
          }),
        );
      }
      return throwError(() => error);
    }),
  );
};
