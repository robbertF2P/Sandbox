import { Component, input, output } from '@angular/core';

@Component({
  selector: 'f2p-app-navbar',
  templateUrl: './f2p-app-navbar.component.html',
})
export class F2pAppNavbarComponent {
  readonly title = input('Floor2Plan');
  readonly moduleMenuLabel = input('Open module menu');
  readonly moduleMenuEnabled = input(true);

  readonly moduleMenuClick = output<void>();
}
