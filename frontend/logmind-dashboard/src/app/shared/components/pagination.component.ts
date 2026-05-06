import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex items-center justify-between gap-4 pt-4">
      <p class="text-sm text-slate-400">
        Showing <span class="text-slate-200">{{ rangeStart() }}</span>–<span class="text-slate-200">{{ rangeEnd() }}</span>
        of <span class="text-slate-200">{{ totalCount() }}</span>
      </p>
      <div class="flex items-center gap-2">
        <button type="button"
                class="rounded-lg border border-white/10 bg-white/5 px-3 py-1.5 text-sm text-slate-200 transition hover:bg-white/10 disabled:opacity-40 disabled:hover:bg-white/5"
                [disabled]="!hasPrevious()"
                (click)="pageChange.emit(page() - 1)">
          ← Prev
        </button>
        <span class="text-sm text-slate-400">
          Page <span class="text-slate-200">{{ page() }}</span> / {{ totalPages() || 1 }}
        </span>
        <button type="button"
                class="rounded-lg border border-white/10 bg-white/5 px-3 py-1.5 text-sm text-slate-200 transition hover:bg-white/10 disabled:opacity-40 disabled:hover:bg-white/5"
                [disabled]="!hasNext()"
                (click)="pageChange.emit(page() + 1)">
          Next →
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent {
  page = input.required<number>();
  pageSize = input.required<number>();
  totalCount = input.required<number>();
  totalPages = input.required<number>();
  hasNext = input.required<boolean>();
  hasPrevious = input.required<boolean>();

  pageChange = output<number>();

  protected readonly rangeStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1);

  protected readonly rangeEnd = computed(() =>
    Math.min(this.page() * this.pageSize(), this.totalCount()));
}
