import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { IssueDetailsDto, IssueDto, IssuesQuery } from '../models/issue';
import { PagedResult } from '../models/paged-result';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class IssuesApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/issues';

  search(query?: IssuesQuery): Observable<PagedResult<IssueDto>> {
    return this.http.get<PagedResult<IssueDto>>(this.base, { params: toParams(query) });
  }

  getById(id: string): Observable<IssueDetailsDto> {
    return this.http.get<IssueDetailsDto>(`${this.base}/${id}`);
  }
}
