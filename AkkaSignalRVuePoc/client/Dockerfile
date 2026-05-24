FROM node:22-alpine AS build
WORKDIR /app

COPY client/package*.json ./
RUN npm ci

COPY client/ ./
ARG VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/live-messages
ENV VITE_SIGNALR_HUB_URL=$VITE_SIGNALR_HUB_URL
RUN npm run build

FROM nginx:alpine AS runtime
COPY client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
