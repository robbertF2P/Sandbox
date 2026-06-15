FROM node:22-alpine AS build
WORKDIR /app

COPY ./AkkaSignalRVuePoc/client/package*.json ./
COPY ./FloorganiseCss /FloorganiseCss/
RUN npm install

COPY ./AkkaSignalRVuePoc/client/ ./
ARG VITE_API_BASE_URL=http://localhost:5000
ARG VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/live-messages
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_SIGNALR_HUB_URL=$VITE_SIGNALR_HUB_URL
RUN npm run build

FROM nginx:alpine AS runtime
COPY ./AkkaSignalRVuePoc/client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
