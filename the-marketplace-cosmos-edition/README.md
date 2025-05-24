# The Marketplace Microservices API

A microservices-based marketplace platform demonstrating clean architecture and event-driven design principles.

## Services Overview

### ListingService

The ListingService is the core service that manages product/service listings, categories, and tags for the marketplace platform. It follows a clean architecture pattern with separate API, Application, Domain, and Infrastructure layers.

**Core Entities:**
- **Listings**: Products or services offered for sale
- **Categories**: Hierarchical categorisation of listings
- **Tags**: Labels for additional classification and search
- **ListingImages**: Multiple images per listing support

#### Current Capabilities

**Listing Management**
- Create listings with title, description, price, location
- Update existing listings
- Delete listings
- Publish listings (changing status from Draft to Published)
- Query listings with filtering options (by category, seller, price range, location)
- View detailed information for a specific listing

**Category Management**
- Create hierarchical categories (parent-child relationships)
- Update category information
- Delete categories
- Query all categories
- Query root categories (categories without parents)
- Query subcategories of a specific category

**Tag System**
- Associate tags with listings
- Repository structure for tag management

**Image Support**
- Store multiple images per listing
- Image association with listings

**Event Publishing**
- Publishes ListingCreated event when a new listing is created
- Publishes ListingPublished event when a listing status changes to Published
- Events are picked up by SearchService for indexing

### SearchService

Provides unified search capabilities across all marketplace entities using OpenSearch:

- Full-text search with relevancy scoring
- Filtering by various criteria
- Indexing of listings and other entities
- Consumes events from ListingService to update search indexes

## Architecture

The solution follows Clean Architecture principles with a clear separation of concerns:

### Layer Structure

**API Layer**
- RESTful controllers for listings and categories
- Standardised response formats
- Proper HTTP status codes for various scenarios
- Input validation

**Application Layer**
- Implements CQRS pattern with MediatR
- Command handlers for create, update, delete, and publish operations
- Query handlers for fetching listings and categories
- DTOs for data transfer between layers
- FluentValidation for input validation
- AutoMapper for object mapping

**Domain Layer**
- Core entities: Listing, Category, Tag, ListingImage
- Domain models with rich properties and relationships
- ListingStatus enum (Draft, Published, Sold, Archived)
- Business rules enforcement

**Infrastructure Layer**
- EntityFrameworkCore for data persistence with PostgreSQL
- Repository implementations for each entity
- Entity configurations for database schema
- Seed data functionality for categories
- External service integration

### Event-Driven Communication

Services communicate using an event-driven approach via Azure Service Bus:

- ListingService publishes events when listings are created, updated, published, or deleted
- SearchService consumes these events to update its search indexes
- Enables loose coupling between services

### Integration Points

**SearchService Integration**
- Publishes events that SearchService consumes for indexing
- Enables searchable listings across the marketplace

**Core Services Integration**
- Uses shared models from Services.Core
- Implements standardised pagination and response formats

## Technology Stack

- **.NET 8**: Core framework
- **Entity Framework Core**: ORM for PostgreSQL
- **OpenSearch**: Search engine
- **MassTransit**: Service bus abstraction
- **MediatR**: In-process messaging
- **FluentValidation**: Input validation
- **AutoMapper**: Object mapping
- **Docker**: Containerisation

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

**Listings**
- `GET /api/listings` - Get all listings (paged)
- `GET /api/listings/{id}` - Get a specific listing
- `POST /api/listings` - Create a new listing
- `PUT /api/listings/{id}` - Update a listing
- `DELETE /api/listings/{id}` - Delete a listing
- `POST /api/listings/{id}/publish` - Publish a listing

**Categories**
- `GET /api/categories` - Get all categories (paged)
- `GET /api/categories/root` - Get root categories
- `GET /api/categories/{id}` - Get a specific category
- `GET /api/categories/{id}/subcategories` - Get subcategories of a category
- `POST /api/categories` - Create a new category
- `PUT /api/categories/{id}` - Update a category
- `DELETE /api/categories/{id}` - Delete a category

### SearchService

- `GET /api/search` - Search across all entities

## Current Status and Next Steps

### ListingService Status

The ListingService is structurally complete with all necessary components implemented. It supports the full lifecycle of marketplace listings from creation to publishing. There are some minor code issues that need to be addressed for full buildability, mostly related to pagination implementations and property mappings.

### Planned Improvements

- Fix remaining code issues in GetListingsQueryHandler and GetCategoriesQueryHandler
- Complete the migration from MediatR to Wolverine.Fx as outlined in the migration plan
- Enhance event publishing using Wolverine's built-in message bus capabilities
- Add proper testing coverage for all commands and queries
- Expand search capabilities and filtering options
