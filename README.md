# Cloudy

A personal cloud storage application demonstrating production-grade backend architecture with clean separation of concerns, comprehensive testing, and containerized deployment.

**Try it here:** https://www.natetp.duckdns.org/cloudy  
*(Self-hosted; containers may be offline. Storage limited to 5MB for demo purposes.)*

## Overview

Cloudy is a full-stack file storage platform that implements clean architecture principles to achieve maintainable, testable, and scalable backend design. The system separates business logic from infrastructure concerns, enabling easy testing and future migrations between storage providers or databases.

## Architecture

The backend follows **Clean Architecture** with strict layer boundaries and dependency inversion:

- **Domain**: Core entities (`File`, `Folder`, `User`) and business rules. Zero framework dependencies.
- **Application**: Use case handlers, service interfaces, and business logic orchestration. Depends only on Domain.
- **Infrastructure**: EF Core repositories, MinIO blob storage adapter, JWT implementation. Implements Application interfaces.
- **API**: REST endpoints, DTOs, authentication middleware. Thin layer that delegates to Application services.

This structure ensures business logic remains testable without databases or external services, and infrastructure can be swapped (e.g., MinIO → S3, PostgreSQL → SQL Server) without touching core logic.

## Tech Stack

### Backend
- **.NET 9** with ASP.NET Core
- **Entity Framework Core** with PostgreSQL
- **MinIO** for object storage (abstracted via `IBlobStore` interface)
- **JWT Bearer** authentication
- **Swagger/OpenAPI** documentation
- **Docker** multi-stage builds

### Frontend
- React 19 + TypeScript
- Tailwind CSS
- Production-ready authentication and file management UI

### DevOps
- **CI/CD**: Automated build and test on pull requests
- **Docker Compose** for local development
- Self-hosted deployment with reverse proxy

## Key Engineering Practices

- **Separation of Concerns**: Domain and Application layers have no framework dependencies
- **Repository Pattern**: Data access abstracted behind interfaces
- **Unit of Work**: Transaction management for consistency
- **Interface-Based Design**: Storage abstraction (`IBlobStore`) allows swapping MinIO for S3/Azure without code changes
- **Comprehensive Testing**: Unit tests for all backend layers
- **Containerization**: Production-ready Docker images with multi-stage builds

## Testing

Backend includes unit test coverage across Domain, Application, and Infrastructure layers, validating business logic independently of external dependencies.

## Deployment

Fully containerized with Docker Compose. Multi-stage builds optimize image size and build time. Self-hosted on personal infrastructure with reverse proxy configuration.
