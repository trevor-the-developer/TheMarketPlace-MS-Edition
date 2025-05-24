# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Quick Start
```bash
# Start infrastructure and run all services
./run-dev.sh all

# Start infrastructure only, then run services individually
docker-compose up -d postgres opensearch opensearch-dashboards
./run-dev.sh auth      # Authentication Service (port 5000)
./run-dev.sh listing   # Listing Service (port 5001)  
./run-dev.sh search    # Search Service (port 5002)
```

### Essential Commands
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Restore packages
dotnet restore
```

### Service URLs
- Authentication Service: http://localhost:5000/swagger
- Listing Service: http://localhost:5001/swagger
- Search Service: http://localhost:5002/swagger
- OpenSearch Dashboard: http://localhost:5601

## Architecture Overview

This is a microservices-based marketplace platform following Clean Architecture and event-driven design principles.

### Core Services
- **AuthenticationService**: JWT authentication, user management
- **ListingService**: Product/service listings, categories, tags
- **SearchService**: OpenSearch-powered unified search across entities
- **DocumentProcessor**: Azure Function for document processing

### Clean Architecture Layers
Each service follows this structure:
```
Services/[ServiceName]/
├── API/                    # Controllers, Program.cs, DTOs
├── Core/
│   ├── Application/        # Commands, Queries, Handlers, Interfaces
│   └── Domain/            # Entities, Business Rules
└── Infrastructure/
    └── Persistence/       # DbContext, Repositories, Configurations
```

### Event-Driven Communication
- **Message Bus**: MassTransit with Azure Service Bus
- **Event Types**: Strongly-typed records in `Services.Core.Events`
- **Pattern**: Services publish domain events, other services consume as needed
- **Example**: ListingService publishes `ListingCreated` → SearchService indexes

### Key Development Patterns

**CQRS with MediatR**:
- Commands for write operations, Queries for reads
- Handlers implement `IRequestHandler<TRequest, TResponse>`
- Automatic registration via assembly scanning

**Repository Pattern**:
- Interfaces in Application layer, implementations in Persistence
- Generic base repository with specific implementations
- Async/await throughout

**Service Registration**:
- Extension methods per layer: `AddApplicationServices()`, `AddPersistenceServices()`
- Typed configuration classes per service
- Environment-specific settings

**Shared Components** (`Services.Core`):
- `BaseEntity`: Common properties (Id, timestamps, IsActive)
- `ServiceResponse<T>`: Standardized API responses
- Event contracts and service bus constants
- Cross-cutting concern abstractions

### Working with This Codebase

**Adding New Features**:
1. Follow CQRS pattern with Command/Query handlers
2. Use FluentValidation for input validation
3. Implement AutoMapper profiles for DTO mapping
4. Publish domain events for cross-service communication

**Database Changes**:
- Each service has its own PostgreSQL database
- Use Entity Framework migrations per service
- Follow existing entity configuration patterns

**Testing**:
- xUnit framework with Moq and Shouldly
- Unit tests in `tests/Demo.UnitTests/Services/[ServiceName].Application.UnitTests/`
- Test structure mirrors source code organization

**Authentication**:
- JWT Bearer tokens from AuthenticationService
- Use `ICurrentUserService` for user context in other services
- Role-based authorization (Admin, Seller roles)