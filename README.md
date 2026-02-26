# Cloudy

A personal cloud storage application for my personal use while exercising clean architecture. The primary purpose was to be a learning exercise for myself.

** Try it here:** https://www.natetp.duckdns.org/cloudy  
*(Self-hosted; containers may be offline. Storage limited to 5MB for demo purposes.)*

## Tech Stack

### Backend
- **.NET 9** with ASP.NET Core
- **Entity Framework Core** with PostgreSQL
- **MinIO** for object storage
- **Docker** multi-stage builds

### Frontend
- React 19 + TypeScript

### DevOps
- **CI/CD**: Automated build and test on pull requests
- **Docker Compose** for local development
- Self-hosted deployment with reverse proxy
