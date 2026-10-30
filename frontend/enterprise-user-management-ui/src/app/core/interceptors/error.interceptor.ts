import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred. Please try again.';

      if (error.error && typeof error.error === 'object') {
        if (error.error.message) {
          errorMessage = error.error.message;
        } else if (error.error.errors && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
          errorMessage = error.error.errors.join('; ');
        }
      } else if (error.status === 0) {
        errorMessage = 'Unable to reach the backend API service. Please verify the API is running.';
      } else if (error.status === 404) {
        errorMessage = 'The requested resource was not found.';
      } else if (error.status === 409) {
        errorMessage = 'A conflict occurred. Record already exists.';
      } else if (error.status === 500) {
        errorMessage = 'Internal server error occurred on the API service.';
      }

      notificationService.showError(errorMessage);
      return throwError(() => error);
    })
  );
};

