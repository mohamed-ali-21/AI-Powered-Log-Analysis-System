# 🧠 AI-Powered Log Analysis System

An intelligent observability platform that leverages AI agents to automatically analyze application logs, detect anomalies, and perform root cause analysis.

> ⚠️ This project is currently under active development.

---

## 🚀 Overview

This project simulates a modern monitoring system enhanced with AI capabilities.  
It combines a **.NET backend**, an **AI Agent built using Google ADK**, and an **Angular dashboard** to provide automated debugging insights.

The system goes beyond traditional logging tools by introducing **agent-based reasoning** to understand and explain system failures.

---

## 🏗️ Architecture

The system follows a modular architecture with clear separation of concerns:

- **.NET Backend (Core System)**
  - Log ingestion API
  - Data storage and management
  - Orchestration of AI workflows
  - Background processing (Worker Service)

- **AI Agent (Google ADK)**
  - Log analysis and clustering
  - Root cause analysis
  - Anomaly detection
  - Intelligent insights generation

- **Angular Frontend**
  - Dashboard for log visualization
  - Error trends and analytics
  - AI-generated insights display

---

## 🔄 Workflow

1. Applications send error logs to the .NET API  
2. Logs are stored in the database  
3. Background worker retrieves logs periodically  
4. Logs are sent to the AI Agent for analysis  
5. The agent processes logs using multi-step reasoning  
6. Structured insights are returned and stored  
7. Insights are displayed on the Angular dashboard  

---

## 🧠 AI Agent Capabilities

The AI Agent transforms raw logs into meaningful insights using advanced analysis techniques:

### 🔍 Log Processing & Understanding
- Parse and normalize structured/unstructured logs  
- Extract key metadata (service, endpoint, exception, timestamp, traceId)  
- Preprocess logs for analysis  

---

### 📊 Error Grouping & Clustering
- Group similar errors by patterns and stack traces  
- Aggregate repeated issues with occurrence counts  
- Detect recurring error signatures  

---

### 📈 Trend & Pattern Analysis
- Analyze error frequency over time  
- Detect time-based patterns and trends  
- Identify affected services and endpoints  

---

### ⚠️ Anomaly & Spike Detection
- Detect sudden spikes in error rates  
- Identify abnormal system behavior  
- Highlight critical incidents  

---

### 🔗 Correlation Analysis
- Correlate errors with:
  - Services  
  - API endpoints  
  - Time windows  
- Identify relationships between failures  

---

### 🧩 Root Cause Analysis
- Infer potential root causes of issues  
- Provide reasoning based on log patterns  
- Explain why errors are occurring  

---

### 💡 Intelligent Insights & Summarization
- Generate human-readable summaries  
- Convert large log volumes into concise insights  
- Highlight key system issues  

---

### 🚨 Issue Prioritization
- Rank issues based on impact and frequency  
- Classify severity (High / Medium / Low)  

---

### 🛠️ Suggested Fixes
- Recommend possible solutions  
- Provide actionable debugging steps  
- Suggest performance improvements  

---

### 📊 Confidence Scoring
- Assign confidence levels to analysis results  
- Indicate uncertainty in root cause detection  

---

### 🧠 Context-Aware Analysis (Advanced)
- Detect issues related to deployments  
- Understand service dependencies  
- Track issue evolution over time  

---

### 🔁 Continuous Learning (Future)
- Learn from previously resolved issues  
- Improve analysis accuracy  
- Build a knowledge base  

---

### 📦 Structured Output Example

```json
{
  "issue": "Database timeout in OrderService",
  "count": 120,
  "affectedEndpoints": ["/api/orders"],
  "rootCause": "High database latency",
  "confidence": 0.85,
  "suggestedFix": "Optimize queries and increase connection pool",
  "severity": "High"
}
