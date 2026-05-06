export interface IssueDto {
  id: string;
  pattern: string;
  serviceName: string;
  firstSeen: string;
  lastSeen: string;
  count: number;
  avgScore: number;
  isAiProcessed: boolean;
  representativeLogId: string;
}

export interface IssueLogReferenceDto {
  id: string;
  timestamp: string;
  message: string;
}

export interface IssueDetailsDto extends IssueDto {
  sampleLogs: IssueLogReferenceDto[];
  latestAnalysisId?: string | null;
}

export interface IssuesQuery {
  isAiProcessed?: boolean;
  serviceName?: string;
  pattern?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
