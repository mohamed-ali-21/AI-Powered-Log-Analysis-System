import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center gap-3 py-16 text-center">
      <div class="grid h-14 w-14 place-items-center rounded-full bg-white/5 ring-1 ring-white/10">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="1.5" class="h-6 w-6 text-slate-300">
          <path stroke-linecap="round" stroke-linejoin="round"
                d="M9 12h6m-6 4h4M5 8h14M5 8a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2m-14 0v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8" />
        </svg>
      </div>
      <h3 class="text-base font-semibold text-slate-100">{{ title() }}</h3>
      @if (description()) {
        <p class="max-w-md text-sm text-slate-400">{{ description() }}</p>
      }
    </div>
  `
})
export class EmptyStateComponent {
  title = input<string>('Nothing here yet');
  description = input<string | null>(null);
}
