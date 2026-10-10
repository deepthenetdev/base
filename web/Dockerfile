# syntax=docker/dockerfile:1
# Build context is the web/ folder:  docker build web

# ---- Build stage: install dependencies and produce static files ----
FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
# Use the lockfile when present for reproducible installs.
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi
COPY . .
RUN npm run build

# ---- Runtime stage: unprivileged nginx serving the static build ----
FROM nginxinc/nginx-unprivileged:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
