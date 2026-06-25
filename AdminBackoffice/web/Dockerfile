FROM node:22-alpine AS build
WORKDIR /app

COPY FloorganiseCss/package*.json /FloorganiseCss/
WORKDIR /FloorganiseCss
RUN npm ci

WORKDIR /app
COPY AdminBackoffice/web/package*.json ./
RUN npm ci

COPY FloorganiseCss /FloorganiseCss
COPY AdminBackoffice/web/ ./
RUN npm run build

FROM nginx:alpine AS runtime
COPY AdminBackoffice/web/nginx.docker.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/admin-shell/browser /usr/share/nginx/html

EXPOSE 80
