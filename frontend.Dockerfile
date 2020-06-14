FROM node:lts-alpine as build

RUN apk add --no-cache git

WORKDIR /app
RUN git clone --depth=1 https://github.com/karenia-works/stmchat_frontend

WORKDIR /app/stmchat_frontend

RUN yarn install
RUN yarn build

FROM caddy:2-alpine
COPY --from=build /app/stmchat_frontend/dist /app
COPY caddy/Caddyfile /etc/caddy/Caddyfile
EXPOSE 80
EXPOSE 443
