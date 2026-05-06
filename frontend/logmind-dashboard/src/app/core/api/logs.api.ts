import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { LogDto, LogsQuery } from '../models/log';
import { PagedResult } from '../models/paged-result';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class LogsApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/logs';

  search(query?: LogsQuery): Observable<PagedResult<LogDto>> {
    return this.http.get<PagedResult<LogDto>>(this.base, { params: toParams(query) });
  }

  getById(id: string): Observable<LogDto> {
    return this.http.get<LogDto>(`${this.base}/${id}`);
  }
}
