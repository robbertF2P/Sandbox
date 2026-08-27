import http from 'node:http';
import { request as httpRequest } from 'node:http';
import { request as httpsRequest } from 'node:https';

const port = Number(process.env.TUNNEL_PROXY_PORT ?? 8888);
const apiTarget = new URL(process.env.API_TARGET ?? 'http://127.0.0.1:5080');
const webTarget = new URL(process.env.WEB_TARGET ?? 'http://127.0.0.1:4200');

function routeToApi(pathname) {
  return pathname.startsWith('/api') || pathname === '/health' || pathname.startsWith('/health/');
}

function proxyRequest(clientReq, clientRes, target) {
  const requestFn = target.protocol === 'https:' ? httpsRequest : httpRequest;
  const url = new URL(clientReq.url ?? '/', target);

  const proxyReq = requestFn(
    {
      protocol: target.protocol,
      hostname: target.hostname,
      port: target.port,
      path: `${url.pathname}${url.search}`,
      method: clientReq.method,
      headers: {
        ...clientReq.headers,
        host: target.host,
      },
    },
    (proxyRes) => {
      clientRes.writeHead(proxyRes.statusCode ?? 502, proxyRes.headers);
      proxyRes.pipe(clientRes);
    },
  );

  proxyReq.on('error', (error) => {
    console.error('Proxy error:', error.message);
    if (!clientRes.headersSent) {
      clientRes.writeHead(502, { 'Content-Type': 'text/plain' });
    }
    clientRes.end('Bad gateway');
  });

  clientReq.pipe(proxyReq);
}

const server = http.createServer((req, res) => {
  const pathname = new URL(req.url ?? '/', 'http://localhost').pathname;
  proxyRequest(req, res, routeToApi(pathname) ? apiTarget : webTarget);
});

server.on('upgrade', (req, socket, head) => {
  const pathname = new URL(req.url ?? '/', 'http://localhost').pathname;
  const target = routeToApi(pathname) ? apiTarget : webTarget;
  const requestFn = target.protocol === 'https:' ? httpsRequest : httpRequest;

  const proxyReq = requestFn({
    protocol: target.protocol,
    hostname: target.hostname,
    port: target.port,
    path: req.url,
    method: req.method,
    headers: {
      ...req.headers,
      host: target.host,
    },
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
  console.log(`  Web  -> ${webTarget.origin}`);
  console.log(`  API  -> ${apiTarget.origin}`);
});
