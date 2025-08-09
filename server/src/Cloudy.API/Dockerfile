# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore each project (same layout you had)
COPY ["src/Cloudy.API/Cloudy.API.csproj", "Cloudy.API/"]
COPY ["src/Cloudy.Application/Cloudy.Application.csproj", "Cloudy.Application/"]
COPY ["src/Cloudy.Infrastructure/Cloudy.Infrastructure.csproj", "Cloudy.Infrastructure/"]
COPY ["src/Cloudy.Domain/Cloudy.Domain.csproj", "Cloudy.Domain/"]
RUN dotnet restore "Cloudy.API/Cloudy.API.csproj"

# Copy remaining source
COPY src .

# Build (Release)
WORKDIR "/src/Cloudy.API"
RUN dotnet build -c Release -o /app/build

# =========================
# Publish stage
# =========================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# =========================
# Base runtime (non-root)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# (optional) run as non-root
RUN adduser --disabled-password --gecos "" app && chown -R app /app
USER app

# =========================
# PROD target (guarded)
# =========================
FROM base AS prod
WORKDIR /app
COPY --from=publish /app/publish ./
# guard entrypoint (place file at ./ops/entrypoint-prod.sh)
COPY ./ops/entrypoint-prod.sh /entrypoint-prod.sh
USER root && chmod +x /entrypoint-prod.sh && chown app:app /entrypoint-prod.sh && USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["/entrypoint-prod.sh"]

# =========================
# DEV target (hot reload)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev
WORKDIR /src
# Tools if you want them available inside container
RUN dotnet tool install --global dotnet-ef \
 && echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /root/.bashrc
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# source is mounted via compose; watch rebuilds
ENTRYPOINT ["dotnet", "watch", "--project", "src/Cloudy.API/Cloudy.API.csproj", "run", "--no-restore"]

