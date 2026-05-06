import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  AgentSettingsDto,
  TestAgentSettingsRequest,
  TestAgentSettingsResponse,
  UpdateAgentSettingsRequest
} from '../models/settings';

@Injectable({ providedIn: 'root' })
export class SettingsApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/settings/agent';

  getAgent(): Observable<AgentSettingsDto> {
    return this.http.get<AgentSettingsDto>(this.base);
  }

  updateAgent(request: UpdateAgentSettingsRequest): Observable<AgentSettingsDto> {
    return this.http.put<AgentSettingsDto>(this.base, request);
  }

  testAgent(request: TestAgentSettingsRequest): Observable<TestAgentSettingsResponse> {
    return this.http.post<TestAgentSettingsResponse>(`${this.base}/test`, request);
  }
}
