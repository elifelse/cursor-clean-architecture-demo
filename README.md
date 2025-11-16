# Cursor Clean Architecture Demo

A .NET 8 Web API implementation demonstrating Clean Architecture principles with JWT authentication.

## Overview

This solution showcases a production-ready Clean Architecture structure with clear separation of concerns across four layers: Api, Application, Domain, and Infrastructure. The project includes JWT authentication, Swagger documentation, and an in-memory repository for rapid development.

## Architecture

```
┌─────────────────────────────────────┐
│         CursorDemo.Api              │
│    (Controllers, Routing, Auth)     │
└──────────────┬──────────────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼──────┐  ┌─────▼──────────────┐
│ Application │  │  Infrastructure    │
│ (Services,  │  │  (Repositories,    │
│  DTOs,      │  │   TokenService)    │
│  Interfaces) │  │                    │
└──────┬──────┘  └─────┬──────────────┘
       │                │
       └───────┬────────┘
               │
       ┌───────▼──────┐
       │    Domain    │
       │  (Entities,  │
       │  Interfaces) │
       └──────────────┘
```

**Dependency Flow:** Api → Application & Infrastructure → Domain

## Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **Api** | HTTP handling, routing, authentication, DI configuration | Application, Infrastructure |
| **Application** | Business logic, use cases, DTOs, service interfaces | Domain |
| **Domain** | Core entities, repository interfaces | None |
| **Infrastructure** | Repository implementations, external services | Domain, Application |

## Folder Structure

```
CursorDemo.sln
├── CursorDemo.Api/
│   ├── Controllers/          # AuthController, BooksController
│   ├── Program.cs            # Startup & DI
│   └── appsettings.json      # Configuration
├── CursorDemo.Application/
│   ├── Configuration/        # JwtSettings
│   ├── DTOs/                 # BookDto, CreateBookDto, LoginDto, TokenResponseDto
│   ├── Interfaces/           # IBookService, ITokenService
│   └── Services/             # BookService
├── CursorDemo.Domain/
│   ├── Entities/             # Book
│   └── Interfaces/           # IBookRepository
└── CursorDemo.Infrastructure/
    ├── Repositories/         # InMemoryBookRepository
    └── Services/             # TokenService
```

## Features

- ✅ **Clean Architecture** - 4-layer separation with dependency inversion
- ✅ **JWT Authentication** - Token-based security with Bearer tokens
- ✅ **Book CRUD** - Create, read operations with in-memory storage
- ✅ **Swagger UI** - Interactive API documentation with JWT support
- ✅ **Dependency Injection** - Interface-based DI configuration
- ✅ **DTO Pattern** - Clean separation between domain and API models

## Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/auth/login` | ❌ | Authenticate and get JWT token |
| `GET` | `/api/books` | ✅ | Get all books |
| `GET` | `/api/books/{id}` | ✅ | Get book by ID |
| `POST` | `/api/books` | ✅ | Create a new book |

**Demo Credentials:**
- Username: `elif`
- Password: `1234`

## How to Run

### Prerequisites
- .NET 8 SDK

### Steps

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API
cd CursorDemo.Api
dotnet run
```

**Access:**
- API: `https://localhost:5001` or `http://localhost:5000`
- Swagger: `https://localhost:5001/swagger`

### Testing Authentication

1. Open Swagger UI
2. Call `POST /api/auth/login` with credentials
3. Copy the token from response
4. Click "Authorize" button and enter: `Bearer <token>`
5. Test protected endpoints

## Technologies

| Technology | Purpose |
|-----------|---------|
| .NET 8 | Core framework |
| ASP.NET Core | Web API framework |
| JWT Bearer | Authentication |
| Swagger/OpenAPI | API documentation |
| Clean Architecture | Architecture pattern |

## Future Enhancements

- Input validation (FluentValidation)
- Global error handling middleware
- Structured logging (Serilog)
- Unit & integration tests
- Entity Framework Core integration
- CQRS with MediatR
- Pagination and filtering
- Background services

## License

MIT License - see [LICENSE](LICENSE) file for details.
