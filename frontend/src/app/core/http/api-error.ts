import { HttpErrorResponse } from '@angular/common/http';

import { ApiResponse } from '../models/api-response.model';

export function apiErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse) {
    const responseError = error.error;

    if (typeof responseError === 'string' && responseError.trim()) {
      return responseError;
    }

    const apiResponse = responseError as Partial<ApiResponse<unknown>> | null;
    const errors = apiResponse?.errors?.filter(Boolean);

    if (errors?.length) {
      return errors.join('\n');
    }

    if (apiResponse?.message) {
      return apiResponse.message;
    }

    if (error.status === 401) {
      return 'Please sign in again before continuing.';
    }

    if (error.status === 0) {
      return 'Cannot reach the API. Make sure the backend is running.';
    }

    return fallback;
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallback;
}
