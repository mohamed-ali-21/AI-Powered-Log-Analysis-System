import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';

import { IssuesApi } from '../../core/api/issues.api';
import { AnalysisApi } from '../../core/api/analysis.api';
import { IssueDetailsDto } from '../../core/models/issue';
import { IssueAnalysisDto } from '../../core/models/analysis';
import { SeverityPillComponent } from '../../shared/components/severity-pill.component';
import { SpinnerComponent } from '../../shared/components/spinner.component';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';

@Component({
  selector: 'app-issue-detail-page',
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
    @if (issue(); as i) {
      <div class="shrink-0 mx-auto w-full max-w-7xl px-6 pt-8 pb-5">
        <a routerLink="/issues" class="inline-flex items-center gap-1 text-xs font-medium text-slate-500 hover:text-slate-200 transition mb-3">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-3 w-3">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
          Back to issues
        </a>

        <div class="flex flex-wrap items-start justify-between gap-4">
          <div class="space-y-2 max-w-3xl">
            <p class="text-[11px] font-semibold uppercase tracking-[0.2em] text-violet-300/80">Issue</p>
            <h1 class="text-2xl font-bold tracking-tight text-white">{{ i.pattern }}</h1>
            <div class="flex flex-wrap items-center gap-2 text-[11px] text-slate-500">
              <span class="rounded-md bg-white/[0.04] px-2 py-0.5 text-slate-300 ring-1 ring-white/10">{{ i.serviceName }}</span>
              <span>·</span>
              <span>{{ i.count }} occurrences</span>
              <span>·</span>
              <span>First seen {{ i.firstSeen | relativeTime }}</span>
              <span>·</span>
              <span>Last seen {{ i.lastSeen | relativeTime }}</span>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <a [routerLink]="['/logs']" [queryParams]="{ issueId: i.id }"
               class="inline-flex items-center gap-1.5 rounded-lg border border-white/10 bg-white/[0.03] px-3 py-1.5 text-xs font-medium text-slate-200 transition hover:bg-white/[0.07]">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-3.5 w-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h10" />
              </svg>
              View all logs
            </a>
            <button type="button" (click)="retry(i.id)"
                    class="inline-flex items-center gap-1.5 rounded-lg border border-violet-500/30 bg-violet-500/10 px-3 py-1.5 text-xs font-medium text-violet-200 transition hover:bg-violet-500/20">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-3.5 w-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h5M20 20v-5h-5M5 19a9 9 0 0 0 14.85-3.36M19 5a9 9 0 0 0-14.85 3.36" />
              </svg>
              Re-analyze
            </button>
          </div>
        </div>
      </div>

      <div class="flex-1 min-h-0 overflow-y-auto">
        <div class="mx-auto w-full max-w-7xl px-6 pb-8 space-y-5">
          <!-- Stats row -->
          <div class="grid gap-4 lg:grid-cols-3">
            <div class="surface rounded-xl p-5">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">Avg Score</p>
              <p class="mt-2 text-3xl font-bold text-white">{{ i.avgScore | number:'1.2-2' }}</p>
              <p class="mt-1 text-[11px] text-slate-500">Severity heuristic</p>
            </div>
            <div class="surface rounded-xl p-5">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">Status</p>
              @if (i.isAiProcessed) {
                <p class="mt-2 text-2xl font-bold text-cyan-300">AI Analyzed</p>
                <p class="mt-1 text-[11px] text-slate-500">Insights available below</p>
              } @else {
                <p class="mt-2 text-2xl font-bold text-amber-300">Pending</p>
                <p class="mt-1 text-[11px] text-slate-500">Worker will pick up shortly</p>
              }
            </div>
            <div class="surface rounded-xl p-5">
              <p class="text-[10px] font-semibold uppercase tracking-wider text-slate-500">Sample Logs</p>
              <p class="mt-2 text-3xl font-bold text-white">{{ i.sampleLogs.length }}</p>
              <p class="mt-1 text-[11px] text-slate-500">Representative entries</p>
            </div>
          </div>

          <!-- AI Insight -->
          @if (analysis(); as a) {
            <section class="surface rounded-xl p-6">
              <div class="flex items-center justify-between gap-3 mb-4">
                <div class="flex items-center gap-2">
                  <span class="grid h-7 w-7 place-items-center rounded-md bg-cyan-500/10 text-cyan-300 ring-1 ring-cyan-500/20">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-3.5 w-3.5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M12 2v4m0 12v4m10-10h-4M6 12H2" />
                    </svg>
                  </span>
                  <h2 class="text-base font-semibold text-white">AI Insight</h2>
                </div>
                <app-severity-pill [severity]="a.severity" />
              </div>

              <p class="text-sm leading-relaxed text-slate-200">{{ a.summary }}</p>

              <div class="mt-5 grid gap-4 md:grid-cols-2">
                <div class="rounded-lg border border-white/5 bg-black/20 p-4">
                  <h3 class="mb-2 text-[10px] font-semibold uppercase tracking-wider text-slate-500">Root cause</h3>
                  <p class="text-sm text-slate-200 leading-relaxed">{{ a.rootCause }}</p>
                </div>
                <div class="rounded-lg border border-white/5 bg-black/20 p-4">
                  <h3 class="mb-2 text-[10px] font-semibold uppercase tracking-wider text-slate-500">Impact</h3>
                  <p class="text-sm text-slate-200 leading-relaxed">{{ a.impact }}</p>
                </div>
              </div>

              @if (a.recommendations.length) {
                <div class="mt-5">
                  <h3 class="mb-3 text-[10px] font-semibold uppercase tracking-wider text-slate-500">Recommendations</h3>
                  <ul class="space-y-2">
                    @for (rec of a.recommendations; track rec) {
                      <li class="flex gap-3 rounded-lg border border-white/5 bg-black/20 px-3 py-2.5">
                        <span class="grid h-5 w-5 shrink-0 place-items-center rounded-full bg-violet-500/15 text-violet-300 text-[10px] font-bold">▸</span>
                        <span class="text-sm text-slate-200 leading-relaxed">{{ rec }}</span>
                      </li>
                    }
                  </ul>
                </div>
              }

              @if (a.tags.length) {
                <div class="mt-5 flex flex-wrap gap-1.5">
                  @for (tag of a.tags; track tag) {
                    <span class="rounded-full bg-violet-500/10 px-2.5 py-0.5 text-[11px] text-violet-200 ring-1 ring-violet-500/30">
                      #{{ tag }}
                    </span>
                  }
                </div>
              }
            </section>
          }

          <!-- Sample logs -->
          <section class="surface rounded-xl p-6">
            <div class="flex items-center gap-2 mb-4">
              <span class="grid h-7 w-7 place-items-center rounded-md bg-violet-500/10 text-violet-300 ring-1 ring-violet-500/20">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-3.5 w-3.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h10" />
                </svg>
              </span>
              <h2 class="text-base font-semibold text-white">Sample logs</h2>
            </div>

            @if (i.sampleLogs.length === 0) {
              <p class="text-sm text-slate-500">No sample logs available.</p>
            } @else {
              <ul class="space-y-2">
                @for (log of i.sampleLogs; track log.id) {
                  <li class="rounded-lg border border-white/5 bg-black/30 p-3">
                    <p class="mb-1 text-[10px] font-mono text-slate-600">{{ log.timestamp | date:'medium' }}</p>
                    <pre class="font-mono text-[12px] leading-relaxed text-slate-200 whitespace-pre-wrap break-words">{{ log.message }}</pre>
                  </li>
                }
              </ul>
            }
          </section>
        </div>
      </div>
    } @else if (loading()) {
      <div class="flex-1 min-h-0"><app-spinner /></div>
    } @else if (notFound()) {
      <div class="mx-auto w-full max-w-7xl px-6 pt-8">
        <p class="text-sm text-slate-500">Issue not found.</p>
      </div>
    }
  `
})
export class IssueDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly issuesApi = inject(IssuesApi);
  private readonly analysisApi = inject(AnalysisApi);

  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly analysis = signal<IssueAnalysisDto | null>(null);

  protected readonly issue = toSignal<IssueDetailsDto | null>(
    this.route.paramMap.pipe(
      switchMap((p) => {
        const id = p.get('id');
        if (!id) return of(null);
        this.loading.set(true);
        this.notFound.set(false);
        return this.issuesApi.getById(id).pipe(
          catchError(() => { this.notFound.set(true); return of(null); })
        );
      })
    ),
    { initialValue: null }
  );

  constructor() {
    this.route.paramMap.subscribe((p) => {
      const id = p.get('id');
      this.analysis.set(null);
      this.loading.set(true);
      if (!id) { this.loading.set(false); return; }
      this.analysisApi.getByIssueId(id).pipe(
        catchError(() => of(null))
      ).subscribe((a) => {
        this.analysis.set(a);
        this.loading.set(false);
      });
    });
  }

  protected retry(id: string) {
    this.analysisApi.retry(id).subscribe();
  }
}
