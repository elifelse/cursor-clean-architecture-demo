# Cursor Clean Architecture Demo

A .NET 8 implementation of Clean Architecture demonstrating separation of concerns across multiple layers.

## Architecture Overview

This solution follows Clean Architecture principles, organizing code into distinct layers with clear dependencies:

```
┌─────────────────────────────────────┐
│         CursorDemo.Api              │  ← Presentation Layer (Controllers, API endpoints)
│    (ASP.NET Core Web API)           │
└──────────────┬──────────────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼──────┐  ┌─────▼──────────────┐
│ Application │  │  Infrastructure    │  ← Application Logic & External Concerns
│             │  │  (Repositories)    │
└──────┬──────┘  └─────┬──────────────┘
       │                │
       └───────┬────────┘
               │
       ┌───────▼──────┐
       │    Domain    │  ← Core Business Entities & Interfaces
       │  (Entities)  │
       └──────────────┘
```

## Project Structure

### 1. **CursorDemo.Domain** (Core Layer)
The innermost layer containing business entities and interfaces. This layer has no dependencies on other projects.

- **Entities/**: Domain entities (e.g., `Book`)
- **Interfaces/**: Repository interfaces (e.g., `IBookRepository`)

**Key Points:**
- Pure business logic
- No external dependencies
- Defines contracts (interfaces) that other layers implement

### 2. **CursorDemo.Application** (Application Layer)
Contains application-specific business logic, use cases, and DTOs.

- **DTOs/**: Data Transfer Objects for API communication
- **Interfaces/**: Service interfaces (e.g., `IBookService`)
- **Services/**: Application services implementing business logic

**Key Points:**
- Depends only on Domain
- Contains use cases and application workflows
- Defines service interfaces consumed by the API layer

### 3. **CursorDemo.Infrastructure** (Infrastructure Layer)
Implements external concerns like data access, file systems, or third-party services.

- **Repositories/**: Concrete implementations of domain repository interfaces (e.g., `InMemoryBookRepository`)

**Key Points:**
- Depends on Domain (implements domain interfaces)
- Contains concrete implementations
- Can be swapped without affecting other layers (e.g., replace InMemory with Entity Framework)

### 4. **CursorDemo.Api** (Presentation Layer)
The outermost layer containing controllers, API endpoints, and dependency injection configuration.

- **Controllers/**: API controllers (e.g., `BooksController`)
- **Program.cs**: Application startup and DI configuration

**Key Points:**
- Depends on Application and Infrastructure
- Handles HTTP requests/responses
- Configures dependency injection
- No business logic, only orchestration

## Dependency Flow

The dependency rule ensures that dependencies point inward:

- **Api** → **Application** & **Infrastructure**
- **Application** → **Domain**
- **Infrastructure** → **Domain**

This means:
- Domain has no dependencies
- Application depends only on Domain
- Infrastructure depends only on Domain
- Api depends on Application and Infrastructure

## Features

### Book Management
- **GET /api/books**: Retrieve all books
- **GET /api/books/{id}**: Get a specific book by ID
- **POST /api/books**: Create a new book

### Sample Data
The `InMemoryBookRepository` is pre-seeded with sample books for demonstration purposes.

## Getting Started

### Prerequisites
- .NET 8 SDK installed
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Running the Application

1. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run the API:**
   ```bash
   cd CursorDemo.Api
   dotnet run
   ```

4. **Access Swagger UI:**
   Navigate to `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)

### Testing the API

**Get all books:**
```bash
curl https://localhost:5001/api/books
```

**Create a new book:**
```bash
curl -X POST https://localhost:5001/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Design Patterns",
    "author": "Gang of Four",
    "isbn": "978-0201633610",
    "publishedDate": "1994-10-21T00:00:00Z"
  }'
```

## Clean Architecture Benefits

1. **Independence**: Business logic is independent of frameworks, UI, and databases
2. **Testability**: Easy to unit test business logic without external dependencies
3. **Flexibility**: Can swap out infrastructure (database, external services) without changing business logic
4. **Maintainability**: Clear separation of concerns makes code easier to understand and maintain
5. **Scalability**: Easy to add new features without affecting existing code

## Project References

The solution maintains proper dependency flow:

- `CursorDemo.Api.csproj` references:
  - `CursorDemo.Application`
  - `CursorDemo.Infrastructure`

- `CursorDemo.Application.csproj` references:
  - `CursorDemo.Domain`

- `CursorDemo.Infrastructure.csproj` references:
  - `CursorDemo.Domain`

- `CursorDemo.Domain.csproj` has no project references

## Dependency Injection

Services are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IBookRepository, InMemoryBookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
```

This follows the Dependency Inversion Principle - high-level modules (Application) depend on abstractions (interfaces), not concrete implementations.

## Future Enhancements

- Add Entity Framework Core for persistent storage
- Implement CQRS pattern with MediatR
- Add validation using FluentValidation
- Implement unit and integration tests
- Add authentication and authorization
- Implement logging and error handling middleware

## License

See LICENSE file for details.
