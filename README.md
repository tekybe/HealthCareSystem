# Healthcare System Architecture

This document serves as the single source of truth for the Healthcare System project. It explains the architecture, design patterns, principles, testing strategy, logging, and rationale behind all design decisions.

## Project Overview

The Healthcare System is a clean architecture-based ASP.NET Core 8.0 application that retrieves patient information through a well-structured REST API. The system implements enterprise-level patterns including CQRS, Mediator, Repository Pattern, and Dependency Injection with comprehensive logging and validation.

## Project Structure

The project is organized into four distinct layers, each with a specific responsibility:

```
src/
├── HealthcareSystem.Domain/          Domain layer - business entities and interfaces
├── HealthcareSystem.Application/     Application layer - business logic, queries, validators
├── HealthcareSystem.Infrastructure/  Infrastructure layer - data access, repositories
└── HealthcareSystem.API/             API layer - controllers, dependency injection, middleware

tests/
├── HealthcareSystem.Tests/           Unit tests for application logic
└── HealthcareSystem.IntegrationTests/ End-to-end integration tests
```

## Program Flow Sequence

When a request arrives at the system, it follows a specific sequence of steps:

1. **HTTP Request Arrives**: A GET request to `/api/patients/{id}` reaches the `PatientsController`.

2. **Controller Receives Request**: The controller receives the patient ID and creates a `GetPatientQuery` with the ID.

3. **MediatR Sends Query**: The controller sends the query through MediatR's `Send()` method. MediatR is a mediator implementation that decouples the controller from the handler.

4. **Validation Pipeline Executes**: Before reaching the handler, the request passes through the validation pipeline behavior. This is where `ValidationBehavior<TRequest, TResponse>` intercepts the request. The validator checks if the patient ID is valid. If validation fails, a `ValidationException` is thrown immediately. Logging records whether validation passed or failed.

5. **Handler Processes Request**: If validation passes, MediatR routes the request to `GetPatientQueryHandler`. The handler receives the query and processes it.

6. **Repository Query Executes**: The handler calls `IPatientRepository.GetByIdAsync(id)` to retrieve the patient from the in-memory data store. Logging records the query and result.

7. **Response is Built**: If the patient exists, the handler creates a `GetPatientResponse` DTO containing only the necessary fields (NHSNumber, Name, DateOfBirth, GPPractice). If not found, a failure result is created.

8. **Result Returns to Controller**: The handler returns a `Result<GetPatientResponse>` object containing either success data or error information.

9. **Controller Sends HTTP Response**: The controller checks the result. If successful, it returns HTTP 200 OK with the patient data. If not found, it returns HTTP 404 Not Found with an error message. Logging records the final outcome.

10. **HTTP Response Sent**: The API sends the JSON response back to the client.

11. **Logging Throughout**: At each stage, Serilog logs the operation (Information level for key operations, Debug for repository queries, Warning for failures).

This flow ensures that every request is validated, logged, and handled consistently through a single mediator.

## Core Architecture Patterns

### Clean Architecture

Clean Architecture organizes the system into concentric layers where each layer depends only on inner layers, never outward. This ensures business logic remains independent of frameworks.

**Why We Use It**: Clean Architecture provides dependency inversion - the core business logic doesn't know about ASP.NET Core, Entity Framework, or any external library. We can test business logic in isolation without any framework dependencies. If we need to change the web framework, the business logic remains untouched.

**Implementation**: 
- Domain layer defines core entities and interfaces
- Application layer contains query handlers and validation
- Infrastructure layer implements repository interfaces
- API layer is the thin outermost shell handling HTTP concerns

### CQRS (Command Query Responsibility Segregation)

CQRS separates read operations (queries) from write operations (commands). Currently, the system implements the Query part - `GetPatientQuery` with its handler.

**Why We Use It**: This pattern creates a clear distinction between retrieving data and modifying data. Each operation has a dedicated handler with specific concerns. A Query handler optimizes for reading; a Command handler optimizes for persisting. This separation makes the system easier to understand and modify.

**Implementation**: 
- Queries like `GetPatientQuery` implement `IRequest<Result<GetPatientResponse>>`
- Handlers like `GetPatientQueryHandler` implement `IRequestHandler<GetPatientQuery, Result<GetPatientResponse>>`
- MediatR routes requests to appropriate handlers

### Mediator Pattern

The Mediator pattern decouples components by providing a central object (mediator) through which components communicate rather than directly with each other.

**Why We Use It**: The controller doesn't directly call the handler. Instead, it calls `_mediator.Send(query)`. This decouples the controller from specific handler implementations. We can change handler logic without touching the controller. We can add cross-cutting concerns (logging, validation, performance monitoring) through pipeline behaviors without modifying handlers.

**Implementation**: 
- MediatR is configured in Program.cs with `builder.Services.AddMediatR()`
- Controllers inject `IMediator` and use `_mediator.Send()` to process requests
- Pipeline behaviors intercept all requests for cross-cutting concerns

### Repository Pattern

The Repository Pattern abstracts data access logic behind an interface, providing a collection-like API to the application layer.

**Why We Use It**: The application layer depends on `IPatientRepository` interface, not concrete database implementations. We can swap in-memory repository with SQL database repository without changing application logic. Testing becomes easier - we mock the repository instead of mocking entire database layers.

**Implementation**: 
- `IPatientRepository` interface defines `GetByIdAsync(int id)`
- `PatientRepository` implements the interface with in-memory data store
- Application layer receives interface through dependency injection

### Dependency Injection

Dependency Injection (DI) means dependencies are provided from outside rather than created internally. ASP.NET Core's built-in container manages object lifetimes and creation.

**Why We Use It**: Components don't create their own dependencies. The container creates them based on configuration. This enables testing (we inject mocks), loose coupling (components depend on abstractions), and flexible configuration (different implementations for different environments).

**Implementation**: 
- `Program.cs` registers all services: `builder.Services.AddScoped<IPatientRepository, PatientRepository>()`
- Controllers receive dependencies through constructor parameters
- ASP.NET Core container resolves all transitive dependencies

### Validation Pipeline Behavior

Pipeline behaviors wrap request processing, allowing us to execute code before and after handlers. Validation behavior runs before the handler, throwing exceptions if validation fails.

**Why We Use Validation in Middleware**: Validation isn't scattered across handlers. A single pipeline behavior validates all requests using registered validators. This follows DRY principle - write validation logic once, apply everywhere. If validation rules change, we update one validator, not multiple handlers. The benefit is consistency and maintainability.

**Implementation**: 
- `ValidationBehavior<TRequest, TResponse>` implements `IPipelineBehavior<TRequest, TResponse>`
- Registered in Program.cs as transient pipeline behavior
- `GetPatientQueryValidator` validates `GetPatientQuery` before handler executes
- Logs validation success/failure

## Logging Strategy

Logging provides visibility into system behavior without writing to debuggers or console. Serilog is configured as the logging framework.

### Logging Configuration

Serilog is configured in `Program.cs` with:
- Console sink for output
- Information level as minimum (Information, Warning, Error)
- Structured format: `[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}`

### Where Logging Occurs

**Controller Layer**: 
- Information: HTTP request received with PatientId
- Information: Patient retrieved successfully
- Warning: Patient retrieval failed with error message

**Validation Behavior**: 
- Information: Validation started with request name
- Debug: No validators found
- Warning: Validation failed with error details
- Information: Validation passed

**Application Handler**: 
- Information: Query processing started
- Warning: Patient not found
- Information: Patient found with NHS number

**Repository Layer**: 
- Debug: Querying for patient ID
- Debug: Patient found/not found result

### Why This Approach

Structured logging with Serilog enables searching logs by properties (e.g., find all failed retrievals for PatientId 5). The message templates create consistent log entries. Logging at appropriate levels (Information for user-facing operations, Debug for internal queries, Warning for failures) makes logs actionable - operators see important events without noise.

## SOLID Principles

SOLID principles guide design decisions ensuring code is maintainable, testable, and flexible to change.

### Single Responsibility Principle (SRP)

Each class has one reason to change, focusing on a single responsibility.

**Implementation**:
- `GetPatientQuery`: Only represents a query request
- `GetPatientQueryHandler`: Only handles the query logic
- `GetPatientQueryValidator`: Only validates query input
- `PatientRepository`: Only accesses patient data
- `ValidationBehavior`: Only validates requests
- Controllers only coordinate HTTP concerns, not business logic

### Open/Closed Principle (OCP)

Classes are open for extension but closed for modification. New functionality is added through extension, not changing existing code.

**Implementation**:
- Adding a new query (e.g., `GetAllPatientsQuery`) doesn't modify `GetPatientQuery` or validation behavior
- New validators are created without modifying `ValidationBehavior`
- The pipeline behavior architecture allows adding concerns (logging, caching, performance monitoring) without changing handlers
- Repository interface allows new implementations (SQL database, external API) without changing handlers

### Liskov Substitution Principle (LSP)

Subtypes must be substitutable for base types without breaking functionality.

**Implementation**:
- Any `IPatientRepository` implementation can replace `PatientRepository`
- Any handler implementing `IRequestHandler<GetPatientQuery, Result<GetPatientResponse>>` can replace `GetPatientQueryHandler`
- Any validator implementing `IValidator<GetPatientQuery>` can replace `GetPatientQueryValidator`
- Any pipeline behavior implementing `IPipelineBehavior<,>` works with existing infrastructure

### Interface Segregation Principle (ISP)

Clients depend on specific interfaces rather than general purpose interfaces.

**Implementation**:
- `IPatientRepository` exposes only patient-related operations (not general `IRepository`)
- `IValidator<T>` is specific to request type (not generic validation interface)
- `IPipelineBehavior<TRequest, TResponse>` is specific to request/response types
- Handlers depend on `IMediator`, not an interface exposing all mediator capabilities

### Dependency Inversion Principle (DIP)

High-level modules depend on abstractions, not low-level modules. Both depend on abstractions.

**Implementation**:
- Controllers depend on `IMediator` (abstraction), not concrete MediatR implementation details
- Handlers depend on `IPatientRepository` (abstraction), not concrete `PatientRepository` class
- Validators depend on `IValidator<T>` (abstraction)
- Pipeline behaviors depend on `IPipelineBehavior<,>` (abstraction)
- All dependencies flow inward toward abstractions in the Domain layer

## DRY Principle (Don't Repeat Yourself)

Code is written once and reused rather than duplicated.

**Implementation**:

1. **Single Validator, Multiple Uses**: `GetPatientQueryValidator` runs automatically through the pipeline. It's not duplicated in the handler or controller.

2. **Pipeline Behavior for Cross-Cutting Concerns**: Validation, logging, and future concerns (caching, rate limiting) are implemented once in pipeline behaviors, not scattered across handlers.

3. **Result Pattern for Consistency**: The `Result<T>` and `Result` types define success/failure structure once. All handlers return this consistent structure.

4. **Repository Abstraction**: Data access logic is in one place. Multiple handlers can use the same repository without duplicating queries.

5. **DTO Mapping**: Response mapping follows one pattern across all handlers. Field selection (NHSNumber, Name, DateOfBirth, GPPractice) is centralized in the DTO class.

## Testing Strategy

The project includes two levels of testing: unit tests and integration tests.

### Unit Testing

Unit tests verify individual components in isolation using mocks.

**What We Test**:
- `GetPatientQueryHandler` with mocked `IPatientRepository`
- `GetPatientQueryValidator` validation rules
- Individual validators with FluentValidation

**Test Examples**:
- Handler with valid patient ID returns success
- Handler with invalid patient ID returns failure
- Validation rejects invalid inputs
- Repository is called with correct parameters

**Why This Approach**: Unit tests are fast (milliseconds), run without database or external services, and provide quick feedback during development. They verify each component works correctly in isolation.

**Test Count**: 26 unit tests covering handlers, validators, and business logic.

### Integration Testing

Integration tests verify the entire request/response cycle using a test web application.

**What We Test**:
- Full HTTP request to controller
- Request passes through validation, handler, and repository
- Response contains expected data and status code
- Error cases return correct HTTP status and error message

**Test Setup**: Uses `WebApplicationFactory<Program>` to create a test host with real services (except persistence).

**Why This Approach**: Integration tests verify components work together correctly. They catch issues that unit tests miss - improper dependency injection, incorrect HTTP status codes, serialization problems.

**Test Count**: 7 integration tests covering happy paths, error cases, and validation failures.


## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Logging**: Serilog 3.1.1 with console sink
- **Mediator**: MediatR 12.2.0 for request/response handling
- **Validation**: FluentValidation 11.9.1 for fluent validator definitions
- **Testing**: xUnit 2.6.6, Moq 4.20.70, FluentAssertions 6.12.0
- **API Documentation**: Swashbuckle 6.5.0 for Swagger/OpenAPI

