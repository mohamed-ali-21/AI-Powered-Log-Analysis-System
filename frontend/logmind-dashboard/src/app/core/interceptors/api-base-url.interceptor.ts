import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';

import { ApiBaseUrlService } from '../services/api-base-url.service';

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api/')) {
    return next(req);
  }

  const baseUrl = inject(ApiBaseUrlService).url();

  if (!baseUrl) {
    return throwError(() => new HttpErrorResponse({
      status: 0,
      statusText: 'Configuration Required',
      url: req.url,
      error: { message: 'API Server URL is not configured. Open Settings to configure it.' }
    }));
  }

  return next(req.clone({ url: `${baseUrl}${req.url}` }));
};
