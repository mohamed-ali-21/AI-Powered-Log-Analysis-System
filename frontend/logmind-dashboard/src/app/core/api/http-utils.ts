import { HttpParams } from '@angular/common/http';

export function toParams(query: object | undefined): HttpParams {
  let params = new HttpParams();
  if (!query) return params;

  for (const [key, value] of Object.entries(query)) {
    if (value === undefined || value === null || value === '') continue;
    params = params.set(key, String(value));
  }
  return params;
}
