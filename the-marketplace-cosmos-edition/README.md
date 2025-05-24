# The Marketplace Microservices API

A microservices-based marketplace platform demonstrating clean architecture and event-driven design principles.

## Services

### ListingService

Manages product/service listings, categories, and tags. Core entities include:

- **Listings**: Products or services offered for sale
- **Categories**: Hierarchical categorization of listings
- **Tags**: Labels for additional classification and search

### SearchService

Provides unified search capabilities across all marketplace entities using OpenSearch:

- Full-text search with relevancy scoring
- Filtering by various criteria
- Indexing of listings and other entities

## Architecture

The solution follows Clean Architecture principles with a clear separation of concerns:

- **API Layer**: Controllers and DTOs
- **Application Layer**: Business logic, commands, queries, and validations
- **Domain Layer**: Entities and business rules
- **Infrastructure Layer**: Database access, external service integration

### Event-Driven Communication

Services communicate using an event-driven approach via Azure Service Bus:

- ListingService publishes events when listings are created, updated, published, or deleted
- SearchService consumes these events to update its search indexes

## Technology Stack

- **.NET 8**: Core framework
- **Entity Framework Core**: ORM for PostgreSQL
- **OpenSearch**: Search engine
- **MassTransit**: Service bus abstraction
- **MediatR**: In-process messaging
- **FluentValidation**: Input validation
- **Docker**: Containerization

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose

### Running the Application

```bash
# Start the services using Docker Compose
docker-compose up
```

This will start:
- PostgreSQL database
- OpenSearch engine
- OpenSearch Dashboards
- ListingService API
- SearchService API

### Development Setup

For local development:

```bash
# Start dependencies
docker-compose up postgres opensearch opensearch-dashboards

# Run the ListingService
cd src/Services/ListingService/API/ListingService.Api
dotnet run

# Run the SearchService
cd src/Services/SearchService/API/SearchService.Api
dotnet run
```

## Testing

Run unit tests:

```bash
dotnet test
```

## API Endpoints

### ListingService

- `GET /api/listings` - Get all listings (paged)
- `GET /api/listings/{id}` - Get a specific listing
- `POST /api/listings` - Create a new listing
- `PUT /api/listings/{id}` - Update a listing
- `DELETE /api/listings/{id}` - Delete a listing
- `POST /api/listings/{id}/publish` - Publish a listing

- `GET /api/categories` - Get all categories (paged)
- `GET /api/categories/root` - Get root categories
- `GET /api/categories/{id}` - Get a specific category
- `GET /api/categories/{id}/subcategories` - Get subcategories of a category
- `POST /api/categories` - Create a new category
- `PUT /api/categories/{id}` - Update a category
- `DELETE /api/categories/{id}` - Delete a category

### SearchService

- `GET /api/search` - Search across all entities