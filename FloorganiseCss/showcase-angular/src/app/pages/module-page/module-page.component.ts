import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-module-page',
  imports: [RouterLink],
  templateUrl: './module-page.component.html',
})
export class ModulePageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly moduleName = this.route.snapshot.paramMap.get('name') ?? 'Module';
}
