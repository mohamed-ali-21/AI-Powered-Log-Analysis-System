export type Severity = 'Low' | 'Medium' | 'High' | 'Critical';

export interface IssueAnalysisDto {
  id: string;
  issueId: string;
  rootCause?: string | null;
  impact?: string | null;
  severity?: Severity | string | null;
  summary?: string | null;
  recommendations: string[];
  tags: string[];
  aiConfidence?: number | null;
  createdAt: string;
}

export interface AiAnalysisQuery {
  severity?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
