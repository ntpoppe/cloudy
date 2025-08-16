# ☁️ Cloudy - A Personal Cloud Storage Platform

A full-stack cloud storage solution built with modern technologies, featuring a React frontend and a robust .NET backend with clean architecture principles.

## Features

### **Completed Features**
- **User Authentication System**
  - JWT-based authentication
  - User registration and login
  - Protected routes and middleware
  - Session management

- **Modern React Frontend**
  - Responsive UI built with Tailwind CSS
  - Component-based architecture with TypeScript
  - React Router for navigation
  - Context-based state management
  - Custom hooks for business logic

- **Clean .NET Backend**
  - Clean Architecture (Domain, Application, Infrastructure, API layers)
  - Entity Framework Core with SQL Server
  - JWT token authentication
  - RESTful API endpoints
  - Comprehensive test coverage

### 🚧 **Work in Progress**
- **Cloud Storage Core**
  - File upload/download functionality
  - Folder creation and management
  - File organization and navigation
  - Storage quota management
  - File sharing and collaboration

## 🏗️ Architecture

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
- **React 19** - Latest React with concurrent features
- **TypeScript 5.8** - Type-safe development
- **Vite** - Fast build tool and dev server
- **Tailwind CSS 4** - Utility-first CSS framework
- **React Router 6** - Client-side routing
- **Radix UI** - Accessible component primitives
- **Lucide React** - Icon library

### **Backend**
- **.NET 9** - Latest .NET framework
- **Entity Framework Core** - ORM for data access
- **Postgres** - Database (with Docker support)
- **JWT Bearer** - Stateless authentication
- **Swagger/OpenAPI** - API documentation
- **Clean Architecture** - Separation of concerns

### **Development Tools**
- **ESLint** - Code quality and consistency
- **Prettier** - Code formatting
- **Docker** - Containerized development environment
- **Git** - Version control

## Getting Started

### **Prerequisites**
- Node.js 18+ and npm
- .NET 9 SDK
- Docker and Docker Compose
- Git

### **Quick Start**

TODO


## Project Structure

### **Key Directories**
- `client/` - React frontend application
- `server/` - .NET backend solution
- `docs/` - Project documentation
- `ops/` - Deployment and operations scripts

### **Database**
The application uses Postgres with Entity Framework Core. Database migrations are automatically applied on startup in development mode.

### **Environment Configuration**
- Frontend: Environment variables in `.env` files
- Backend: `appsettings.json` and environment-specific overrides

## Testing

### **Frontend Testing**
- TODO

### **Backend Testing**
- Unit tests for domain logic
- Integration tests for repositories
- API tests for controllers
- Test coverage for all layers

## Deployment

### **Docker Support**
- Multi-stage Docker builds for both frontend and backend
- Docker Compose for local development
- Production-ready containerization

### **Environment Variables**
- TODO: DESCRIBE THESE
