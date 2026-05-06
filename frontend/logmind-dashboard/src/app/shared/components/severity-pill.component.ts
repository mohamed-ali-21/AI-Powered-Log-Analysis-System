import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-severity-pill',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span [class]="cls()">
      <span class="h-1.5 w-1.5 rounded-full" [style.background]="dot()"></span>
      {{ label() }}
    </span>
  `,
  styles: [`:host { display: inline-flex; }`]
})
export class SeverityPillComponent {
  severity = input<string | null | undefined>(null);

  protected readonly label = computed(() => this.severity() || 'Unknown');

  protected readonly cls = computed(() => {
    const base = 'inline-flex items-center gap-2 rounded-full px-2.5 py-1 text-xs font-semibold ring-1';
    switch ((this.severity() || '').toLowerCase()) {
      case 'critical':
        return `${base} bg-red-500/10 text-red-300 ring-red-500/30`;
      case 'high':
        return `${base} bg-orange-500/10 text-orange-300 ring-orange-500/30`;
      case 'medium':
        return `${base} bg-amber-500/10 text-amber-300 ring-amber-500/30`;
      case 'low':
        return `${base} bg-emerald-500/10 text-emerald-300 ring-emerald-500/30`;
      default:
        return `${base} bg-slate-500/10 text-slate-300 ring-slate-500/30`;
    }
  });

  protected readonly dot = computed(() => {
    switch ((this.severity() || '').toLowerCase()) {
      case 'critical': return '#ef4444';
      case 'high': return '#fb923c';
      case 'medium': return '#f59e0b';
      case 'low': return '#10b981';
      default: return '#94a3b8';
    }
  });
}
