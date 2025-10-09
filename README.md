# Cloudy - My Personal Cloud Platform

A full-stack cloud storage solution, featuring a React frontend and an ASP.NET REST backend following clean architecture principles, with comprehensive unit testing.  

Try it here: (self-hosted, may not be running): https://www.natetp.duckdns.org/cloudy

## Features

- **User Authentication System**
  - JWT-based authentication
  - User registration and login
  - Protected routes and middleware
  - Session management
 
- **File Management**
  - File uploading, storage, and deletion
  - Organized per user
  - Secure access control for files

- **Modern React Frontend**
  - Responsive UI built with Tailwind CSS
  - Component-based architecture with TypeScript
  - React Router for navigation
  - Context-based state management
  - Custom hooks for business logic

- **ASP.NET Backend**
  - Clean Architecture (Domain, Application, Infrastructure, API layers)
  - Entity Framework Core with PostgreSQL
  - JWT token authentication
  - RESTful API endpoints
  - Comprehensive test coverage
 
- **CI**
  - Automated build and test on every pull request

##  Architecture

### **Frontend (React + TypeScript)**
```
client/
├── src/
│   ├── components/          # Reusable UI components
│   │   ├── ui/             # Base UI components (Button, Card, Input, etc.)
│   │   ├── auth/           # Authentication components
│   │   ├── files/          # File management components
│   │   ├── layout/         # Layout and navigation components
│   │   └── activity/       # Activity and notification components
│   ├── pages/              # Route components
│   │   ├── auth/           # Authentication pages
│   │   └── dashboard/      # Main application dashboard
│   ├── hooks/              # Custom React hooks
│   ├── contexts/           # React context providers
│   ├── services/           # API service layer
│   ├── types/              # TypeScript type definitions
│   └── lib/                # Utility functions and configurations
```

### **Backend (.NET 9 + Clean Architecture)**
```
server/
├── src/
│   ├── Cloudy.API/         # Web API layer
│   ├── Cloudy.Application/  # Business logic and use cases
│   ├── Cloudy.Domain/      # Domain models and business rules
│   └── Cloudy.Infrastructure/ # Data access and external services
├── tests/                   # Comprehensive test suite
└── docker-compose.yml       # Development environment setup
```

## Tech Stack

### **Frontend**
- React 19
- TypeScript 5.8
- Vite

### **Backend**
- .NET 9*
- Entity Framework Core
- Postgres
- JWT Bearer
- Swagger/OpenAPI
- Clean Architecture

### **Development Tools**
- Docker
- Git

## Testing
- Test coverage for backend layers

## Deployment
### **Hosting**
- Self-hosted on a personal laptop using a reverse proxy
  
### **Docker Support**
- Multi-stage Docker builds for both frontend and backend
- Docker Compose for local development
- Production-ready containerization


