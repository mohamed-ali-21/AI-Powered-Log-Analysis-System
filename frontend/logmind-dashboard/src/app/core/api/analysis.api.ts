import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { AiAnalysisQuery, IssueAnalysisDto } from '../models/analysis';
import { PagedResult } from '../models/paged-result';
import { toParams } from './http-utils';

@Injectable({ providedIn: 'root' })
export class AnalysisApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/ai-analysis';

  search(query?: AiAnalysisQuery): Observable<PagedResult<IssueAnalysisDto>> {
    return this.http.get<PagedResult<IssueAnalysisDto>>(this.base, { params: toParams(query) });
  }

  getByIssueId(issueId: string): Observable<IssueAnalysisDto> {
    return this.http.get<IssueAnalysisDto>(`${this.base}/${issueId}`);
  }

  retry(issueId: string): Observable<unknown> {
    return this.http.post(`${this.base}/retry/${issueId}`, {});
  }
}
