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
- ✅ **Validation Rules** - FluentValidation for automatic request validation
- ✅ **Global Error Handling** - Unified error responses via middleware

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

## Validation & Error Handling

### FluentValidation

The API uses FluentValidation for automatic request validation. Validators are defined in the Application layer and run automatically before controller actions execute. This ensures all incoming requests are validated according to business rules without requiring manual checks in controllers.

**How It Works:**
- Validators are registered automatically via dependency injection
- Validation runs before the controller action is invoked
- Invalid requests return a 400 Bad Request with detailed field-level error messages
- Controllers remain clean with no manual validation logic

**Example Validator:**
```csharp
public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MinimumLength(3)
            .WithMessage("Title must be at least 3 characters long.");
        
        RuleFor(x => x.Author)
            .NotEmpty()
            .WithMessage("Author is required.")
            .MinimumLength(3)
            .WithMessage("Author must be at least 3 characters long.");
    }
}
```

### Global Exception Handling

The `GlobalExceptionMiddleware` catches all unhandled exceptions, logs them, and returns standardized error responses. It runs in the middleware pipeline after HTTPS redirection to ensure all errors are handled consistently without interfering with validation errors.

**Features:**
- Catches all unhandled exceptions
- Maps common exception types to appropriate HTTP status codes:
  - `KeyNotFoundException` → 404 Not Found
  - `UnauthorizedAccessException` → 401 Unauthorized
  - `ArgumentException` → 400 Bad Request
  - All others → 500 Internal Server Error
- Logs exception details for debugging
- Returns standardized error responses matching validation error format

### Standardized Error Response

All API errors (validation failures and exceptions) return a consistent `ErrorResponse` format:

**Validation Error Example:**
```json
{
  "statusCode": 400,
  "message": "Validation failed. Please check the errors and try again.",
  "errors": {
    "ISBN": ["ISBN is required."],
    "Title": ["Title must be at least 3 characters long."],
    "Author": ["Author is required.", "Author must be at least 3 characters long."],
    "PublishedDate": ["Published date is required."]
  },
  "details": null
}
```

**Exception Error Example:**
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request.",
  "errors": null,
  "details": "System.Exception: ... (only in Development environment)"
}
```

**Response Fields:**
- **statusCode**: HTTP status code (400, 401, 404, 500, etc.)
- **message**: Human-readable error message
- **errors**: Dictionary of field-specific validation errors (null for exceptions)
- **details**: Stack trace and exception details (only in Development environment, null in Production)

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
| FluentValidation | Request validation |
| Swagger/OpenAPI | API documentation |
| Clean Architecture | Architecture pattern |

## Future Enhancements

- Structured logging (Serilog)
- Unit & integration tests
- Entity Framework Core integration
- CQRS with MediatR
- Pagination and filtering
- Background services

## License

MIT License - see [LICENSE](LICENSE) file for details.
