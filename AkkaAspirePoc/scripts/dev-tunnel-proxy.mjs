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
    return { target: apiTarget, path: pathname, isAspire: false };
  }

  if (pathname === aspirePrefix || pathname.startsWith(`${aspirePrefix}/`)) {
    const stripped = pathname === aspirePrefix
      ? '/'
      : pathname.slice(aspirePrefix.length) || '/';
    return { target: aspireTarget, path: stripped, isAspire: true };
  }

  return { target: webTarget, path: pathname, isAspire: false };
}

function buildForwardHeaders(clientReq, target) {
  const forwardedProto =
    clientReq.headers['x-forwarded-proto']
    ?? (clientReq.socket.encrypted ? 'https' : 'http');
  const forwardedHost = clientReq.headers['x-forwarded-host'] ?? clientReq.headers.host ?? target.host;

  return {
    ...clientReq.headers,
    host: target.host,
    'x-forwarded-host': forwardedHost,
    'x-forwarded-proto': forwardedProto,
    'x-forwarded-for': clientReq.headers['x-forwarded-for'] ?? clientReq.socket.remoteAddress,
  };
}

function rewriteAspireLocation(location, publicHost) {
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
    const aspirePath = normalized === '/' ? `${aspirePrefix}/` : `${aspirePrefix}${normalized}`;
    return `https://${publicHost}${aspirePath}`;
  } catch {
    return location;
  }
}

function rewriteAspireHtml(body) {
  return body
    .replace(/<base\s+href="\/"\s*\/?>/gi, `<base href="${aspirePrefix}/" />`)
    .replace(/<base\s+href='\/'\s*\/?>/gi, `<base href="${aspirePrefix}/" />`);
}

function rewriteResponseHeaders(headers, clientReq, isAspire) {
  const publicHost = clientReq.headers['x-forwarded-host'] ?? clientReq.headers.host;
  const rewritten = { ...headers };

  if (isAspire && rewritten.location) {
    rewritten.location = rewriteAspireLocation(
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
      const headers = rewriteResponseHeaders(proxyRes.headers, clientReq, route.isAspire);
      const contentType = String(headers['content-type'] ?? '');
      const shouldRewriteHtml = route.isAspire && contentType.includes('text/html');

      if (!shouldRewriteHtml) {
        clientRes.writeHead(proxyRes.statusCode ?? 502, headers);
        proxyRes.pipe(clientRes);
        return;
      }

      const chunks = [];
      proxyRes.on('data', (chunk) => chunks.push(chunk));
      proxyRes.on('end', () => {
        const body = rewriteAspireHtml(Buffer.concat(chunks).toString('utf8'));
        headers['content-length'] = Buffer.byteLength(body);
        delete headers['transfer-encoding'];
        clientRes.writeHead(proxyRes.statusCode ?? 502, headers);
        clientRes.end(body);
      });
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
          .map(([key, value]) => `${key}: ${Array.isArray(value) ? value.join(', ') : value}`)
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
  if (head?.length > 0) {
    proxyReq.write(head);
  }
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
