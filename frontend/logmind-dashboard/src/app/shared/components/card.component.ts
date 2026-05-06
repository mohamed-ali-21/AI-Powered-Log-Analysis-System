import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="surface flex h-full flex-col rounded-xl p-5">
      <ng-content />
    </div>
  `,
  styles: [`:host { display: block; }`]
})
export class CardComponent {}
