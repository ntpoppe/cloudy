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
dotnet ef migrations add UserFile
  --startup-project ../Cloudy.API \
  --context Cloudy.Infrastructure.Data.CloudyDbContext \
  --output-dir Migrations

dotnet ef database update \
  --startup-project ../Cloudy.API \
  --context Cloudy.Infrastructure.Data.CloudyDbContext \
```

### Compose up/down
```bash
docker compose --env-file .env.dev up -d --build
docker compose --env-file .env.dev down
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

### Minio
## 1. Install MinIO & Client
On Arch Linux:
```bash
sudo pacman -Syu
sudo pacman -S minio minio-client   # client is called "mcli" on Arch
```

## 2. Configure minio
In `/etc/minio/minio.conf`:
```bash
MINIO_ROOT_USER="adminuser"
MINIO_ROOT_PASSWORD="supersecretpassword"
MINIO_VOLUMES="/srv/minio/data"
MINIO_OPTS="--address :9000 --console-address :9001"
```
## 3. Create storage path

```bash
sudo mkdir -p /srv/minio/data
sudo chown -R minio:minio /srv/minio
```

## 4. Enable and start MinIO
```bash
sudo systemctl enable --now minio
journalctl -u minio -f
```

## 5. Create a bucket: Set up client alias and create bucket
```bash
mcli alias set local http://127.0.0.1:9000 adminuser supersecretpassword
mcli mb local/cloudy
mcli version enable local/cloudy
```

## 6. Create a .env
```bash
INIO_ENDPOINT=127.0.0.1:9000
MINIO_ACCESS_KEY=CLOUDY_APP_KEY
MINIO_SECRET_KEY=CLOUDY_APP_SECRET
MINIO_BUCKET=cloudy
```

## BlobStore: What, Why, and How

### What it is

A **thin storage port (interface)** your Application layer uses for file bytes. It hides the concrete backend (MinIO today, S3/Azure/local FS tomorrow).

### Why it exists

- **Keeps Domain/Application clean** (no SDK types, no storage details).
- **Enables easy swap of backends** (only Infrastructure changes).
- **Makes testing simple** (mock the interface).
- **Centralizes upload/download/signing rules** (checksums, limits, metadata).

---

## The Interface (Minimal but Complete)

### Responsibilities (What BlobStore Should Handle)

- **Streaming I/O:** Never buffer whole files in RAM. If size is unknown, spool to temp file.
- **Content type:** Accept caller’s type; (optionally) sniff magic bytes elsewhere.
- **Object keys:** Accept a caller-provided `objectKey`. (Caller decides naming scheme.)
- **Errors:** Throw clear exceptions; don’t swallow SDK errors.
- **Presigned URLs:** Short TTL, least-privileged operations (PUT/GET only).

### What BlobStore Should **NOT** Do

- DB writes, auth/ACL logic, folder tree, business rules. Those stay in Application.
- Generate object keys (that’s the caller’s concern—see below).

---

## Object Key Strategy (Caller Decides)

Use a stable scheme that avoids collisions and helps debugging:
`{ulid}/{yyyy}/{MM}/{dd}/{ulid}-{safeFileName}`

Example: 01J8.../2025/08/30/01J8...-report.pdf

Store bucket + objectKey in your DB row. That’s all you need to retrieve.

## Typical Call Flows

### A) Presigned (recommended)

1. API calls `GetPresignedPutUrlAsync(bucket, objectKey, ttl)`.  
2. Client `PUT`s bytes directly to MinIO.  
3. API finalizes DB row (size, type, hash/etag).  
4. For download, API returns `GetPresignedGetUrlAsync(...)`.  

**Why:** API never proxies multi-GB streams.

---

### B) Proxy (simple)

1. API receives `IFormFile` stream → `UploadAsync(...)` → save metadata.  
2. Keep `DownloadAsync(...)` if you need `GET /files/{id}` to stream through your API.

---

## Implementation (MinIO-backed)

In Infrastructure, `MinioBlobStore` uses the MinIO .NET SDK:

- `PutObjectAsync` → `UploadAsync`  
- `RemoveObjectAsync` → `DeleteAsync`  
- `PresignedPutObjectAsync` / `PresignedGetObjectAsync` → presigned URLs  
- `GetObjectAsync` (callback stream) → `DownloadAsync`

### Key details

- **Size:** MinIO needs object size. If `Stream.CanSeek == false`, spool to a temp file to determine size.  
- **Dispose:** Close temp streams; delete temp files in `finally`.  
- **CT:** Pass `CancellationToken`.  
- **Retries:** Let SDK handle; optionally wrap with Polly.

---

## Where It Plugs In

**Application (FileService)** depends on `IBlobStore`:

- Create `objectKey`.  
- Call `_blob.UploadAsync(...)`.  
- Save DB row (`Bucket`, `ObjectKey`, `OriginalName`, `ContentType`, `Size`, etc.).  
- For delete: `_blob.DeleteAsync(...)` then soft-delete metadata.

**Infrastructure** provides `MinioBlobStore` and DI registration.  

**API** exposes endpoints (`presign`, `finalize`, `download-url`) and never sees SDK types.

---