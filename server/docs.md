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