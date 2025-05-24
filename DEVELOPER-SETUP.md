# The Marketplace Microservices API Development Setup Guide

This comprehensive guide will help you get the Marketplace microservices solution up and running on your development machine.

## Prerequisites

Before starting, ensure you have the following installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
- **Git** - [Download here](https://git-scm.com/downloads)
- **Your preferred IDE** (Visual Studio, VS Code, or JetBrains Rider)

### Verify Prerequisites

Run these commands to verify your setup:

```bash
# Check .NET version (should show 8.0.x)
dotnet --version

# Check Docker
docker --version
docker-compose --version

# Check Git
git --version
```

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd the-marketplace-msapi
```

### 2. Start the Complete Solution

The fastest way to get everything running:

```bash
# Start all services and infrastructure
./run-dev.sh all
```

**That's it!** All services will be available at:
- Authentication Service: http://localhost:5000/swagger
- Listing Service: http://localhost:5001/swagger  
- Search Service: http://localhost:5002/swagger
- Document Processor: http://localhost:5003/swagger
- Hangfire Dashboard: http://localhost:5003/hangfire
- OpenSearch Dashboard: http://localhost:5601
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- MinIO Console: http://localhost:9001 (minioadmin/minioadmin)

## Detailed Setup Options

### Option 1: Docker Compose (Recommended for Full Testing)

Start everything with a single command:

```bash
# Start all services with their dependencies
docker-compose up
```

This approach:
- ✅ Closest to production environment
- ✅ No local .NET runtime issues
- ✅ Easy cleanup with `docker-compose down`
- ❌ Slower for active development (requires rebuilding for code changes)

### Option 2: Hybrid Development Setup (Recommended for Active Development)

Start infrastructure with Docker, run services locally for faster development:

```bash
# Start only the infrastructure services
docker-compose up -d postgres opensearch opensearch-dashboards rabbitmq mongodb minio

# Run services individually (in separate terminals)
./run-dev.sh auth     # Terminal 1: Authentication Service
./run-dev.sh listing  # Terminal 2: Listing Service  
./run-dev.sh search   # Terminal 3: Search Service
./run-dev.sh docs     # Terminal 4: Document Processor Service
```

This approach:
- ✅ Fast development cycle (hot reload, debugging)
- ✅ Real database and search engine
- ✅ Easy to restart individual services
- ❌ Requires multiple terminal windows

### Option 3: Manual Setup (Full Control)

For complete control over each service:

```bash
# 1. Start infrastructure
docker-compose up -d postgres opensearch opensearch-dashboards rabbitmq mongodb minio

# 2. Run each service manually (separate terminals)

# Terminal 1: Authentication Service
cd src/Services/AuthenticationService/API/AuthenticationService.Api
dotnet run --urls=http://localhost:5000

# Terminal 2: Listing Service
cd src/Services/ListingService/API/ListingService.Api
dotnet run --urls=http://localhost:5001

# Terminal 3: Search Service
cd src/Services/SearchService/API/SearchService.Api
dotnet run --urls=http://localhost:5002

# Terminal 4: Document Processor Service
cd src/Functions/DocumentProcessor
dotnet run --urls=http://localhost:5003
```

## Service Endpoints and Documentation

| Service | URL | Swagger UI | Purpose |
|---------|-----|------------|---------|
| Authentication | http://localhost:5000 | http://localhost:5000/swagger | User registration, login, JWT tokens |
| Listing | http://localhost:5001 | http://localhost:5001/swagger | Marketplace listings and categories |
| Search | http://localhost:5002 | http://localhost:5002/swagger | Search across all marketplace content |
| Document Processor | http://localhost:5003 | http://localhost:5003/swagger | Background job processing |
| Hangfire Dashboard | http://localhost:5003/hangfire | N/A | Job monitoring and management |
| OpenSearch Dashboard | http://localhost:5601 | N/A | Search engine management and monitoring |
| RabbitMQ Management | http://localhost:15672 | N/A | Message broker management (guest/guest) |
| MinIO Console | http://localhost:9001 | N/A | Object storage management (minioadmin/minioadmin) |

## Getting Started with the API

### Step 1: Register a User

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com", 
    "password": "Password123!"
  }'
```

### Step 2: Login and Get JWT Token

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!"
  }'
```

Save the returned JWT token - you'll need it for authenticated requests.

### Step 3: Create Your First Listing

```bash
curl -X POST http://localhost:5001/api/listings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "title": "My First Listing",
    "description": "A test listing to verify the setup",
    "price": 99.99,
    "location": "Test City"
  }'
```

### Step 4: Search for Your Listing

```bash
curl "http://localhost:5002/api/search?query=first+listing"
```

## Using Swagger UI

For easier API testing, use the Swagger interfaces:

1. **Open Swagger UI** for any service (e.g., http://localhost:5001/swagger)
2. **Authenticate**: Click the "Authorize" button
3. **Enter your token**: Format as `Bearer YOUR_JWT_TOKEN_HERE`
4. **Test endpoints**: Expand any endpoint and click "Try it out"

## Development Workflow

### Making Code Changes

**For Docker Compose setup:**
```bash
# After making changes, rebuild and restart
docker-compose down
docker-compose up --build
```

**For Hybrid/Manual setup:**
- Services running with `dotnet run` will automatically reload on code changes
- Only restart if you change configuration or add new dependencies

### Database Changes

If you modify Entity Framework models:

```bash
# Navigate to the service with changes (e.g., ListingService)
cd src/Services/ListingService/Infrastructure

# Add a new migration
dotnet ef migrations add YourMigrationName --startup-project ../API/ListingService.Api

# Apply migrations (done automatically on startup, but you can run manually)
dotnet ef database update --startup-project ../API/ListingService.Api
```

### Viewing Logs

**Docker Compose:**
```bash
# View logs for all services
docker-compose logs -f

# View logs for specific service
docker-compose logs -f listingservice
```

**Individual Services:**
Logs appear directly in the terminal where you ran `dotnet run`

## Troubleshooting

### Common Issues and Solutions

**🔥 OpenSearch fails to start**
```bash
# Remove the data volume and restart
docker-compose down
docker volume rm the-marketplace-msapi_opensearch-data
docker-compose up -d opensearch
```

**🔥 Port already in use**
```bash
# Find what's using the port (replace 5000 with your port)
lsof -i :5000  # On macOS/Linux
netstat -ano | findstr :5000  # On Windows

# Kill the process or change the port in your configuration
```

**🔥 Database connection errors**
```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# View PostgreSQL logs
docker-compose logs postgres

# Reset the database
docker-compose down
docker volume rm the-marketplace-msapi_postgres-data
docker-compose up -d postgres
```

**🔥 JWT token expired**
- Tokens expire after a set time (check AuthenticationService configuration)
- Simply login again to get a fresh token

**🔥 Build errors**
```bash
# Clean and restore packages
dotnet clean
dotnet restore

# Check .NET version
dotnet --version  # Should show 8.0.x

# Update packages if needed
dotnet list package --outdated
```

**🔥 Services can't communicate**
- Ensure all services are running on correct ports
- Check firewall settings
- Verify Docker network configuration with `docker network ls`

### Health Checks

Check if services are healthy:

```bash
# Quick health check for all services
curl http://localhost:5000/health  # Authentication Service
curl http://localhost:5001/health  # Listing Service
curl http://localhost:5002/health  # Search Service
```

### Reset Everything

If you need a completely fresh start:

```bash
# Stop all containers and remove volumes
docker-compose down -v

# Remove any orphaned containers
docker container prune

# Remove unused images (optional)
docker image prune

# Restart everything
docker-compose up
```

## Architecture Overview

The solution follows a microservices architecture with clean separation of concerns:

### Services

- **AuthenticationService**: User registration, login, JWT token management
- **ListingService**: Marketplace listings, categories, and business rules
- **SearchService**: Full-text search capabilities using OpenSearch
- **DocumentProcessor**: Background job processing for PDF generation and document workflows

### Infrastructure

- **PostgreSQL**: Primary database for AuthenticationService, ListingService, and SearchService
- **MongoDB**: Document database for DocumentProcessor
- **OpenSearch**: Search engine and analytics
- **RabbitMQ**: Message broker for event-driven communication
- **MinIO**: Object storage for file management
- **Hangfire**: Background job processing system

### Architecture Patterns

Each service implements:
- **Clean Architecture**: Domain → Application → Infrastructure → API layers
- **CQRS**: Command Query Responsibility Segregation with MediatR
- **Event-Driven Design**: Services publish/consume events for loose coupling
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling and testability

## Next Steps

Once you have the solution running:

1. **Explore the API**: Use Swagger UI to test different endpoints
2. **Check the code**: Review the clean architecture implementation
3. **Add features**: Create new endpoints or modify existing ones
4. **Run tests**: Execute `dotnet test` to run the test suite
5. **Monitor with OpenSearch**: Use the dashboard to see indexed data

## Getting Help

If you encounter issues not covered in this guide:

1. Check the service logs for detailed error messages
2. Review the project documentation in `/docs`
3. Ensure all prerequisites are correctly installed
4. Try the "Reset Everything" steps for a clean slate

Happy coding! 🚀
