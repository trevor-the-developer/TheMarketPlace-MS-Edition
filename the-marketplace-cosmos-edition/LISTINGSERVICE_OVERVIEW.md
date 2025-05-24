# ListingService Overview

## Current State and Capabilities

The ListingService is a microservice within the marketplace-msapi solution that manages listings, categories, and tags for a marketplace platform. It follows a clean architecture pattern with separate API, Application, Domain, and Infrastructure layers.

### Core Capabilities

* **Listing Management**
  * Create listings with title, description, price, location
  * Update existing listings
  * Delete listings
  * Publish listings (changing status from Draft to Published)
  * Query listings with filtering options (by category, seller, price range, location)
  * View detailed information for a specific listing

* **Category Management**
  * Create hierarchical categories (parent-child relationships)
  * Update category information
  * Delete categories
  * Query all categories
  * Query root categories (categories without parents)
  * Query subcategories of a specific category

* **Tag System**
  * Associate tags with listings
  * Repository structure for tag management

* **Image Support**
  * Store multiple images per listing
  * Image association with listings

* **Event Publishing**
  * Publishes ListingCreated event when a new listing is created
  * Publishes ListingPublished event when a listing status changes to Published
  * Events are picked up by SearchService for indexing

### Architecture Details

* **Domain Layer**
  * Core entities: Listing, Category, Tag, ListingImage
  * Domain models with rich properties and relationships
  * ListingStatus enum (Draft, Published, Sold, Archived)

* **Application Layer**
  * Implements CQRS pattern with MediatR
  * Command handlers for create, update, delete, and publish operations
  * Query handlers for fetching listings and categories
  * DTOs for data transfer between layers
  * FluentValidation for input validation
  * AutoMapper for object mapping

* **Infrastructure Layer**
  * EntityFrameworkCore for data persistence
  * Repository implementations for each entity
  * Entity configurations for database schema
  * Seed data functionality for categories

* **API Layer**
  * RESTful controllers for listings and categories
  * Standardized response formats
  * Proper HTTP status codes for various scenarios
  * Input validation

### Integration Points

* **SearchService Integration**
  * Publishes events that SearchService consumes for indexing
  * Enables searchable listings across the marketplace

* **Core Services Integration**
  * Uses shared models from Services.Core
  * Implements standardized pagination, response formats

### Current Status

The ListingService is structurally complete with all necessary components implemented. It supports the full lifecycle of marketplace listings from creation to publishing. There are some minor code issues that need to be addressed for full buildability, mostly related to pagination implementations and property mappings.

### Next Steps

* Fix remaining code issues in GetListingsQueryHandler and GetCategoriesQueryHandler
* Complete the migration from MediatR to Wolverine.Fx as outlined in the migration plan
* Enhance event publishing using Wolverine's built-in message bus capabilities
* Add proper testing coverage for all commands and queries