import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LinksService } from '../links.service';
import { PortalLink, PortalLinksResponse } from '../portal-links';

interface PortalCard {
  title: string;
  description: string;
  url: string | null;
  available: boolean;
  label: string;
  external: boolean;
  status?: string | null;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent implements OnInit {
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly cards = signal<PortalCard[]>([]);

  constructor(private readonly linksService: LinksService) {}

  ngOnInit(): void {
    this.linksService.getLinks().subscribe({
      next: links => {
        this.cards.set(this.toCards(links));
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.message ?? 'Failed to load portal links');
        this.cards.set(this.fallbackCards());
        this.loading.set(false);
      }
    });
  }

  private toCards(links: PortalLinksResponse): PortalCard[] {
    return [
      {
        title: 'Todo app',
        description: 'Try the Akka-backed todo list.',
        url: '/todos',
        available: true,
        label: 'Open todos',
        external: false
      },
      this.fromLink(links.aspireDashboard, 'Open dashboard'),
      this.fromLink(links.sentry, 'Open Sentry'),
      {
        title: 'API health',
        description: 'Liveness and readiness checks.',
        url: links.api.healthUrl,
        available: true,
        label: 'Health check',
        external: true
      },
      {
        title: 'Todos API',
        description: 'REST endpoint backed by Akka actors.',
        url: links.api.todosUrl,
        available: true,
        label: 'View JSON',
        external: true
      },
      {
        title: 'API portal',
        description: 'HTML landing page served by the API host.',
        url: '/',
        available: true,
        label: 'Open API portal',
        external: false
      }
    ];
  }

  private fromLink(link: PortalLink, label: string): PortalCard {
    return {
      title: link.title,
      description: link.description,
      url: link.url,
      available: link.available,
      label,
      external: true,
      status: link.status
    };
  }

  private fallbackCards(): PortalCard[] {
    return [
      {
        title: 'Todo app',
        description: 'Try the Akka-backed todo list.',
        url: '/todos',
        available: true,
        label: 'Open todos',
        external: false
      },
      {
        title: 'Aspire dashboard',
        description: 'Distributed app orchestration, logs, traces, and resource health.',
        url: (window as unknown as { __ASPIRE_DASHBOARD_URL__?: string }).__ASPIRE_DASHBOARD_URL__ ?? null,
        available: false,
        label: 'Open dashboard',
        external: true,
        status: 'Start the AppHost — the dashboard is not available in API-only demo mode.'
      },
      {
        title: 'Sentry performance',
        description: 'Optional cloud observability — no local UI.',
        url: (window as unknown as { __SENTRY_PROJECT_URL__?: string }).__SENTRY_PROJECT_URL__ ?? null,
        available: false,
        label: 'Open Sentry',
        external: true,
        status: 'Optional — set Sentry:Dsn and Sentry:ProjectUrl in the API.'
      }
    ];
  }
}
