export interface AgentSettingsDto {
  model: string;
  apiKeyMasked: string;
  apiKeyConfigured: boolean;
  updatedAt?: string | null;
  source: string;
  availableModels: string[];
}

export interface UpdateAgentSettingsRequest {
  model: string;
  apiKey?: string;
}

export interface TestAgentSettingsRequest {
  model: string;
  apiKey?: string;
}

export interface TestAgentSettingsResponse {
  success: boolean;
  message: string;
  latencyMs?: number | null;
}
