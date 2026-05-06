import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { IssuesApi } from '../../core/api/issues.api';
import { IssueDto, IssuesQuery } from '../../core/models/issue';
import { PagedResult } from '../../core/models/paged-result';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { PaginationComponent } from '../../shared/components/pagination.component';
import { SpinnerComponent } from '../../shared/components/spinner.component';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';

@Component({
  selector: 'app-issues-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DecimalPipe,
    FormsModule,
    RouterLink,
    EmptyStateComponent,
    PaginationComponent,
    SpinnerComponent,
    RelativeTimePipe
  ],
  template: `
    <div class="shrink-0 mx-auto w-full max-w-7xl px-6 pt-8 pb-5">
      <div class="flex flex-wrap items-end justify-between gap-4 mb-6">
        <div>
          <p class="text-[11px] font-semibold uppercase tracking-[0.2em] text-violet-300/80 mb-1.5">Grouping</p>
          <h1 class="text-2xl font-bold tracking-tight text-white">Issues</h1>
          <p class="mt-1 text-sm text-slate-400">Logs grouped into recurring problems by pattern + service.</p>
        </div>

        <div class="flex items-center gap-1 rounded-lg border border-white/10 bg-black/20 p-1">
          <button type="button" (click)="setStatus(undefined)" [class]="tabClass(undefined)">All</button>
          <button type="button" (click)="setStatus(false)" [class]="tabClass(false)">Pending</button>
          <button type="button" (click)="setStatus(true)" [class]="tabClass(true)">Analyzed</button>
        </div>
      </div>

      <form class="surface rounded-xl p-4 grid gap-3 sm:grid-cols-3" (ngSubmit)="apply()">
        <input type="text" [(ngModel)]="serviceName" name="serviceName" placeholder="Service name"
               class="rounded-lg border border-white/10 bg-black/20 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-600 focus:border-violet-400/50 focus:outline-none focus:ring-2 focus:ring-violet-500/20" />
        <input type="text" [(ngModel)]="pattern" name="pattern" placeholder="Pattern contains..."
               class="rounded-lg border border-white/10 bg-black/20 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-600 focus:border-violet-400/50 focus:outline-none focus:ring-2 focus:ring-violet-500/20" />
        <button type="submit"
                class="rounded-lg bg-violet-500 px-4 py-2 text-sm font-medium text-white shadow-lg shadow-violet-500/30 transition hover:bg-violet-400">
          Search
        </button>
      </form>
    </div>

    <div class="flex-1 min-h-0 overflow-y-auto">
      <div class="mx-auto w-full max-w-7xl px-6 pb-8 space-y-6">
        @if (loading()) {
          <app-spinner />
        } @else if (rows().length === 0) {
          <div class="surface rounded-xl">
            <app-empty-state title="No issues found" description="Once the worker groups logs, they'll appear here." />
          </div>
        } @else {
          <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            @for (issue of rows(); track issue.id) {
              <a [routerLink]="['/issues', issue.id]"
                 class="group relative overflow-hidden rounded-xl border border-white/[0.06] bg-white/[0.02] p-5 transition hover:border-violet-500/30 hover:bg-white/[0.04]">
                <span class="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-violet-500/40 to-transparent opacity-0 group-hover:opacity-100 transition"></span>

                <div class="flex items-start justify-between gap-2 mb-3">
                  <span class="inline-flex items-center rounded-md bg-white/[0.04] px-2 py-0.5 text-[11px] font-medium text-slate-300 ring-1 ring-white/10">
                    {{ issue.serviceName }}
                  </span>
                  @if (issue.isAiProcessed) {
                    <span class="rounded-full bg-cyan-500/10 px-2 py-0.5 text-[9px] font-bold tracking-wider text-cyan-300 ring-1 ring-cyan-500/30">
                      AI ANALYZED
                    </span>
                  } @else {
                    <span class="rounded-full bg-amber-500/10 px-2 py-0.5 text-[9px] font-bold tracking-wider text-amber-300 ring-1 ring-amber-500/30">
                      PENDING
                    </span>
                  }
                </div>

                <h3 class="mb-4 line-clamp-2 min-h-[2.75rem] text-base font-semibold text-slate-100 group-hover:text-white">
                  {{ issue.pattern }}
                </h3>

                <div class="grid grid-cols-3 gap-2 pt-3 border-t border-white/5">
                  <div>
                    <p class="text-[10px] uppercase tracking-wider text-slate-600">Count</p>
                    <p class="mt-0.5 text-sm font-bold text-slate-100">{{ issue.count }}</p>
                  </div>
                  <div>
                    <p class="text-[10px] uppercase tracking-wider text-slate-600">Avg score</p>
                    <p class="mt-0.5 text-sm font-bold text-slate-100">{{ issue.avgScore | number:'1.2-2' }}</p>
                  </div>
                  <div>
                    <p class="text-[10px] uppercase tracking-wider text-slate-600">Last seen</p>
                    <p class="mt-0.5 text-sm font-bold text-slate-100">{{ issue.lastSeen | relativeTime }}</p>
                  </div>
                </div>
              </a>
            }
          </div>

          <div class="surface rounded-xl px-5 py-3">
            <app-pagination
              [page]="result()!.page"
              [pageSize]="result()!.pageSize"
              [totalCount]="result()!.totalCount"
              [totalPages]="result()!.totalPages"
              [hasNext]="result()!.hasNext"
              [hasPrevious]="result()!.hasPrevious"
              (pageChange)="goTo($event)" />
          </div>
        }
      </div>
    </div>
  `
})
export class IssuesPage {
  private readonly api = inject(IssuesApi);

  protected serviceName = '';
  protected pattern = '';
  protected status: boolean | undefined = undefined;

  protected readonly result = signal<PagedResult<IssueDto> | null>(null);
  protected readonly rows = computed(() => this.result()?.items ?? []);
  protected readonly loading = signal(true);

  private page = 1;
  private readonly pageSize = 24;

  constructor() {
    this.load();
  }

  protected apply() { this.page = 1; this.load(); }
  protected goTo(page: number) { this.page = page; this.load(); }

  protected setStatus(status: boolean | undefined) {
    this.status = status;
    this.page = 1;
    this.load();
  }

  protected tabClass(status: boolean | undefined): string {
    const base = 'rounded-md px-3 py-1.5 text-xs font-medium transition';
    return this.status === status
      ? `${base} bg-violet-500/15 text-violet-200 ring-1 ring-violet-500/30`
      : `${base} text-slate-400 hover:text-slate-200`;
  }

  private load() {
    this.loading.set(true);
    const query: IssuesQuery = {
      page: this.page,
      pageSize: this.pageSize,
      serviceName: this.serviceName.trim() || undefined,
      pattern: this.pattern.trim() || undefined,
      isAiProcessed: this.status
    };
    this.api.search(query).subscribe({
      next: (res) => { this.result.set(res); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
