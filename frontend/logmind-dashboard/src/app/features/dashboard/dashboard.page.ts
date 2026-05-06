import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { IssuesApi } from '../../core/api/issues.api';
import { LogsApi } from '../../core/api/logs.api';
import { AnalysisApi } from '../../core/api/analysis.api';
import { SeverityPillComponent } from '../../shared/components/severity-pill.component';
import { SpinnerComponent } from '../../shared/components/spinner.component';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { IssueDto } from '../../core/models/issue';
import { IssueAnalysisDto } from '../../core/models/analysis';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    SeverityPillComponent,
    SpinnerComponent,
    RelativeTimePipe
  ],
  template: `
    <div class="shrink-0 mx-auto w-full max-w-7xl px-6 pt-8 pb-4">
      <div class="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-[0.2em] text-violet-300/80 mb-1.5">Overview</p>
          <h1 class="text-3xl font-bold tracking-tight text-white">
            <span class="gradient-text">Intelligence</span> Dashboard
          </h1>
          <p class="mt-1 max-w-2xl text-sm text-slate-400">
            Real-time view of ingested logs, grouped issues, and AI-generated insights across your services.
          </p>
        </div>
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-y-auto">
      <div class="mx-auto w-full max-w-7xl px-6 pb-8 space-y-6">
        @if (loading()) {
          <app-spinner />
        } @else {
          <!-- KPI tiles -->
          <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div class="surface rounded-xl p-5 relative overflow-hidden group hover:border-violet-500/30 transition">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">Total Logs</p>
                  <p class="mt-2 text-3xl font-bold text-white">{{ totalLogs() | number }}</p>
                  <p class="mt-1 text-[11px] text-emerald-400">● Streaming live</p>
                </div>
                <div class="grid h-9 w-9 place-items-center rounded-lg bg-violet-500/10 text-violet-300 ring-1 ring-violet-500/20">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" class="h-4 w-4">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h10" />
                  </svg>
                </div>
              </div>
            </div>

            <div class="surface rounded-xl p-5 relative overflow-hidden hover:border-violet-500/30 transition">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">Open Issues</p>
                  <p class="mt-2 text-3xl font-bold text-white">{{ totalIssues() | number }}</p>
                  <p class="mt-1 text-[11px] text-slate-400">Last 24h grouped</p>
                </div>
                <div class="grid h-9 w-9 place-items-center rounded-lg bg-amber-500/10 text-amber-300 ring-1 ring-amber-500/20">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" class="h-4 w-4">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v4m0 4h.01M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0Z" />
                  </svg>
                </div>
              </div>
            </div>

            <div class="surface rounded-xl p-5 relative overflow-hidden hover:border-violet-500/30 transition">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">Pending AI</p>
                  <p class="mt-2 text-3xl font-bold text-amber-300">{{ pending() | number }}</p>
                  <p class="mt-1 text-[11px] text-slate-400">Worker picks up next cycle</p>
                </div>
                <div class="grid h-9 w-9 place-items-center rounded-lg bg-orange-500/10 text-orange-300 ring-1 ring-orange-500/20">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" class="h-4 w-4">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 2m6-2a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                  </svg>
                </div>
              </div>
            </div>

            <div class="surface rounded-xl p-5 relative overflow-hidden hover:border-violet-500/30 transition">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">AI Analyses</p>
                  <p class="mt-2 text-3xl font-bold text-white">{{ totalAnalyses() | number }}</p>
                  <p class="mt-1 text-[11px] text-cyan-300">Insights generated</p>
                </div>
                <div class="grid h-9 w-9 place-items-center rounded-lg bg-cyan-500/10 text-cyan-300 ring-1 ring-cyan-500/20">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" class="h-4 w-4">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 2v4m0 12v4m10-10h-4M6 12H2m15.07-7.07-2.83 2.83M9.76 14.24l-2.83 2.83m0-12.14 2.83 2.83m4.48 4.48 2.83 2.83" />
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <!-- Two columns: Recent Issues + Latest Insights -->
          <div class="grid gap-5 lg:grid-cols-2">
            <section class="surface rounded-xl p-5 flex flex-col">
              <div class="flex items-center justify-between mb-4">
                <div>
                  <h2 class="text-base font-semibold text-white">Recent Issues</h2>
                  <p class="text-[11px] text-slate-500">Most recently grouped patterns</p>
                </div>
                <a routerLink="/issues" class="text-xs font-medium text-violet-300 hover:text-violet-200 transition">View all →</a>
              </div>
              <ul class="space-y-2 flex-1">
                @for (issue of recentIssues(); track issue.id) {
                  <li>
                    <a [routerLink]="['/issues', issue.id]"
                       class="group flex items-start justify-between gap-4 rounded-lg border border-transparent px-3 py-2.5 hover:border-white/10 hover:bg-white/[0.03] transition">
                      <div class="min-w-0 flex-1">
                        <p class="truncate text-sm font-medium text-slate-100 group-hover:text-white">{{ issue.pattern }}</p>
                        <p class="mt-0.5 truncate text-[11px] text-slate-500">
                          {{ issue.serviceName }} · {{ issue.count }} occurrences
                        </p>
                      </div>
                      <div class="flex flex-col items-end gap-1 shrink-0">
                        @if (issue.isAiProcessed) {
                          <span class="rounded-full bg-cyan-500/10 px-2 py-0.5 text-[9px] font-bold tracking-wider text-cyan-300 ring-1 ring-cyan-500/30">AI</span>
                        } @else {
                          <span class="rounded-full bg-slate-500/10 px-2 py-0.5 text-[9px] font-bold tracking-wider text-slate-400 ring-1 ring-slate-500/30">PENDING</span>
                        }
                        <span class="text-[10px] text-slate-600">{{ issue.lastSeen | relativeTime }}</span>
                      </div>
                    </a>
                  </li>
                } @empty {
                  <li class="py-8 text-center text-sm text-slate-500">No issues yet.</li>
                }
              </ul>
            </section>

            <section class="surface rounded-xl p-5 flex flex-col">
              <div class="flex items-center justify-between mb-4">
                <div>
                  <h2 class="text-base font-semibold text-white">Latest AI Insights</h2>
                  <p class="text-[11px] text-slate-500">Auto-generated by the LogMind agent</p>
                </div>
                <a routerLink="/analysis" class="text-xs font-medium text-violet-300 hover:text-violet-200 transition">View all →</a>
              </div>
              <ul class="space-y-2.5 flex-1">
                @for (analysis of recentAnalyses(); track analysis.id) {
                  <li class="rounded-lg border border-white/5 bg-black/20 p-3">
                    <div class="mb-2 flex items-center justify-between gap-2">
                      <app-severity-pill [severity]="analysis.severity" />
                      <span class="text-[10px] text-slate-600">{{ analysis.createdAt | date:'short' }}</span>
                    </div>
                    <p class="text-sm font-medium text-slate-100">{{ analysis.summary }}</p>
                    @if (analysis.rootCause) {
                      <p class="mt-1 line-clamp-2 text-[11px] text-slate-500">{{ analysis.rootCause }}</p>
                    }
                  </li>
                } @empty {
                  <li class="py-8 text-center text-sm text-slate-500">No AI analyses yet.</li>
                }
              </ul>
            </section>
          </div>
        }
      </div>
    </div>
  `
})
export class DashboardPage {
  private readonly logs = inject(LogsApi);
  private readonly issues = inject(IssuesApi);
  private readonly analyses = inject(AnalysisApi);

  protected readonly loading = signal(true);
  protected readonly totalLogs = signal(0);
  protected readonly totalIssues = signal(0);
  protected readonly pending = signal(0);
  protected readonly totalAnalyses = signal(0);
  protected readonly recentIssues = signal<IssueDto[]>([]);
  protected readonly recentAnalyses = signal<IssueAnalysisDto[]>([]);

  constructor() {
    forkJoin({
      logs: this.logs.search({ page: 1, pageSize: 1 }),
      issues: this.issues.search({ page: 1, pageSize: 5 }),
      pending: this.issues.search({ page: 1, pageSize: 1, isAiProcessed: false }),
      analyses: this.analyses.search({ page: 1, pageSize: 5 })
    }).subscribe({
      next: ({ logs, issues, pending, analyses }) => {
        this.totalLogs.set(logs.totalCount);
        this.totalIssues.set(issues.totalCount);
        this.pending.set(pending.totalCount);
        this.totalAnalyses.set(analyses.totalCount);
        this.recentIssues.set(issues.items);
        this.recentAnalyses.set(analyses.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
