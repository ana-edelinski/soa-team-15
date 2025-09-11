FROM node:20 AS build

WORKDIR /app
COPY package*.json ./

RUN npm install --legacy-peer-deps

COPY . .
RUN npm run build --prod

FROM nginx:stable-alpine
COPY --from=build /app/dist/explorer /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80