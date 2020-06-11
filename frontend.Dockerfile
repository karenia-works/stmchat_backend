FROM node:lts as build

RUN apt update
RUN apt install -y git

WORKDIR /app
RUN git clone --depth=1 https://github.com/karenia-works/stmchat_frontend

WORKDIR /app/stmchat_frontend
COPY . .

RUN yarn install
RUN yarn build

FROM nginx:stable
COPY --from=build /app/stmchat_frontend/dist /app
COPY static.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
EXPOSE 443