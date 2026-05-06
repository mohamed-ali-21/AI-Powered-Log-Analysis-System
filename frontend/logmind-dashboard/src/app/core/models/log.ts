export interface LogDto {
  id: string;
  message: string;
  stackTrace?: string | null;
  serviceName: string;
  timestamp: string;
  processed: boolean;
  processedAt?: string | null;
  pattern?: string | null;
  score?: number | null;
  issueId?: string | null;
}

export interface LogsQuery {
  serviceName?: string;
  pattern?: string;
  issueId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
