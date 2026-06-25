import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { F2pButtonComponent } from '@floorganise/ui';
import { IdentityAuthService } from '@f2p/identity/data-access';

@Component({
  selector: 'f2p-login-page',
  imports: [FormsModule, F2pButtonComponent],
  templateUrl: './login-page.component.html',
})
export class LoginPageComponent {
  private readonly auth = inject(IdentityAuthService);
  private readonly router = inject(Router);

  readonly userName = signal('reg user');
  readonly password = signal('test');
  readonly rememberMe = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  onSubmit(): void {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    this.auth.login(this.userName(), this.password(), this.rememberMe()).subscribe({
      next: (success) => {
        this.isSubmitting.set(false);
        if (success) {
          void this.router.navigateByUrl('/');
          return;
        }

        this.errorMessage.set('Login failed. Check the API host is running on :5080.');
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('Login failed. Check the API host is running on :5080.');
      },
    });
  }
}
