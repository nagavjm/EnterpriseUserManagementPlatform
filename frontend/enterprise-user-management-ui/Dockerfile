# Stage 1: Build Angular SPA
FROM node:20-alpine AS build
WORKDIR /app

# Copy dependency files for Docker layer caching
COPY package*.json ./
RUN npm ci

# Copy application source files
COPY . .

# Build Angular production bundle
RUN npm run build -- --configuration production

# Stage 2: Nginx Alpine Runtime
FROM nginx:1.27-alpine AS final

# Remove default static HTML
RUN rm -rf /usr/share/nginx/html/*

# Copy custom nginx configuration with reverse proxy & pushState fallback
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy compiled Angular production assets
COPY --from=build /app/dist/enterprise-user-management-ui/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

