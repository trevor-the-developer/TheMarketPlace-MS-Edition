# The Marketplace Microservices API - Technical Design Document v2.0

## Executive Summary

**TL;DR:** A modern, event-driven microservices platform built on .NET 8 that provides a scalable marketplace solution with unified API access, real-time capabilities, and clean architecture patterns.

### Key Highlights
- **Architecture:** Event-driven microservices with clean architecture principles
- **Technology Stack:** .NET 8, RabbitMQ/MassTransit, PostgreSQL, OpenSearch, MongoDB
- **Communication:** Hybrid synchronous (HTTP/REST) and asynchronous (messaging) patterns
- **Security:** JWT-based authentication with role-based access control
- **Scalability:** Independent service scaling with database-per-service pattern
- **Infrastructure:** Containerised deployment with comprehensive monitoring

### Business Value Delivered
- **Developer Productivity** - Clear patterns and well-defined service boundaries
- **System Reliability** - Fault tolerance and graceful degradation
- **Business Agility** - Rapid feature development and independent deployment
- **Operational Excellence** - Comprehensive monitoring and automated operations

---

## Table of Contents
- [1. Architecture Overview](#1-architecture-overview)
  - [Key Objectives](#key-objectives)
- [2. Architecture Components](#2-architecture-components)
  - [2.1 Client Applications Layer](#21-client-applications-layer)
  - [2.2 Ingress Layer](#22-ingress-layer)
  - [2.3 API Gateway Layer](#23-api-gateway-layer)
  - [2.4 Authentication & Authorisation Service](#24-authentication--authorisation-service)
  - [2.5 Core Business Services](#25-core-business-services)
    - [2.5.1 Listing Management Service](#251-listing-management-service)
    - [2.5.2 Search Service](#252-search-service)
    - [2.5.3 Document Processor Service](#253-document-processor-service)
  - [2.6 Message Broker Infrastructure](#26-message-broker-infrastructure)
- [3. Data Architecture](#3-data-architecture)
  - [3.1 Database Per Service Pattern](#31-database-per-service-pattern)
  - [3.2 Data Consistency Strategies](#32-data-consistency-strategies)
- [4. Communication Patterns](#4-communication-patterns)
  - [4.1 Synchronous Communication (HTTP/HTTPS)](#41-synchronous-communication-httphttps)
  - [4.2 Asynchronous Communication (Messaging)](#42-asynchronous-communication-messaging)
- [5. Clean Architecture Implementation](#5-clean-architecture-implementation)
  - [5.1 Layer Structure](#51-layer-structure)
  - [5.2 Dependency Management](#52-dependency-management)
- [6. Development and Deployment](#6-development-and-deployment)
  - [6.1 Containerisation Strategy](#61-containerisation-strategy)
  - [6.2 Development Workflow](#62-development-workflow)
  - [6.3 CI/CD Pipeline](#63-cicd-pipeline)
- [7. Monitoring and Observability](#7-monitoring-and-observability)
  - [7.1 Logging Strategy](#71-logging-strategy)
  - [7.2 Metrics and Monitoring](#72-metrics-and-monitoring)
  - [7.3 Distributed Tracing](#73-distributed-tracing)
- [8. Security Considerations](#8-security-considerations)
  - [8.1 Authentication and Authorisation](#81-authentication-and-authorisation)
  - [8.2 Data Protection](#82-data-protection)
- [9. Performance and Scalability](#9-performance-and-scalability)
  - [9.1 Caching Strategy](#91-caching-strategy)
  - [9.2 Scaling Patterns](#92-scaling-patterns)
- [10. Disaster Recovery and Business Continuity](#10-disaster-recovery-and-business-continuity)
  - [10.1 Backup Strategy](#101-backup-strategy)
  - [10.2 High Availability](#102-high-availability)
- [11. Configuration Management](#11-configuration-management)
  - [11.1 Environment Variables](#111-environment-variables)
  - [11.2 Service Configuration](#112-service-configuration)
- [12. Testing Strategy](#12-testing-strategy)
  - [12.1 Testing Approach](#121-testing-approach)
  - [12.2 API Testing with Postman](#122-api-testing-with-postman)
- [13. Future Considerations](#13-future-considerations)
  - [13.1 Planned Enhancements](#131-planned-enhancements)
  - [13.2 Technology Considerations](#132-technology-considerations)
- [14. Conclusion](#14-conclusion)

---

## 1. Architecture Overview

This document outlines the technical architecture for The Marketplace Microservices API, a modern, event-driven distributed system built on .NET 8. The architecture implements microservices patterns with clean architecture principles, utilising RabbitMQ with MassTransit for inter-service communication and a comprehensive technology stack optimised for scalability, maintainability, and resilience.

### Key Objectives

- **Service Independence** - Enable autonomous development, deployment, and scaling of business capabilities
- **Event-Driven Communication** - Implement resilient, asynchronous messaging patterns between services
- **Clean Architecture** - Maintain clear separation of concerns with testable, maintainable code
- **Unified API Access** - Provide consistent client interfaces through API Gateway patterns
- **Data Ownership** - Ensure each service owns and manages its domain data
- **Horizontal Scalability** - Support independent scaling based on service-specific demands
- **Real-time Capabilities** - Enable responsive user experiences through event-driven updates

## 2. Architecture Components

### 2.1 Client Applications Layer

**Supported Client Types:**
- **Web Applications** - Single Page Applications (Angular, React, Vue.js)
- **Mobile Applications** - Native iOS/Android and cross-platform solutions
- **API Consumers** - Third-party integrations and B2B partners

**Access Pattern:**
All client applications access backend services through a unified API Gateway, providing consistency and abstraction from internal service complexity.

**Client Authentication Flow:**
1. Client authenticates with Authentication Service and receives JWT token
2. JWT token is included in subsequent requests to any service
3. API Gateway validates token before forwarding requests
4. Services receive pre-validated requests with user context

### 2.2 Ingress Layer

**Implementation:** NGINX

**Responsibilities:**
- **Load Balancing** - Distribution of incoming requests across multiple gateway instances
- **SSL/TLS Termination** - Secure HTTPS handling and certificate management
- **Request Routing** - Initial traffic direction to appropriate gateway services
- **Rate Limiting** - Protection against abuse and excessive requests
- **Health Checks** - Ensuring traffic routes only to healthy gateway instances

**Configuration:**
- Container-based deployment with Docker
- Integration with container orchestration platforms (Kubernetes/Docker Swarm)
- Automatic SSL certificate management (Let's Encrypt integration)

**NGINX Configuration Example:**
```nginx
# Load balancing configuration
upstream api_gateways {
    server api-gateway-1:8080;
    server api-gateway-2:8080;
    server api-gateway-3:8080;
    # Health check directives
    health_check interval=10 fails=3 passes=2;
}

server {
    listen 443 ssl http2;
    server_name api.marketplace.com;
    
    # SSL configuration
    ssl_certificate /etc/letsencrypt/live/api.marketplace.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.marketplace.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    
    # Rate limiting
    limit_req zone=api_limit burst=20 nodelay;
    
    location / {
        proxy_pass http://api_gateways;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 2.3 API Gateway Layer

**Implementation:** ASP.NET Core

**Core Functions:**
- **Request Routing** - Intelligent routing to appropriate microservices based on URL patterns
- **Request Aggregation** - Combining multiple service calls into unified client responses
- **Protocol Translation** - Converting between external and internal communication formats
- **Authentication Integration** - JWT token validation before forwarding requests to services
- **Cross-Cutting Concerns** - Centralised logging, monitoring, and error handling
- **Circuit Breaker** - Preventing cascade failures through resilience patterns

**Service Endpoints:**
- Authentication Gateway: http://localhost:5000/swagger
- Listing Management Gateway: http://localhost:5001/swagger
- Search Gateway: http://localhost:5002/swagger
- Document Processor Gateway: http://localhost:5003/swagger

**API Gateway Routing Example:**
```csharp
// Routing configuration in Program.cs
app.MapGet("/api/listings/{id}", async (HttpContext context, string id) =>
{
    // Extract and validate JWT token
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    if (!tokenValidator.ValidateToken(token))
    {
        context.Response.StatusCode = 401;
        return;
    }
    
    // Route request to Listing Service
    var response = await httpClient.GetAsync($"http://listing-service/api/listings/{id}");
    
    // Handle response
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        await context.Response.WriteAsync(content);
    }
    else
    {
        context.Response.StatusCode = (int)response.StatusCode;
    }
})
.WithName("GetListing")
.WithOpenApi();
```

### 2.4 Authentication & Authorisation Service

**Technology Stack:**
- ASP.NET Core 8
- JWT Authentication
- Entity Framework Core with PostgreSQL
- Identity framework integration

**Capabilities:**
- **User Management** - Registration, login, profile management
- **JWT Token Management** - Issuance, validation, refresh, and revocation
- **Role-Based Access Control** - Admin, Seller, and Customer roles
- **Email Confirmation** - Secure user verification workflows
- **Multi-Factor Authentication** - Enhanced security options

**Security Features:**
- Secure password hashing (bcrypt/scrypt)
- Token expiration and rotation policies
- Brute force protection
- Account lockout mechanisms

**User Authentication Flow:**
1. User registers with first name, last name, email, password, DOB, and role
2. System sends confirmation email with token
3. User confirms email by sending token back
4. User logs in with email and password
5. System returns JWT access token, refresh token, and expiration
6. User includes access token in all subsequent API requests
7. When access token expires, user requests a new one using refresh token

**Sample JWT Configuration:**
```csharp
// JWT configuration in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
        };
    });
```

### 2.5 Core Business Services

#### 2.5.1 Listing Management Service

**Purpose:** Manages marketplace listings, categories, and product organisation

**Technology Stack:**
- ASP.NET Core 8
- Entity Framework Core with PostgreSQL
- MassTransit for event publishing
- MediatR for internal command/query handling
- FluentValidation for input validation

**Domain Entities:**
- **Listings** - Products and services offered for sale
- **Categories** - Hierarchical product categorisation
- **Tags** - Flexible labeling system for enhanced discoverability

**Event Publishing:**
- `ListingCreated` - New listing added to marketplace
- `ListingUpdated` - Existing listing modified
- `ListingPublished` - Listing becomes publicly visible
- `ListingDeleted` - Listing removed from marketplace

**Entity Relationships:**
```
Listing (1) ---- (*) Tags
Listing (*) ---- (1) Category
Category (1) ---- (*) ChildCategories
```

**Sample Listing Entity:**
```csharp
public class Listing
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Location { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public ListingStatus Status { get; private set; }
    public ICollection<Tag> Tags { get; private set; }
    
    // Domain methods for listing lifecycle management
    public void Publish() { ... }
    public void Update(string title, string description, decimal price) { ... }
    public void Delete() { ... }
}
```

**MediatR Command Example:**
```csharp
// Command definition
public class CreateListingCommand : IRequest<Guid>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Location { get; set; }
    public Guid CategoryId { get; set; }
    public IEnumerable<string> Tags { get; set; }
}

// Command handler
public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, Guid>
{
    private readonly IListingRepository _repository;
    private readonly IBus _bus;
    
    public CreateListingCommandHandler(IListingRepository repository, IBus bus)
    {
        _repository = repository;
        _bus = bus;
    }
    
    public async Task<Guid> Handle(CreateListingCommand command, CancellationToken cancellationToken)
    {
        var listing = new Listing(command.Title, command.Description, command.Price, 
            command.Location, command.CategoryId, command.Tags);
            
        await _repository.AddAsync(listing, cancellationToken);
        
        // Publish domain event via MassTransit
        await _bus.Publish(new ListingCreatedEvent 
        { 
            ListingId = listing.Id,
            Title = listing.Title,
            // Other relevant properties
        }, cancellationToken);
        
        return listing.Id;
    }
}
```

#### 2.5.2 Search Service

**Purpose:** Provides unified search capabilities across marketplace entities

**Technology Stack:**
- ASP.NET Core 8
- OpenSearch for search engine
- MassTransit for event consumption
- Custom indexing strategies

**Search Capabilities:**
- **Full-Text Search** - Content-based search with relevancy scoring
- **Faceted Search** - Filtering by categories, price ranges, ratings
- **Geospatial Search** - Location-based listing discovery
- **Autocomplete** - Real-time search suggestions
- **Advanced Filtering** - Complex query combinations

**Event Consumption:**
- Listens to listing events for real-time index updates
- Maintains search consistency with source data
- Handles bulk re-indexing operations

**OpenSearch Index Example:**
```json
{
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "title": { 
        "type": "text",
        "analyzer": "english",
        "fields": {
          "keyword": { "type": "keyword" }
        }
      },
      "description": { "type": "text", "analyzer": "english" },
      "price": { "type": "float" },
      "location": { "type": "geo_point" },
      "categoryId": { "type": "keyword" },
      "categoryPath": { "type": "keyword" },
      "tags": { "type": "keyword" },
      "createdAt": { "type": "date" },
      "updatedAt": { "type": "date" },
      "status": { "type": "keyword" }
    }
  }
}
```

**Advanced Search Example:**
```csharp
// Advanced search DTO
public class AdvancedSearchRequest
{
    public string Query { get; set; }
    public SearchFilters Filters { get; set; }
    public SearchSort Sort { get; set; }
    public Pagination Pagination { get; set; }
}

public class SearchFilters
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<Guid> CategoryIds { get; set; }
    public string Location { get; set; }
    public List<string> Tags { get; set; }
}

// OpenSearch query builder
public class OpenSearchQueryBuilder
{
    public QueryContainer BuildAdvancedSearchQuery(AdvancedSearchRequest request)
    {
        var queryContainer = new QueryContainer();
        
        // Full-text search across multiple fields
        if (!string.IsNullOrEmpty(request.Query))
        {
            queryContainer &= new MultiMatchQuery
            {
                Fields = new[] { "title^3", "description" },
                Query = request.Query,
                Type = TextQueryType.BestFields,
                Fuzziness = Fuzziness.Auto
            };
        }
        
        // Apply filters if specified
        if (request.Filters != null)
        {
            // Price range filter
            if (request.Filters.MinPrice.HasValue || request.Filters.MaxPrice.HasValue)
            {
                var rangeQuery = new NumericRangeQuery
                {
                    Field = "price",
                    GreaterThanOrEqualTo = request.Filters.MinPrice,
                    LessThanOrEqualTo = request.Filters.MaxPrice
                };
                queryContainer &= rangeQuery;
            }
            
            // Category filter
            if (request.Filters.CategoryIds?.Count > 0)
            {
                queryContainer &= new TermsQuery
                {
                    Field = "categoryId",
                    Terms = request.Filters.CategoryIds.Select(id => id.ToString())
                };
            }
            
            // Tags filter
            if (request.Filters.Tags?.Count > 0)
            {
                queryContainer &= new TermsQuery
                {
                    Field = "tags",
                    Terms = request.Filters.Tags
                };
            }
            
            // Location filter using geo-distance
            if (!string.IsNullOrEmpty(request.Filters.Location))
            {
                // Logic to convert location to coordinates
                var coordinates = GetCoordinatesFromLocation(request.Filters.Location);
                queryContainer &= new GeoDistanceQuery
                {
                    Field = "location",
                    Distance = "10km",
                    Location = new GeoLocation(coordinates.Latitude, coordinates.Longitude)
                };
            }
        }
        
        return queryContainer;
    }
}
```

#### 2.5.3 Document Processor Service

**Purpose:** Handles document generation, processing, and storage

**Technology Stack:**
- ASP.NET Core 8
- Hangfire for background job processing
- MongoDB for document metadata
- MinIO for object storage
- PDF generation libraries

**Processing Capabilities:**
- **PDF Generation** - Creating formatted documents from templates
- **File Upload Management** - Secure file handling and validation
- **Background Processing** - Asynchronous document operations
- **Metadata Management** - Document indexing and retrieval

**Storage Integration:**
- MinIO for scalable object storage
- MongoDB for document metadata and search
- Hangfire dashboard for job monitoring

**Document Generation Flow:**
1. Client requests document generation (e.g., listing PDF)
2. API returns a job ID immediately (202 Accepted)
3. Hangfire processes the job asynchronously
4. Service retrieves necessary data from other services
5. PDF is generated and stored in MinIO
6. Document metadata is stored in MongoDB
7. Client can check job status and download when ready

**Hangfire Job Example:**
```csharp
// Controller endpoint
[HttpPost("generate")]
public IActionResult GenerateDocument([FromBody] DocumentGenerationRequest request)
{
    // Validate request
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
        
    // Enqueue background job
    var jobId = BackgroundJob.Enqueue<IDocumentGenerationService>(
        service => service.GenerateDocumentAsync(request));
        
    // Return accepted with job ID for status checking
    return Accepted(new { JobId = jobId });
}

// Document generation service
public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly IListingServiceClient _listingClient;
    private readonly IMinioStorageService _storageService;
    private readonly IDocumentMetadataRepository _metadataRepo;
    
    public async Task GenerateDocumentAsync(DocumentGenerationRequest request)
    {
        // Retrieve listing data
        var listing = await _listingClient.GetListingAsync(request.ListingId);
        
        // Generate PDF
        var pdfBytes = await GeneratePdfFromTemplate(listing, request.IncludeImages);
        
        // Store in MinIO
        var objectName = $"listings/{request.ListingId}/{Guid.NewGuid()}.pdf";
        await _storageService.UploadObjectAsync("documents", objectName, pdfBytes);
        
        // Store metadata in MongoDB
        var metadata = new DocumentMetadata
        {
            Id = Guid.NewGuid(),
            JobId = JobId.FromExpression(() => GenerateDocumentAsync(request)),
            Type = DocumentType.ListingPdf,
            ReferenceId = request.ListingId,
            ObjectName = objectName,
            ContentType = "application/pdf",
            SizeInBytes = pdfBytes.Length,
            CreatedAt = DateTime.UtcNow,
            Status = DocumentStatus.Completed
        };
        
        await _metadataRepo.AddAsync(metadata);
    }
}
```

### 2.6 Message Broker Infrastructure

**Implementation:** RabbitMQ with MassTransit

**Messaging Patterns:**
- **Publisher/Subscriber** - Event broadcasting to multiple consumers
- **Request/Response** - Synchronous-style messaging over async transport
- **Send/Receive** - Point-to-point command delivery

**MassTransit Configuration:**
- Automatic exchange and queue provisioning
- Message serialisation (JSON)
- Dead letter queue handling
- Retry policies with exponential backoff
- Circuit breaker integration

**Message Types:**
- **Events** - Domain events (ListingCreated, UserRegistered)
- **Commands** - Action requests (ProcessDocument, UpdateIndex)
- **Queries** - Information requests (GetListingDetails)

**MassTransit Configuration Example:**
```csharp
// MassTransit configuration in Program.cs
builder.Services.AddMassTransit(config =>
{
    // Register consumers
    config.AddConsumer<ListingCreatedConsumer>();
    config.AddConsumer<ListingUpdatedConsumer>();
    config.AddConsumer<ListingDeletedConsumer>();
    
    // Configure RabbitMQ
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
        
        // Configure retry policy
        cfg.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        });
        
        // Configure dead letter queue
        cfg.UseDelayedRedelivery(r => 
            r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
        
        // Configure error queue
        cfg.UseExceptionLogger();
        
        // Configure consumers
        cfg.ConfigureEndpoints(context);
    });
});
```

**Event Consumer Example:**
```csharp
// Event definition
public class ListingCreatedEvent
{
    public Guid ListingId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Location { get; set; }
    public Guid CategoryId { get; set; }
    public IEnumerable<string> Tags { get; set; }
}

// Event consumer in Search Service
public class ListingCreatedConsumer : IConsumer<ListingCreatedEvent>
{
    private readonly ISearchIndexService _indexService;
    private readonly ILogger<ListingCreatedConsumer> _logger;
    
    public ListingCreatedConsumer(ISearchIndexService indexService, ILogger<ListingCreatedConsumer> logger)
    {
        _indexService = indexService;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<ListingCreatedEvent> context)
    {
        try
        {
            var listing = context.Message;
            _logger.LogInformation("Indexing new listing {ListingId}", listing.ListingId);
            
            await _indexService.IndexListingAsync(new ListingSearchDocument
            {
                Id = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Location = listing.Location,
                CategoryId = listing.CategoryId,
                Tags = listing.Tags.ToList(),
                CreatedAt = DateTime.UtcNow
            });
            
            _logger.LogInformation("Successfully indexed listing {ListingId}", listing.ListingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing listing {ListingId}", context.Message.ListingId);
            throw; // Rethrow to trigger retry policy
        }
    }
}
```

## 3. Data Architecture

### 3.1 Database Per Service Pattern

Each microservice maintains its own database to ensure data ownership and service independence:

**Authentication Service:**
- **Database:** PostgreSQL
- **Schema:** Users, Roles, Authentication tokens, User profiles
- **Tables:**
  - Users (Id, FirstName, LastName, Email, PasswordHash, DateOfBirth, CreatedAt)
  - Roles (Id, Name, Description)
  - UserRoles (UserId, RoleId)
  - RefreshTokens (Id, UserId, Token, ExpiryDate, CreatedAt, RevokedAt)

**Listing Service:**
- **Database:** PostgreSQL
- **Schema:** Listings, Categories, Tags, Relationships
- **Tables:**
  - Categories (Id, Name, Description, ParentCategoryId, CreatedAt, UpdatedAt)
  - Listings (Id, Title, Description, Price, Location, CategoryId, CreatedAt, UpdatedAt, Status)
  - Tags (Id, Name)
  - ListingTags (ListingId, TagId)

**Search Service:**
- **Database:** OpenSearch
- **Schema:** Search indexes, Aggregations, Search analytics
- **Indexes:**
  - listings-index (Document schema as shown in section 2.5.2)
  - search-analytics (User search patterns, click-through rates, etc.)

**Document Processor:**
- **Database:** MongoDB
- **Schema:** Document metadata, Processing jobs, File references
- **Collections:**
  - DocumentMetadata (Id, Type, ReferenceId, ObjectName, ContentType, SizeInBytes, CreatedAt, Status)
  - ProcessingJobs (Id, Type, Parameters, Status, CreatedAt, CompletedAt, ErrorMessage)

### 3.2 Data Consistency Strategies

**Eventual Consistency:**
- Cross-service data synchronisation through events
- Compensation patterns for handling failures
- Idempotent message processing

**Transactional Boundaries:**
- ACID transactions within individual services
- Saga patterns for distributed transactions
- Outbox pattern for reliable event publishing

**Outbox Pattern Implementation:**
```csharp
// Outbox pattern implementation for reliable event publishing
public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMessageOutbox _outbox;
    
    public async Task AddAsync(Listing listing)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Add listing to database
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
            
            // Add events to outbox (same transaction)
            await _outbox.AddMessageAsync(new ListingCreatedEvent
            {
                ListingId = listing.Id,
                Title = listing.Title,
                // Other properties
            });
            
            // Commit transaction
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

// Background job to process outbox messages
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxProcessingService>();
            
            await outboxService.ProcessPendingMessagesAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

## 4. Communication Patterns

### 4.1 Synchronous Communication (HTTP/HTTPS)

**Use Cases:**
- Client-to-Gateway communication
- Real-time data queries
- Administrative operations
- Health checks and monitoring

**Implementation:**
- RESTful APIs with OpenAPI documentation
- JSON request/response formats
- Standardised error handling
- Request correlation tracking

**Swagger/OpenAPI Example:**
```csharp
// Controller with OpenAPI annotations
[ApiController]
[Route("api/listings")]
[Produces("application/json")]
public class ListingsController : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetListing(Guid id)
    {
        var listing = await _mediator.Send(new GetListingQuery { Id = id });
        
        if (listing == null)
            return NotFound();
            
        return Ok(listing);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(CreateListingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        var command = _mapper.Map<CreateListingCommand>(request);
        var listingId = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetListing), new { id = listingId }, new CreateListingResponse { Id = listingId });
    }
}
```

### 4.2 Asynchronous Communication (Messaging)

**Use Cases:**
- Cross-service event notifications
- Background processing triggers
- System integration workflows
- Data synchronisation

**Message Flow Example:**
```
1. Listing Service publishes ListingCreated event
2. RabbitMQ routes event to subscribers
3. Search Service consumes event and updates index
4. Document Service generates listing PDF
5. Notification Service sends user confirmation
```

**Reliability Features:**
- Message persistence for durability
- Publisher confirms for reliable sending
- Consumer acknowledgments for processing confirmation
- Dead letter queues for error handling

**Event Publication Example:**
```csharp
// Event publication with MassTransit
public class ListingService : IListingService
{
    private readonly IListingRepository _repository;
    private readonly IBus _bus;
    
    public async Task<Guid> CreateListingAsync(CreateListingCommand command)
    {
        // Create and persist listing entity
        var listing = new Listing(command.Title, command.Description, command.Price, 
            command.Location, command.CategoryId, command.Tags);
        
        await _repository.AddAsync(listing);
        
        // Publish event to message broker
        await _bus.Publish(new ListingCreatedEvent
        {
            ListingId = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            Location = listing.Location,
            CategoryId = listing.CategoryId,
            Tags = listing.Tags.Select(t => t.Name).ToList(),
            CreatedAt = DateTime.UtcNow
        });
        
        return listing.Id;
    }
}
```

## 5. Clean Architecture Implementation

### 5.1 Layer Structure

Each microservice follows Clean Architecture principles with distinct layers:

**Domain Layer (Core):**
- Entities and value objects
- Domain services and business rules
- Repository interfaces
- Domain events

**Application Layer:**
- Use cases and business workflows
- Command/Query handlers (MediatR)
- Application services
- DTOs and mapping profiles

**Infrastructure Layer:**
- Data access implementations
- External service integrations
- Message broker implementations
- Cross-cutting concerns

**Presentation Layer:**
- Controllers and API endpoints
- Authentication/authorisation filters
- Request/response models
- Swagger documentation

### 5.2 Dependency Management

**Dependency Injection Container:**
```csharp
// Clean Architecture DI configuration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Register application services
        services.AddScoped<IListingService, ListingService>();
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        return services;
    }
    
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IListingRepository, ListingRepository>();
        
        // Register external services
        services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
        
        // Register database context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            
        return services;
    }
}
```

## 6. Development and Deployment

### 6.1 Containerisation Strategy

**Docker Configuration:**
Each service includes optimised Dockerfile for .NET 8 applications:

```dockerfile
# Multi-stage build for optimized production images
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Listing/Listing.API/Listing.API.csproj", "src/Services/Listing/Listing.API/"]
COPY ["src/Services/Listing/Listing.Application/Listing.Application.csproj", "src/Services/Listing/Listing.Application/"]
COPY ["src/Services/Listing/Listing.Domain/Listing.Domain.csproj", "src/Services/Listing/Listing.Domain/"]
COPY ["src/Services/Listing/Listing.Infrastructure/Listing.Infrastructure.csproj", "src/Services/Listing/Listing.Infrastructure/"]

RUN dotnet restore "src/Services/Listing/Listing.API/Listing.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Listing/Listing.API"
RUN dotnet build "Listing.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Listing.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Listing.API.dll"]
```

**Docker Compose Configuration:**
```yaml
version: '3.8'

services:
  # Infrastructure Services
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - api-gateway
  
  rabbitmq:
    image: rabbitmq:3-management
    environment:
      RABBITMQ_DEFAULT_USER: marketplace
      RABBITMQ_DEFAULT_PASS: password123
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
  
  # Databases
  postgres-auth:
    image: postgres:15
    environment:
      POSTGRES_DB: marketplace_auth
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password123
    ports:
      - "5432:5432"
    volumes:
      - postgres_auth_data:/var/lib/postgresql/data
  
  # Microservices
  auth-service:
    build:
      context: .
      dockerfile: src/Services/Authentication/Authentication.API/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres-auth;Database=marketplace_auth;Username=postgres;Password=password123
      - RabbitMQ__Host=rabbitmq
    depends_on:
      - postgres-auth
      - rabbitmq
    ports:
      - "5000:80"

volumes:
  rabbitmq_data:
  postgres_auth_data:
```

### 6.2 Development Workflow

**Local Development Setup:**
1. Clone repository and navigate to project root
2. Run `docker-compose up -d` to start infrastructure services
3. Run individual services with `dotnet run` or use Visual Studio/Rider
4. Access Swagger documentation at service-specific endpoints
5. Use Postman collections for API testing

**Development Standards:**
- Feature branch workflow with pull requests
- Code review requirements before merging
- Automated code formatting and linting
- Unit test coverage requirements (minimum 80%)

### 6.3 CI/CD Pipeline

**GitHub Actions Workflow:**
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Code Coverage Report
      uses: codecov/codecov-action@v3

  build-and-deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Build Docker images
      run: |
        docker build -t marketplace/auth-service -f src/Services/Authentication/Authentication.API/Dockerfile .
        docker build -t marketplace/listing-service -f src/Services/Listing/Listing.API/Dockerfile .
        
    - name: Deploy to staging
      run: |
        # Deploy to staging environment
        echo "Deploying to staging..."
```

## 7. Monitoring and Observability

### 7.1 Logging Strategy

**Structured Logging with Serilog:**
```csharp
// Serilog configuration
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "ListingService")
        .Enrich.WithProperty("Version", Assembly.GetExecutingAssembly().GetName().Version)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(context.Configuration.GetConnectionString("Elasticsearch")))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "marketplace-logs-{0:yyyy.MM.dd}",
            TypeName = "_doc"
        });
});
```

### 7.2 Metrics and Monitoring

**Prometheus Metrics:**
```csharp
// Custom metrics registration
public static class MetricsExtensions
{
    private static readonly Counter RequestsTotal = Metrics
        .CreateCounter("marketplace_requests_total", "Total number of requests", new[] { "method", "endpoint", "status" });
    
    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("marketplace_request_duration_seconds", "Request duration in seconds", new[] { "method", "endpoint" });
    
    public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
    {
        app.UseMiddleware<MetricsMiddleware>();
        return app;
    }
}

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            RequestsTotal.WithTags(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode.ToString()
            ).Inc();
            
            RequestDuration.WithTags(
                context.Request.Method,
                context.Request.Path
            ).Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
```

### 7.3 Distributed Tracing

**OpenTelemetry Configuration:**
```csharp
// OpenTelemetry tracing setup
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("Marketplace.ListingService")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["Jaeger:Host"];
                options.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"]);
            }));
```

## 8. Security Considerations

### 8.1 Authentication and Authorisation

**JWT Security Best Practices:**
- Short-lived access tokens (15 minutes)
- Refresh token rotation
- Secure token storage recommendations
- Token revocation capabilities

**API Security Middleware:**
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
        
        await _next(context);
    }
}
```

### 8.2 Data Protection

**Encryption Standards:**
- AES-256 for data at rest
- TLS 1.3 for data in transit
- Secure key management with Azure Key Vault
- PII data anonymisation in logs

**Input Validation:**
```csharp
public class CreateListingRequestValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^[a-zA-Z0-9\\s\\-_.,!?]+$");
            
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(1000000);
            
        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

## 9. Performance and Scalability

### 9.1 Caching Strategy

**Multi-Layer Caching:**
```csharp
// Redis distributed caching
public class CachedListingService : IListingService
{
    private readonly IListingService _listingService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedListingService> _logger;
    
    public async Task<ListingDto> GetListingAsync(Guid id)
    {
        var cacheKey = $"listing:{id}";
        var cachedListing = await _cache.GetStringAsync(cacheKey);
        
        if (cachedListing != null)
        {
            _logger.LogInformation("Cache hit for listing {Id}", id);
            return JsonSerializer.Deserialize<ListingDto>(cachedListing);
        }
        
        var listing = await _listingService.GetListingAsync(id);
        
        if (listing != null)
        {
            var serialized = JsonSerializer.Serialize(listing);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }
        
        return listing;
    }
}
```

### 9.2 Scaling Patterns

**Horizontal Pod Autoscaling (Kubernetes):**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: listing-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: listing-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## 10. Disaster Recovery and Business Continuity

### 10.1 Backup Strategy

**Automated Database Backups:**
- Daily full backups with 30-day retention
- Point-in-time recovery capabilities
- Cross-region backup replication
- Automated backup testing and validation

### 10.2 High Availability

**Multi-Region Deployment:**
- Active-passive configuration across regions
- Automated failover mechanisms
- Data replication strategies
- RTO: 15 minutes, RPO: 5 minutes

## 11. Configuration Management

### 11.1 Environment Variables

**Authentication Service:**
```bash
# Database Configuration
ConnectionStrings__DefaultConnection=Host=localhost;Database=marketplace_auth;Username=postgres;Password=password123

# JWT Configuration
JwtSettings__Key=your-super-secret-jwt-signing-key-here
JwtSettings__Issuer=marketplace-api
JwtSettings__Audience=marketplace-client
JwtSettings__ExpiryInMinutes=15

# Email Configuration
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-app-password

# RabbitMQ Configuration
RabbitMQ__Host=localhost
RabbitMQ__Username=marketplace
RabbitMQ__Password=password123
```

### 11.2 Service Configuration

**Listing Service:**
```bash
# Database Configuration
ConnectionStrings__DefaultConnection=Host=localhost;Database=marketplace_listings;Username=postgres;Password=password123

# RabbitMQ Configuration
RabbitMQ__Host=localhost
RabbitMQ__Username=marketplace
RabbitMQ__Password=password123

# Redis Configuration
Redis__ConnectionString=localhost:6379

# External Service URLs
Services__AuthenticationUrl=http://localhost:5000
Services__SearchUrl=http://localhost:5002
```

## 12. Testing Strategy

### 12.1 Testing Approach

**Testing Pyramid Implementation:**
- **Unit Tests (70%)** - Domain logic, services, handlers
- **Integration Tests (20%)** - Database, external APIs, message brokers
- **End-to-End Tests (10%)** - Critical user journeys

**Test Categories:**
```csharp
// Unit test example
[Test]
public async Task CreateListing_WithValidData_ShouldReturnListingId()
{
    // Arrange
    var command = new CreateListingCommand
    {
        Title = "Test Listing",
        Description = "Test Description",
        Price = 100.00m,
        Location = "London",
        CategoryId = Guid.NewGuid()
    };
    
    var mockRepository = new Mock<IListingRepository>();
    var mockBus = new Mock<IBus>();
    
    var handler = new CreateListingCommandHandler(mockRepository.Object, mockBus.Object);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Listing>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### 12.2 API Testing with Postman

**Comprehensive Test Collections:**
- Authentication flow testing
- CRUD operations for all entities
- Error scenario validation
- Performance benchmarking

**Example Test Scripts:**
```javascript
// Postman test script for user registration
pm.test("User registration should succeed", function () {
    pm.response.to.have.status(201);
    
    const responseJson = pm.response.json();
    pm.expect(responseJson).to.have.property('userId');
    pm.expect(responseJson.userId).to.be.a('string');
    
    // Store user ID for subsequent tests
    pm.globals.set("userId", responseJson.userId);
});

pm.test("Response time is less than 2000ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

## 13. Future Considerations

### 13.1 Planned Enhancements

**Short-term (3-6 months):**
- Real-time notifications with SignalR
- Advanced search filters and sorting
- Image upload and processing capabilities
- User rating and review system

**Medium-term (6-12 months):**
- Payment integration (Stripe/PayPal)
- Messaging system between users
- Mobile application development
- Analytics and reporting dashboard

**Long-term (12+ months):**
- Machine learning recommendations
- Multi-tenant architecture
- International expansion features
- Advanced marketplace analytics

### 13.2 Technology Considerations

**Potential Technology Upgrades:**
- Migration to .NET 9 when stable
- Adoption of minimal APIs
- Integration with cloud-native services
- Container orchestration with Kubernetes
- Service mesh implementation (Istio)

**Performance Optimizations:**
- Database query optimization
- CDN implementation for static assets
- Edge computing for global users
- Advanced caching strategies

## 14. Conclusion

This microservices architecture provides a robust, scalable foundation for The Marketplace platform. The combination of .NET 8, clean architecture principles, event-driven communication via RabbitMQ and MassTransit, and comprehensive DevOps practices creates a system optimised for:

- **Developer Productivity** - Clear patterns and well-defined boundaries
- **System Reliability** - Fault tolerance and graceful degradation  
- **Business Agility** - Rapid feature development and deployment
- **Operational Excellence** - Comprehensive monitoring and automated operations

The architecture successfully demonstrates modern microservices best practices while providing a practical implementation that teams can understand, maintain, and evolve as business requirements change.

By implementing this design, The Marketplace platform is positioned to handle significant growth in users, transactions, and features while maintaining high performance, reliability, and developer satisfaction.

---

**Document Version:** 2.0  
**Last Updated:** [Current Date]  
**Review Cycle:** Quarterly  
**Next Review:** [Date + 3 months]