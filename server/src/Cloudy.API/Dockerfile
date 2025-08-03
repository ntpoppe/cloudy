FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore each project
COPY ["src/Cloudy.API/Cloudy.API.csproj", "Cloudy.API/"]
COPY ["src/Cloudy.Application/Cloudy.Application.csproj", "Cloudy.Application/"]
COPY ["src/Cloudy.Infrastructure/Cloudy.Infrastructure.csproj", "Cloudy.Infrastructure/"]
COPY ["src/Cloudy.Domain/Cloudy.Domain.csproj", "Cloudy.Domain/"]
RUN dotnet restore "Cloudy.API/Cloudy.API.csproj"

# Copy remaining source
COPY src .

WORKDIR "/src/Cloudy.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cloudy.API.dll"] 
