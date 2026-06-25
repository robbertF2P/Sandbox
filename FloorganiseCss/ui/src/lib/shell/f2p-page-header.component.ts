import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'f2p-page-header',
  imports: [RouterLink],
  templateUrl: './f2p-page-header.component.html',
})
export class F2pPageHeaderComponent {
  readonly title = input.required<string>();
  readonly homeLink = input<string | null>('/');
  readonly maxWidthClass = input('max-w-6xl');
}
