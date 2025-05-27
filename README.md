# TheMarketPlace Microservices API

A modern microservices-based marketplace platform built with .NET 8, featuring user authentication, listing management, search capabilities, and document processing.

## Quick Start

### Prerequisites

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
- **Git** - [Download here](https://git-scm.com/downloads)

### Get Started in 30 Seconds

```bash
# Clone the repository
git clone <repository-url>
cd the-marketplace-msapi

# Start everything with Docker
./run-dev.sh --setup-docker
```

**That's it!** All services are now running and ready to use.

## Service URLs

Once started, access your services at:

| Service | URL | Purpose |
|---------|-----|---------|
| **Authentication** | http://localhost:5000/swagger | User registration & login |
| **Listing** | http://localhost:5001/swagger | Marketplace listings |
| **Search** | http://localhost:5002/swagger | Search functionality |
| **Document Processor** | http://localhost:5003/swagger | PDF generation |

### Management Dashboards

| Dashboard | URL | Credentials |
|-----------|-----|-------------|
| **Hangfire** | http://localhost:5003/hangfire | Background jobs |
| **RabbitMQ** | http://localhost:15672 | guest/guest |
| **OpenSearch** | http://localhost:5601 | Search analytics |
| **MinIO Console** | http://localhost:9001 | minioadmin/minioadmin |

## Development Modes

### 1. Full Docker Environment (Recommended for Testing)

```bash
# Start all services with Docker
./run-dev.sh --setup-docker

# Clean start with fresh data
./run-dev.sh --setup-docker --clean-volumes

# Stop everything
./run-dev.sh --teardown-docker
```

### 2. Individual Service Development (Recommended for Coding)

```bash
# Run specific services for active development
./run-dev.sh auth      # Authentication service only
./run-dev.sh listing   # Listing service only
./run-dev.sh search    # Search service only
./run-dev.sh docs      # Document processor only
```

*Note: Infrastructure (databases, message queues) starts automatically*

### 3. Service Health Monitoring

```bash
# Check if all services are healthy
./run-dev.sh --health-check

# Check service status
./run-dev.sh status
```

## API Testing

### Option 1: Automated Testing Script

```bash
# Run complete test suite
./api_tests.sh all

# Test specific services
./api_tests.sh auth
./api_tests.sh listing
./api_tests.sh search
```

### Option 2: Manual API Testing

#### 1. Register a User
```bash
curl -X POST http://localhost:5000/api/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com",
    "password": "Test@123",
    "dateOfBirth": "2000-01-01T00:00:00Z",
    "role": 1
  }'
```

#### 2. Confirm Email (Required)
```bash
curl -G "http://localhost:5000/api/confirm-email" \
  --data-urlencode "email=test@example.com" \
  --data-urlencode "token=YOUR_CONFIRMATION_TOKEN"
```

#### 3. Login and Get Token
```bash
curl -X POST http://localhost:5000/api/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123"
  }'
```

#### 4. Create a Listing
```bash
curl -X POST http://localhost:5001/api/listings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "command": {
      "title": "My First Listing",
      "description": "A test listing",
      "price": 99.99,
      "location": "Test City",
      "categoryId": "CATEGORY_ID_FROM_/api/categories",
      "tags": ["test"]
    }
  }'
```

### Option 3: Swagger UI

Visit any service's Swagger UI (e.g., http://localhost:5001/swagger):
1. Click **"Authorize"**
2. Enter: `Bearer YOUR_JWT_TOKEN`
3. Test endpoints interactively

## Project Structure

### Services
- **AuthenticationService** (`src/Services/AuthenticationService/`) - User management & JWT tokens
- **ListingService** (`src/Services/ListingService/`) - Marketplace listings & categories  
- **SearchService** (`src/Services/SearchService/`) - Full-text search with OpenSearch
- **DocumentProcessor** (`src/Functions/DocumentProcessor/`) - Background job processing

### Infrastructure
- **PostgreSQL** - Primary database
- **OpenSearch** - Search engine
- **RabbitMQ** - Message broker
- **MongoDB** - Document storage
- **MinIO** - File storage

## Development Workflow

### Day-to-Day Development

```bash
# Start infrastructure + your service
./run-dev.sh auth  # Develops auth service with hot reload

# In another terminal, test your changes
./api_tests.sh auth
```

### Integration Testing

```bash
# Full environment testing
./run-dev.sh --setup-docker
./api_tests.sh all
```

### Debugging

```bash
# Start only infrastructure
./run-dev.sh --setup-docker
docker compose stop auth-svc  # Stop the service you want to debug

# Run your service with debugging support
cd src/Services/AuthenticationService/API/AuthenticationService.Api
dotnet run --configuration Debug --urls=http://localhost:5000
```

## Architecture Highlights

- **Clean Architecture** - Domain → Application → Infrastructure → API layers
- **CQRS with MediatR** - Command Query Responsibility Segregation
- **Event-Driven** - Services communicate via RabbitMQ events
- **JWT Authentication** - Secure token-based authentication
- **Background Jobs** - Hangfire for document processing
- **Container Ready** - Docker Compose for easy deployment

## Troubleshooting

### Common Issues

**Services won't start:**
```bash
# Check Docker is running
docker --version

# Fresh restart
./run-dev.sh --setup-docker --clean-volumes
```

**Port conflicts:**
```bash
# Check what's using the port
lsof -i :5000  # macOS/Linux
netstat -ano | findstr :5000  # Windows
```

**Database issues:**
```bash
# Reset databases
./api_tests.sh --clean-databases
```

**Token expired:**
- Tokens expire after 60 minutes
- Simply login again for a fresh token

### Get Help

1. **Check service health**: `./run-dev.sh --health-check`
2. **View logs**: `docker compose logs [service-name]`
3. **Fresh start**: `./run-dev.sh --setup-docker --clean-volumes`

## Additional Documentation

- **[API Testing Guide](docs/api-testing-script.md)** - Comprehensive testing documentation
- **[Services Runner Guide](docs/services-runner.md)** - Detailed development setup options
- **[Postman Collection](postman/)** - Ready-to-use API testing collection

## Contributing

1. Fork the repository
2. Create a feature branch
3. Run tests: `./api_tests.sh all`
4. Submit a pull request

---

**Ready to build the next great marketplace? Let's go! 🚀**