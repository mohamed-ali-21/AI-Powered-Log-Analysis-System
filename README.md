# 🧠 LogMind — AI-Powered Log Analysis System

An intelligent observability platform that ingests application logs, groups them into recurring issues, and uses an LLM agent to produce root-cause analyses, impact assessments, severity ratings, and actionable recommendations.

> ⚠️ Active development — learning / portfolio project.

---

## 🚀 Overview

LogMind turns a raw log stream into structured engineering insight.

- A **.NET backend** ingests logs, persists them, and groups them into issues.
- An **in-process AI agent** (Microsoft Semantic Kernel + Groq's OpenAI-compatible API) analyzes each grouped issue and emits structured JSON insight.
- An **Angular dashboard** visualizes the live stream, the grouped issues, and the AI-generated analyses.

Everything runs inside one ASP.NET Core host — no separate agent service to deploy.

---

## 🏗️ Architecture

```
┌────────────────────┐      ┌──────────────────────────────────────┐      ┌─────────────────────┐
│  Angular Frontend  │ ───▶ │            .NET Backend              │ ◀─── │  Apps emitting logs │
│  (logmind-         │      │  ┌────────────┐    ┌──────────────┐  │      └─────────────────────┘
│   dashboard)       │      │  │ LogMin.API │ ── │ EF Core + DB │  │
│                    │      │  └────────────┘    └──────────────┘  │
│  Dashboard / Logs  │      │  ┌──────────────────────────────┐    │
│  Issues / Analysis │      │  │ LogMin.Worker (BG services)  │    │      ┌─────────────────────┐
│  Settings          │      │  │  • LogProcessing             │    │ ───▶ │  Groq API (LLM)     │
│                    │      │  │  • IssueGrouping             │    │      │  OpenAI-compatible  │
│                    │      │  │  • IssueAnalysisAgent (SK)   │    │      └─────────────────────┘
└────────────────────┘      │  └──────────────────────────────┘    │
                            └──────────────────────────────────────┘
```

### Backend projects

- **`LogMin.API`** — ASP.NET Core host. Controllers for logs, issues, AI analyses, and settings. Runs the worker as background services.
- **`LogMin.Application`** — orchestrators, query services, agent settings service, DTOs, abstractions.
- **`LogMin.Infrastructure`** — EF Core, repositories, persistence, migrations.
- **`LogMin.Worker`** — background services (`LogProcessingBackgroundService`, `IssueAnalysisBackgroundService`) and the `IssueAnalysisAgent` built on Microsoft Semantic Kernel.

### Frontend

- **`frontend/logmind-dashboard`** — Angular 21 SPA, standalone components, signals, lazy-loaded routes, Tailwind v4. Sidebar shell with five pages: Dashboard, Logs, Issues, AI Analysis, Settings.

---

## 🔄 Workflow

1. Applications POST logs to `/api/logs`.
2. Logs are persisted in SQL Server.
3. `LogProcessingBackgroundService` scores logs, extracts patterns (e.g. `MemoryExhaustion`, `ConcurrencyIssue`, `IOFailure`), and links them to issues.
4. `IssueGroupingService` aggregates logs sharing a `(pattern, service)` signature into an `Issue` with first-seen / last-seen / count / avg score.
5. `IssueAnalysisBackgroundService` picks up unanalyzed issues and asks the LLM agent for a structured insight.
6. The result is persisted as `IssueAnalysis` (severity, root cause, impact, summary, recommendations, tags).
7. The Angular dashboard reads everything via the API.

---

## 🤖 AI Agent

The agent is **in-process .NET**, no separate Python service.

- **Framework**: Microsoft Semantic Kernel (`Microsoft.SemanticKernel.Connectors.OpenAI`)
- **Provider**: Groq (OpenAI-compatible endpoint at `https://api.groq.com/openai/v1`)
- **Default model**: `llama-3.3-70b-versatile`
- **Configurable from the UI**: open **Settings → AI Agent**, pick a model from the dropdown of supported Groq models, paste your API key, save. Changes apply on the next analysis cycle without a restart.

### Capabilities

- **Pattern extraction** — surface recurring error signatures (`NetworkFailure`, `IOFailure`, `ConcurrencyIssue`, …).
- **Issue grouping** — collapse N occurrences of the same root pattern under one issue with count / avg-score / time window.
- **Root-cause inference** — the agent reasons across sample logs and produces a hypothesis.
- **Impact assessment** — describes the user-facing or system effect.
- **Severity rating** — `Low | Medium | High | Critical` based on score + reasoning.
- **Recommendations** — concrete, actionable next steps.
- **Tagging** — short tags for filtering / search.
- **Summary** — one-line synopsis of the issue.

### Structured output

```json
{
  "issueId": "…",
  "rootCause": "Connection pool exhausted under spike traffic",
  "impact": "All checkout requests fail with 5xx until pool drains",
  "severity": "High",
  "summary": "checkout-api ECONNREFUSED to redis://cache:6379",
  "recommendations": [
    "Increase connection pool size from 10 → 50",
    "Add circuit breaker around redis client",
    "Verify cache health probe before deployment"
  ],
  "tags": ["redis", "connection-pool", "checkout"]
}
```

---

## 🖥️ Frontend pages

| Page | What it shows |
|---|---|
| **Dashboard** | KPI tiles (total logs, open issues, pending AI, total analyses) + recent issues + latest AI insights |
| **Logs** | Table view of every ingested log with service / pattern / score / message / link to parent issue |
| **Issues** | Grid of grouped issues with count, avg score, last-seen, AI-analyzed status, filter tabs |
| **Issue detail** | Full issue metadata, AI insight panel (root cause / impact / recommendations / tags), sample logs, "View all logs" deep-link, "Re-analyze" button |
| **AI Analysis** | Grid of insights filterable by severity |
| **Settings** | Backend API Server URL (per-browser, runtime), AI agent model dropdown + API key + connection test |

---

## 🛠️ Configuration

### Backend API base URL (frontend → backend)

Stored in `localStorage` per browser. An HTTP interceptor rewrites every `/api/*` request to `${apiServerUrl}${path}`. No code-level fallback — if it's empty or unreachable, the request fails with a clear error. Set it in **Settings → Backend API Server**.

### LLM agent

Resolved at runtime per analysis cycle, in this priority order:

1. Database row written via the Settings page (preferred)
2. `appsettings.json` → `IssueAnalysis:Agent:ApiKey`
3. Environment variable `LOGMIND_LLM_API_KEY`

If none of those have a key, the worker logs a clear error and skips the cycle.

### Database

SQL Server. Connection string in `backend/LogMin.API/appsettings.json` under `ConnectionStrings:LogMindDb`. Migrations auto-apply on startup via `db.Database.Migrate()`.

---

## 🚦 Running locally

```bash
# Backend
cd backend
dotnet run --project LogMin.API --launch-profile https
# → API on https://localhost:7246, Swagger at /swagger

# Frontend
cd frontend/logmind-dashboard
npm install
ng serve
# → http://localhost:4200
```

First-run setup:

1. Open `http://localhost:4200`.
2. Go to **Settings → Backend API Server**, enter `https://localhost:7246`, save.
3. Go to **Settings → AI Agent**, pick a model, paste your Groq API key, save (or click **Test connection** first).
4. POST a log via Swagger or `curl`. Within ~30 s the worker groups it and the agent analyzes it.

---

## 🗺️ Roadmap

- [ ] Continuous learning — feed back resolved issues into prompt context
- [ ] Per-user authentication (settings currently global, single-tenant)
- [ ] Native Anthropic provider (currently Groq / OpenAI-compatible only)
- [ ] Metrics export (Prometheus / OpenTelemetry)
- [ ] WebSocket push for live dashboard updates (currently poll-based)
