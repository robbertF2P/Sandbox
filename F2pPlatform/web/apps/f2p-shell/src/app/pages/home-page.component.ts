import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface ModuleTile {
  title: string;
  description: string;
  route: string;
}

@Component({
  selector: 'f2p-home-page',
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
})
export class HomePageComponent {
  readonly tiles: ModuleTile[] = [
    {
      title: 'Reference',
      description: 'Template context — module status + SignalR events',
      route: '/reference',
    },
  ];
}
