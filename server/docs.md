| Layer              | Responsibility                                       | Cloud Storage Analogy                                |
| ------------------ | ---------------------------------------------------- | ---------------------------------------------------- |
| **API**            | HTTP endpoints, DTOs, JWT auth, Swagger, CORS        | Presents upload/download/share operations to clients |
| **Application**    | Use-case handlers, interfaces (`IFileStorage`)       | Orchestrates storage calls, issues pre-signed URLs   |
| **Domain**         | Entities (`FileMetadata`, `Folder`), invariants      | Captures rules: versions, permissions, checksums     |
| **Infrastructure** | EF Core, blob adapters (Azure, S3), background queue | Implements actual blob reads/writes, thumbnails      |

### Why Layering Matters (and Makes Life Easier)

Splitting things up into API, Application, Domain, and Infrastructure layers isn’t just for show—it actually makes the code way easier to test and maintain. The business logic (the “core” stuff) doesn’t care about frameworks or how/where data is stored. So, if you want to swap out your ORM, change storage providers, or just write some unit tests, you don’t have to touch the important logic. You can test use-cases and entities by themselves, without spinning up a database or web server.

### No Framework Lock-In

The Domain and Application layers don’t depend on EF Core, ASP.NET Core, Azure SDKs, or any other outside libraries. That means if you ever need to move from SQL Server to Postgres, or from local disk to S3, you can do it without rewriting your business logic. This kind of flexibility is a big deal for a cloud storage system that’s going to change and grow over time.

### Project Dependencies
# Application → Domain
dotnet add src/Cloudy.Application/Cloudy.Application.csproj \
  reference src/Cloudy.Domain/Cloudy.Domain.csproj

# Infrastructure → Domain, Application
dotnet add src/Cloudy.Infrastructure/Cloudy.Infrastructure.csproj \
  reference src/Cloudy.Domain/Cloudy.Domain.csproj
dotnet add src/Cloudy.Infrastructure/Cloudy.Infrastructure.csproj \
  reference src/Cloudy.Application/Cloudy.Application.csproj

# API → Application, Infrastructure
dotnet add src/Cloudy.API/Cloudy.API.csproj \
  reference src/Cloudy.Application/Cloudy.Application.csproj
dotnet add src/Cloudy.API/Cloudy.API.csproj \
  reference src/Cloudy.Infrastructure/Cloudy.Infrastructure.csproj

### Value Objects
A Value Object is an immutable type that:
- Has no identity (unlike entities, which have a unique Id)
- Is defined entirely by its properties (value-based equality)
- Is immutable once created
- Can be reused or replaced freely (you don’t modify a value object—you create a new one)

### Migrations (in Infrastructure)
```bash
dotnet ef migrations add InitialCreate \
  --startup-project ../Cloudy.API \
  --context Cloudy.Infrastructure.Data.CloudyDbContext \
  --output-dir Migrations

dotnet ef database update \
  --startup-project ../Cloudy.API \
  --context Cloudy.Infrastructure.Data.CloudyDbContext \
  --connection "Host=localhost;Port=5433;Database=cloudy;Username=cloudy_user;Password=thiswillbechanged"
```

```bash
cloudy-api:
  container_name: cloudy-api
  build:
    context: .
    dockerfile: src/Cloudy.API/Dockerfile
  ports:
    - "5000:80"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Host=cloudy-db;Port=5432;Database=cloudy;Username=cloudy_user;Password=thiswillbechanged
  depends_on:
    - cloudy-db
```
