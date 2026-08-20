import http from 'node:http';
import { readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { homedir } from 'node:os';
import { request as httpRequest } from 'node:http';
import { request as httpsRequest } from 'node:https';

const port = Number(process.env.TUNNEL_PROXY_PORT ?? 8888);
const apiTarget = new URL(process.env.API_TARGET ?? 'http://127.0.0.1:5080');
const webTarget = new URL(process.env.WEB_TARGET ?? 'http://127.0.0.1:4200');
const aspireTarget = new URL(process.env.ASPIRE_DASHBOARD_TARGET ?? 'https://127.0.0.1:17261');
const aspirePrefix = '/aspire';
const loginTokenFile = process.env.ASPIRE_DASHBOARD_LOGIN_TOKEN_FILE ?? '/tmp/aspire-dashboard-login-token.txt';

function readDashboardLoginToken() {
  try {
    const logDir = `${homedir()}/.aspire/logs`;
    const logs = readdirSync(logDir)
      .filter((name) => name.startsWith('cli_'))
      .sort()
      .reverse();

    for (const logName of logs) {
      const content = readFileSync(`${logDir}/${logName}`, 'utf8');
      const match = content.match(/login\?t=([a-f0-9]+)/);
      if (match) {
        return match[1];
      }
    }
  } catch {
    // Aspire CLI log not available yet.
  }

  return null;
}

const dashboardLoginToken = process.env.ASPIRE_DASHBOARD_LOGIN_TOKEN ?? readDashboardLoginToken();
if (dashboardLoginToken) {
  writeFileSync(loginTokenFile, dashboardLoginToken, 'utf8');
}

function routeRequest(pathname) {
  if (pathname.startsWith('/api') || pathname === '/health' || pathname.startsWith('/health/')) {
    return { target: apiTarget, path: pathname };
  }

  if (pathname === aspirePrefix || pathname.startsWith(`${aspirePrefix}/`)) {
    const stripped = pathname === aspirePrefix
      ? '/'
      : pathname.slice(aspirePrefix.length) || '/';
    return { target: aspireTarget, path: stripped };
  }

  return { target: webTarget, path: pathname };
}

function buildForwardHeaders(clientReq, target, publicHost) {
  const forwardedProto =
    clientReq.headers['x-forwarded-proto']
    ?? (clientReq.socket.encrypted ? 'https' : 'http');
  const forwardedHost = clientReq.headers['x-forwarded-host'] ?? clientReq.headers.host ?? publicHost ?? target.host;

  return {
    ...clientReq.headers,
    host: target.host,
    'x-forwarded-host': forwardedHost,
    'x-forwarded-proto': forwardedProto,
    'x-forwarded-for': clientReq.headers['x-forwarded-for'] ?? clientReq.socket.remoteAddress,
  };
}

function rewriteLocation(location, publicHost) {
  if (!location || !publicHost) {
    return location;
  }

  try {
    const resolved = new URL(location, aspireTarget);
    if (!['localhost', '127.0.0.1'].includes(resolved.hostname)) {
      return location;
    }

    const suffix = `${resolved.pathname}${resolved.search}`;
    const normalized = suffix.startsWith('/') ? suffix : `/${suffix}`;
    return `https://${publicHost}${aspirePrefix}${normalized === '/' ? '/login' : normalized}`;
  } catch {
    return location;
  }
}

function rewriteResponseHeaders(headers, clientReq) {
  const publicHost = clientReq.headers['x-forwarded-host'] ?? clientReq.headers.host;
  const rewritten = { ...headers };

  if (rewritten.location) {
    rewritten.location = rewriteLocation(
      Array.isArray(rewritten.location) ? rewritten.location[0] : rewritten.location,
      publicHost,
    );
  }

  return rewritten;
}

function proxyRequest(clientReq, clientRes, route) {
  const requestFn = route.target.protocol === 'https:' ? httpsRequest : httpRequest;
  const incomingUrl = new URL(clientReq.url ?? '/', 'http://localhost');

  const proxyReq = requestFn(
    {
      protocol: route.target.protocol,
      hostname: route.target.hostname,
      port: route.target.port,
      path: `${route.path}${incomingUrl.search}`,
      method: clientReq.method,
      headers: buildForwardHeaders(clientReq, route.target),
      rejectUnauthorized: false,
    },
    (proxyRes) => {
      clientRes.writeHead(proxyRes.statusCode ?? 502, rewriteResponseHeaders(proxyRes.headers, clientReq));
      proxyRes.pipe(clientRes);
    },
  );

  proxyReq.on('error', (error) => {
    console.error(`Proxy error (${route.target.origin}${route.path}):`, error.message);
    if (!clientRes.headersSent) {
      clientRes.writeHead(502, { 'Content-Type': 'text/plain' });
    }
    clientRes.end('Bad gateway');
  });

  clientReq.pipe(proxyReq);
}

const server = http.createServer((req, res) => {
  const pathname = new URL(req.url ?? '/', 'http://localhost').pathname;
  proxyRequest(req, res, routeRequest(pathname));
});

server.on('upgrade', (req, socket, head) => {
  const pathname = new URL(req.url ?? '/', 'http://localhost').pathname;
  const route = routeRequest(pathname);
  const requestFn = route.target.protocol === 'https:' ? httpsRequest : httpRequest;

  const proxyReq = requestFn({
    protocol: route.target.protocol,
    hostname: route.target.hostname,
    port: route.target.port,
    path: `${route.path}${new URL(req.url ?? '/', 'http://localhost').search}`,
    method: req.method,
    headers: buildForwardHeaders(req, route.target),
    rejectUnauthorized: false,
  });

  proxyReq.on('upgrade', (proxyRes, proxySocket, proxyHead) => {
    socket.write(
      `HTTP/1.1 ${proxyRes.statusCode} ${proxyRes.statusMessage}\r\n` +
        Object.entries(proxyRes.headers)
          .filter(([, value]) => value !== undefined)
          .map(([key, value]) => `${key}: ${value}`)
          .join('\r\n') +
        '\r\n\r\n',
    );
    if (proxyHead.length > 0) {
      socket.write(proxyHead);
    }
    proxySocket.pipe(socket);
    socket.pipe(proxySocket);
  });

  proxyReq.on('error', () => socket.destroy());
  proxyReq.end();
});

server.listen(port, '0.0.0.0', () => {
  console.log(`Tunnel proxy listening on http://0.0.0.0:${port}`);
  console.log(`  Web     -> ${webTarget.origin}`);
  console.log(`  API     -> ${apiTarget.origin}`);
  console.log(`  Aspire  -> ${aspireTarget.origin} (prefix ${aspirePrefix})`);
  if (dashboardLoginToken) {
    console.log(`  Aspire login token cached at ${loginTokenFile}`);
  }
});
