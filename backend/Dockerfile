# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY EnterpriseUserManagement.sln ./
COPY src/EnterpriseUserManagement.Domain/*.csproj src/EnterpriseUserManagement.Domain/
COPY src/EnterpriseUserManagement.Application/*.csproj src/EnterpriseUserManagement.Application/
COPY src/EnterpriseUserManagement.Infrastructure/*.csproj src/EnterpriseUserManagement.Infrastructure/
COPY src/EnterpriseUserManagement.Api/*.csproj src/EnterpriseUserManagement.Api/
COPY tests/EnterpriseUserManagement.UnitTests/*.csproj tests/EnterpriseUserManagement.UnitTests/
COPY tests/EnterpriseUserManagement.IntegrationTests/*.csproj tests/EnterpriseUserManagement.IntegrationTests/

RUN dotnet restore EnterpriseUserManagement.sln

# Copy all source files
COPY . .

# Publish API
WORKDIR /src/src/EnterpriseUserManagement.Api
RUN dotnet publish EnterpriseUserManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: ASP.NET Core 9 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for container health checks
USER root
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Prepare persistent data directory with proper non-root permissions
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app

USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5100 \
    JsonStorage__StorageDirectory=/app/data \
    JsonStorage__FileName=users.json

EXPOSE 5100

HEALTHCHECK --interval=15s --timeout=5s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5100/health/ready || exit 1

ENTRYPOINT ["dotnet", "EnterpriseUserManagement.Api.dll"]

