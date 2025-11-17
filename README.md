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
│   ├── Controllers/          # AuthController, BooksController, BooksV2Controller, CacheTestController
│   ├── Program.cs            # Startup & DI
│   └── appsettings.json      # Configuration
├── CursorDemo.Application/
│   ├── Configuration/        # JwtSettings
│   ├── DTOs/                 # BookDto, CreateBookDto, LoginDto, TokenResponseDto
│   ├── Interfaces/           # IBookService, ITokenService, ICacheService
│   └── Services/             # BookService
├── CursorDemo.Domain/
│   ├── Entities/             # Book
│   └── Interfaces/           # IBookRepository
└── CursorDemo.Infrastructure/
    ├── Repositories/         # InMemoryBookRepository
    └── Services/             # TokenService, MemoryCacheService, BackgroundBookRefresherService
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
- ✅ **Pagination** - Page-based navigation with configurable page size
- ✅ **Filtering** - Search by title, author, or ISBN
- ✅ **Sorting** - Sort by any field in ascending or descending order
- ✅ **Serilog Structured Logging** - Console and file logging with structured output
- ✅ **Background Worker** - Long-running hosted service for periodic tasks
- ✅ **In-Memory Caching** - Performance optimization with configurable cache expiration and pattern-based invalidation

## Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/auth/login` | ❌ | Authenticate and get JWT token |
| `GET` | `/api/v1/books` | ✅ | Get books (with pagination, filtering, sorting) |
| `GET` | `/api/v1/books/{id}` | ✅ | Get book by ID |
| `POST` | `/api/v1/books` | ✅ | Create a new book |
| `POST` | `/api/cachetest/test` | ✅ | Test cache (set/get value) |
| `GET` | `/api/cachetest/get/{key}` | ✅ | Get cached value by key |
| `DELETE` | `/api/cachetest/remove/{key}` | ✅ | Remove cache entry by key |
| `DELETE` | `/api/cachetest/remove-pattern` | ✅ | Remove cache entries by pattern |

**Demo Credentials:**
- Username: `elif`
- Password: `1234`

## Backend API Versioning

The API uses URL segment-based versioning with the `Asp.Versioning` library. Versions are specified in the URL path: `/api/v1/...` or `/api/v2/...`.

**Configuration:**
- Default version: 1.0
- Versioning strategy: URL segment (`UrlSegmentApiVersionReader`)
- Swagger generates separate documentation for each version

**Controller Versioning Approaches:**

1. **Single Controller, Multiple Versions** - `AuthController` supports both v1.0 and v2.0:
   ```csharp
   [ApiVersion("1.0")]
   [ApiVersion("2.0")]
   [Route("api/v{version:apiVersion}/[controller]")]
   ```

2. **Separate Controllers** - `BooksController` (v1.0) and `BooksV2Controller` (v2.0) for different implementations.

**Versioned Endpoints:**
- `POST /api/v1/auth/login` or `/api/v2/auth/login`
- `GET /api/v1/books` (v1.0) or `/api/v2/BooksV` (v2.0)
- `GET /api/v1/books/{id}` (v1.0) or `/api/v2/BooksV/{id}` (v2.0)
- `POST /api/v1/books` (v1.0) or `/api/v2/BooksV` (v2.0)

Swagger UI displays a version selector to switch between API versions.

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

## Pagination, Filtering & Sorting

The `GET /api/books` endpoint supports pagination, filtering, and sorting through query parameters. All parameters are validated and work seamlessly with the global error handling system.

### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number (minimum: 1) |
| `pageSize` | int | 10 | Items per page (range: 1-100) |
| `search` | string | null | Search term (filters by title, author, or ISBN) |
| `sortBy` | string | null | Field to sort by: `title`, `author`, `isbn`, `publishedDate`, `createdAt` |
| `desc` | bool | false | Sort in descending order |

### Example Request

```
GET /api/books?page=1&pageSize=10&search=clean&sortBy=title&desc=false
```

### Example Response

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Clean Code",
      "author": "Robert C. Martin",
      "isbn": "978-0132350884",
      "publishedDate": "2008-08-11T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 12,
  "totalPages": 2
}
```

Invalid pagination parameters (e.g., `page=0` or `pageSize=200`) return a standardized error response with validation details, consistent with other API errors.

## Logging (Serilog)

The application uses Serilog for structured logging, providing rich, structured log output that's easy to query and analyze. Serilog is configured at startup and automatically captures all application events.

**Configuration:**
- **Sinks**: Console (for development) and File (for persistence)
- **Log File**: `logs/app.log` (daily rolling)
- **Minimum Level**: Information

**What Gets Logged:**
- Application lifecycle events (startup, shutdown)
- HTTP requests (method, path, status code, response time)
- Exceptions and errors (with full stack traces)
- Background worker execution
- Cache operations (HIT, MISS, SET, INVALIDATE)
- All application events at Information level and above

**Example Log Output:**
```
[2024-01-15 10:30:00 INF] Starting web application
[2024-01-15 10:30:01 INF] Application started successfully
[2024-01-15 10:30:15 INF] HTTP GET /api/v1/books responded 200 in 45ms
[2024-01-15 10:30:15 INF] Cache MISS for key: books:paged:page:1:pageSize:10:desc:False. Fetching from repository...
[2024-01-15 10:30:15 INF] Cached result for key: books:paged:page:1:pageSize:10:desc:False (expires in 30 seconds)
[2024-01-15 10:30:30 INF] Background refresh executed at 2024-01-15 10:30:30
```

Serilog is configured in `Program.cs` and `appsettings.json`, allowing easy customization of log levels and output destinations.

## Background Worker

The application includes a background worker service (`BackgroundBookRefresherService`) that runs periodic tasks independently of HTTP requests. Background workers in .NET are implemented as `IHostedService` and run for the lifetime of the application.

**Features:**
- Runs every 30 seconds automatically
- Executes independently of API requests
- Uses dependency injection to access repositories and services
- Logs all execution events via Serilog
- Gracefully shuts down when the application stops

**Example Startup Logs:**
```
[2024-01-15 10:30:00 INF] BackgroundBookRefresherService is starting.
[2024-01-15 10:30:00 INF] BackgroundBookRefresherService started. Refresh interval: 30 seconds
[2024-01-15 10:30:30 INF] Background refresh executed at 2024-01-15 10:30:30
[2024-01-15 10:31:00 INF] Background refresh executed at 2024-01-15 10:31:00
```

The background service is registered in `Program.cs` using `AddHostedService<T>()` and automatically starts when the application launches.

## Caching

The application implements in-memory caching to improve performance by reducing database/repository calls. Caching is implemented using the `ICacheService` interface with a `MemoryCacheService` implementation.

### Architecture

**Interface Layer (Application):**
- `ICacheService` - Defines caching operations: `Get<T>`, `Set<T>`, `Remove`, and `RemoveByPattern`

**Implementation Layer (Infrastructure):**
- `MemoryCacheService` - In-memory cache implementation using `IMemoryCache` from .NET

### Implementation Details

**Cache Service Features:**
- Generic type support for caching any object type
- Configurable expiration times
- Pattern-based cache invalidation (e.g., `books:*`)
- Automatic key registry for pattern matching

**BookService Caching:**
- **List Results**: `GetBooksPagedAsync` results are cached for 30 seconds
- **Cache Key Generation**: Keys are generated based on pagination parameters (page, pageSize, search, sortBy, desc) to ensure different queries are cached separately
- **Cache Invalidation**: When a new book is created via `CreateBookAsync`, all book list caches are invalidated using pattern-based removal (`books:*`)
- **Cache Logging**: All cache operations (HIT, MISS, SET, INVALIDATE) are logged via Serilog for monitoring and debugging

**Example Cache Keys:**
```
books:paged:page:1:pageSize:10:desc:False
books:paged:page:1:pageSize:10:search:clean:sortBy:title:desc:False
```

### Configuration

Caching is registered in `Program.cs`:

```csharp
// Caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

The `MemoryCacheService` is registered as a singleton since it manages the cache state, while `IMemoryCache` is provided by the framework.

### Usage Example

```csharp
public class BookService : IBookService
{
    private readonly ICacheService _cacheService;
    
    public async Task<PagedResult<BookDto>> GetBooksPagedAsync(PaginationParameters parameters)
    {
        var cacheKey = GenerateCacheKey(parameters);
        
        // Try cache first
        var cached = _cacheService.Get<PagedResult<BookDto>>(cacheKey);
        if (cached != null) return cached;
        
        // Fetch from repository if not cached
        var result = await _bookRepository.GetPagedAsync(...);
        
        // Cache for 30 seconds
        _cacheService.Set(cacheKey, result, TimeSpan.FromSeconds(30));
        
        return result;
    }
    
    public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
    {
        var book = await _bookRepository.AddAsync(...);
        
        // Invalidate all book caches
        _cacheService.RemoveByPattern("books:*");
        
        return book;
    }
}
```

### Cache Logging

All cache operations are logged via Serilog for monitoring and debugging:

**Example Log Output:**
```
[2024-01-15 10:30:15 INF] Cache MISS for key: books:paged:page:1:pageSize:10:desc:False. Fetching from repository...
[2024-01-15 10:30:15 INF] Cached result for key: books:paged:page:1:pageSize:10:desc:False (expires in 30 seconds)
[2024-01-15 10:30:20 INF] Cache HIT for key: books:paged:page:1:pageSize:10:desc:False
[2024-01-15 10:30:25 INF] Cache invalidated for pattern: books:*
```

### Testing Cache

**Method 1: Automatic Logging (Recommended)**
1. Call `GET /api/v1/books?page=1&pageSize=10` (first call)
   - Check logs: `Cache MISS` - data fetched from repository
2. Call the same endpoint again (within 30 seconds)
   - Check logs: `Cache HIT` - data served from cache
3. Create a new book via `POST /api/v1/books`
   - Check logs: `Cache invalidated` - all book caches cleared
4. Call `GET /api/v1/books` again
   - Check logs: `Cache MISS` - cache was cleared, fetching fresh data

**Method 2: Cache Test Endpoints**
Use the `CacheTestController` endpoints to manually test cache operations:
- `POST /api/cachetest/test?key=test:key&value=test-value&expirationSeconds=30` - Set and get a test value
- `GET /api/cachetest/get/{key}` - Retrieve a cached value
- `DELETE /api/cachetest/remove/{key}` - Remove a specific cache entry
- `DELETE /api/cachetest/remove-pattern?pattern=books:*` - Remove all entries matching a pattern

### Benefits

- **Performance**: Reduces repository calls for frequently accessed data
- **Scalability**: Helps handle high-traffic scenarios by serving cached responses
- **Flexibility**: Pattern-based invalidation allows selective cache clearing
- **Observability**: Comprehensive logging for cache operations (HIT/MISS/SET/INVALIDATE)
- **Clean Architecture**: Caching logic is abstracted behind an interface, making it easy to swap implementations (e.g., Redis for distributed caching)

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
| Serilog | Structured logging |
| Swagger/OpenAPI | API documentation |
| Clean Architecture | Architecture pattern |

## Future Enhancements

- Unit & integration tests
- Entity Framework Core integration
- CQRS with MediatR

## License

MIT License - see [LICENSE](LICENSE) file for details.
