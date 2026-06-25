import { Component, computed, input } from '@angular/core';
import { F2pButtonSize, F2pButtonVariant } from './f2p-button.model';

@Component({
  selector: 'f2p-btn',
  template: `
    <button
      [attr.type]="type()"
      [class]="buttonClass()"
      [disabled]="disabled()"
    >
      <ng-content />
    </button>
  `,
})
export class F2pButtonComponent {
  readonly variant = input<F2pButtonVariant>('primary');
  readonly size = input<F2pButtonSize>('md');
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly disabled = input(false);

  readonly buttonClass = computed(() => {
    const variant = this.variant();
    const size = this.size();
    return `f2ps-btn f2ps-btn-${variant} f2ps-btn-${size}`;
  });
}
