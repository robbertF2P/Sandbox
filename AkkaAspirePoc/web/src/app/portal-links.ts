export interface PortalLink {
  title: string;
  url: string | null;
  description: string;
  available: boolean;
  status?: string | null;
}

export interface ApiLinks {
  baseUrl: string;
  healthUrl: string;
  todosUrl: string;
  linksUrl: string;
}

export interface WebLinks {
  baseUrl: string;
  todosUrl: string;
}

export interface PortalLinksResponse {
  aspireDashboard: PortalLink;
  sentry: PortalLink;
  api: ApiLinks;
  web: WebLinks;
}
