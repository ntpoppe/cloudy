# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore each project (cache-friendly)
COPY ["src/Cloudy.API/Cloudy.API.csproj", "Cloudy.API/"]
COPY ["src/Cloudy.Application/Cloudy.Application.csproj", "Cloudy.Application/"]
COPY ["src/Cloudy.Infrastructure/Cloudy.Infrastructure.csproj", "Cloudy.Infrastructure/"]
COPY ["src/Cloudy.Domain/Cloudy.Domain.csproj", "Cloudy.Domain/"]
RUN dotnet restore "Cloudy.API/Cloudy.API.csproj"

# Copy remaining source
COPY src .
WORKDIR "/src/Cloudy.API"
RUN dotnet build -c Release -o /app/build

# =========================
# Publish stage
# =========================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish
# If you want later:
#  /p:PublishTrimmed=true /p:PublishReadyToRun=true

# =========================
# Base runtime (idempotent non-root)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Create 'app' user if missing, and own /app
RUN if ! id -u app >/dev/null 2>&1; then \
      useradd -m -r -s /usr/sbin/nologin app; \
    fi && chown -R app:app /app

# =========================
# PROD target (guarded)
# =========================
FROM base AS prod
WORKDIR /app
COPY --from=publish /app/publish ./
COPY ./ops/entrypoint-prod.sh /entrypoint-prod.sh

# Make entrypoint executable as root, then drop privileges
RUN chmod +x /entrypoint-prod.sh && chown app:app /entrypoint-prod.sh
USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["/entrypoint-prod.sh"]

# =========================
# DEV target (build + run normally)
# =========================
FROM base AS dev
WORKDIR /app
COPY --from=publish /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cloudy.API.dll"]
