# Services Runner Script Documentation

## Overview

The `run-dev.sh` script provides a comprehensive development and deployment tool for TheMarketPlace microservices architecture. It offers flexible service management, Docker orchestration, and development workflow support with intelligent infrastructure handling and service health monitoring.

## Features

- **Individual Service Development**: Run single services in development mode with .NET CLI
- **Docker Orchestration**: Complete container lifecycle management with Docker Compose
- **Infrastructure Management**: Automated setup of PostgreSQL, OpenSearch, RabbitMQ, MongoDB, and MinIO
- **Health Monitoring**: Comprehensive service and infrastructure health checks
- **Development Workflow**: Streamlined developer experience with automatic dependency management
- **Service Discovery**: Clear service URLs and management dashboard access
- **Smart Infrastructure**: Optional infrastructure startup with dependency awareness

## Prerequisites

- **Docker & Docker Compose v2**: Required for container management
- **.NET SDK**: Required for individual service development
- **curl**: For health checks
- **bash**: Script requires bash shell

### Verify Prerequisites

```bash
# Check Docker Compose v2
docker compose version

# Check .NET SDK
dotnet --version

# Check curl
curl --version
```

## Quick Start

### 1. Complete Environment Setup (Recommended)

For full development environment:

```bash
# Start all services with Docker
./run-dev.sh --setup-docker
```

### 2. Individual Service Development

For developing a specific service:

```bash
# Run auth service in development mode
./run-dev.sh auth

# Run listing service in development mode
./run-dev.sh listing
```

### 3. Health Monitoring

Check system status:

```bash
# Check all service health
./run-dev.sh --health-check

# Check service status
./run-dev.sh status
```

## Command Reference

### Basic Syntax

```bash
./run-dev.sh [OPTIONS] [SERVICE]
```

### Services

| Service | Description | Port | Path |
|---------|-------------|------|------|
| `auth` | Authentication Service | 5000 | `src/Services/AuthenticationService/API/AuthenticationService.Api` |
| `listing` | Listing Service | 5001 | `src/Services/ListingService/API/ListingService.Api` |
| `search` | Search Service | 5002 | `src/Services/SearchService/API/SearchService.Api` |
| `docs` | Document Processor | 5003 | `src/Functions/DocumentProcessor` |
| `all` | All services via Docker | - | Docker Compose |
| `status` | Service status check | - | Health monitoring |

### Docker Management Options

| Option | Description |
|--------|-------------|
| `--setup-docker` | Start all services with Docker Compose |
| `--teardown-docker` | Stop all Docker containers |
| `--clean-volumes` | Remove Docker volumes (use with setup/teardown) |
| `--skip-infrastructure` | Skip starting infrastructure containers |
| `--health-check` | Check health of all running services |
| `--help` | Show help message |

## Service Information

### Service Endpoints

When services are running, you can access:

```bash
# API Documentation (Swagger)
Authentication:  http://localhost:5000/swagger
Listing:         http://localhost:5001/swagger
Search:          http://localhost:5002/swagger
Document Proc:   http://localhost:5003/swagger

# Health Checks
Authentication:  http://localhost:5000/health
Listing:         http://localhost:5001/health
Search:          http://localhost:5002/health
Document Proc:   http://localhost:5003/health
```

### Management Dashboards

```bash
# Service Management
Hangfire:        http://localhost:5003/hangfire

# Infrastructure Dashboards
RabbitMQ:        http://localhost:15672 (guest/guest)
OpenSearch:      http://localhost:5601
MinIO Console:   http://localhost:9001 (minioadmin/minioadmin)
```

### Authentication Flow

The script displays the complete authentication flow:

```bash
1. POST /api/register (with role: 0=Customer, 1=Seller, 2=Admin)
2. GET /api/confirm-email?email={email}&token={token} (REQUIRED)
3. POST /api/login
4. Use Bearer token for authenticated requests
```

## Usage Examples

### Development Workflows

```bash
# Full environment setup
./run-dev.sh --setup-docker

# Clean environment setup
./run-dev.sh --setup-docker --clean-volumes

# Individual service development
./run-dev.sh auth
./run-dev.sh listing
./run-dev.sh search
./run-dev.sh docs

# Environment cleanup
./run-dev.sh --teardown-docker
./run-dev.sh --teardown-docker --clean-volumes
```

### Monitoring and Health Checks

```bash
# Check all service health
./run-dev.sh --health-check

# Check service status and Docker containers
./run-dev.sh status

# Quick health check (manual)
curl http://localhost:5000/health  # Auth service
curl http://localhost:5001/health  # Listing service
```

### Infrastructure Management

```bash
# Start with infrastructure only
./run-dev.sh auth  # Auto-starts infrastructure + auth service

# Skip infrastructure (if already running)
./run-dev.sh --skip-infrastructure auth

# Full Docker orchestration
./run-dev.sh all  # Uses docker compose up --build
```

## Infrastructure Components

The script automatically manages these infrastructure services:

### Database Services
- **PostgreSQL**: Primary database for user data and listings
- **MongoDB**: Document storage for complex data structures
- **OpenSearch**: Search and analytics engine

### Message and Storage
- **RabbitMQ**: Message broker for service communication
- **MinIO**: S3-compatible object storage for files

### Monitoring
- **OpenSearch Dashboards**: Search analytics and monitoring
- **Hangfire**: Background job processing dashboard

## Service Management

### Individual Service Development

When running individual services, the script:

1. **Checks Dependencies**: Verifies .NET SDK availability
2. **Starts Infrastructure**: Automatically starts required infrastructure containers
3. **Validates Paths**: Ensures service directories exist
4. **Runs Service**: Uses `dotnet run --urls=http://localhost:{port}`
5. **Displays Information**: Shows health check and Swagger URLs

### Docker Orchestration

When using Docker mode, the script:

1. **Validates Docker**: Checks Docker and Compose v2 availability
2. **Manages Containers**: Handles existing container cleanup
3. **Volume Management**: Optional volume cleanup for fresh starts
4. **Service Coordination**: Starts all services with proper dependencies
5. **Status Reporting**: Displays service URLs and management dashboards

## Health Monitoring

### Service Health Checks

The script provides comprehensive health monitoring:

```bash
# Individual service health
check_service_health "Authentication Service" 5000
check_service_health "Listing Service" 5001
check_service_health "Search Service" 5002
check_service_health "Document Processor" 5003
```

### Infrastructure Health Checks

```bash
# Infrastructure component checks
PostgreSQL:  docker compose ps postgres
RabbitMQ:    docker compose ps rabbitmq  
OpenSearch:  docker compose ps opensearch
```

### Health Check Process

- **Retry Logic**: 10 attempts with 2-second intervals
- **Endpoint Testing**: Tests `/health` endpoints
- **Status Reporting**: Color-coded success/failure indicators
- **Container Validation**: Checks Docker container status

## Development Workflow Integration

### Recommended Development Process

```bash
# 1. Initial Setup
./run-dev.sh --setup-docker --clean-volumes

# 2. Development Phase
# Stop Docker services for the service you're developing
docker compose stop auth-svc  # If developing auth service

# 3. Run Individual Service
./run-dev.sh --skip-infrastructure auth

# 4. Test and Iterate
# Service runs with hot reload via dotnet run

# 5. Integration Testing
./run-dev.sh --health-check

# 6. Cleanup
./run-dev.sh --teardown-docker
```

### Service Dependencies

Each service automatically gets:
- **Infrastructure**: PostgreSQL, RabbitMQ, OpenSearch, MongoDB, MinIO
- **Service Discovery**: Proper localhost URLs for inter-service communication
- **Health Monitoring**: Automatic health check endpoints
- **Documentation**: Swagger UI for API exploration

## Error Handling and Troubleshooting

### Common Issues

#### ❌ Docker Not Available
```bash
✗ Docker is not running. Please start Docker and try again.
✗ Docker Compose v2 is not available. Please update Docker Desktop.
```

**Solution**: Ensure Docker Desktop is running and updated to support Compose v2.

#### ❌ .NET SDK Not Found
```bash
✗ .NET CLI is not available. Please install .NET SDK
```

**Solution**: Install .NET SDK appropriate for your services.

#### ❌ Service Path Not Found
```bash
✗ Service path 'src/Services/AuthenticationService/API/AuthenticationService.Api' does not exist
```

**Solution**: Ensure you're running the script from the project root directory.

#### ❌ Port Already in Use
**Solution**: Check for existing services and stop them, or use Docker mode instead.

### Health Check Failures

```bash
⚠ Authentication Service may not be ready yet
```

**Common causes**:
- Service still starting up
- Database connectivity issues
- Port conflicts
- Missing environment variables

**Solutions**:
```bash
# Check container logs
docker compose logs auth-svc

# Restart infrastructure
./run_services.sh --setup-docker

# Check port availability
netstat -ln | grep :5000
```

## Performance Considerations

### Resource Usage

| Mode | Memory | Startup Time | Use Case |
|------|--------|--------------|----------|
| Individual Service | ~200MB | ~10s | Active development |
| Docker All Services | ~2GB | ~60s | Integration testing |
| Infrastructure Only | ~1GB | ~30s | Hybrid development |

### Optimization Tips

1. **Use `--skip-infrastructure`** when infrastructure is already running
2. **Individual services** for faster development cycles
3. **Docker mode** for integration testing and demos
4. **Volume cleanup** only when necessary (slower startup)

## Integration with Other Tools

### Works Well With

- **API Testing Script**: Use `./api_tests.sh` after `./run_services.sh --setup-docker`
- **Postman Collections**: All services provide Swagger endpoints for import
- **IDE Integration**: Individual services support debugging and hot reload
- **CI/CD Pipelines**: Docker mode suitable for automated testing

### Example CI/CD Integration

```bash
#!/bin/bash
# CI/CD pipeline integration

# Start environment
./run-dev.sh --setup-docker --clean-volumes

# Wait for services to be ready
./run-dev.sh --health-check

# Run tests
./api_tests.sh --skip-registration all

# Cleanup
./run-dev.sh --teardown-docker --clean-volumes
```

## Advanced Usage

### Custom Development Workflow

```bash
# Start only infrastructure
./run-dev.sh --setup-docker
docker compose stop auth-svc listing-svc search-svc document-processor

# Run services individually as needed
./run-dev.sh --skip-infrastructure auth    # Terminal 1
./run-dev.sh --skip-infrastructure listing # Terminal 2
```

### Service Debugging

```bash
# Start infrastructure
./run-dev.sh --skip-infrastructure

# Run service with debugger support
cd src/Services/AuthenticationService/API/AuthenticationService.Api
dotnet run --configuration Debug --urls=http://localhost:5000
```

### Production-Like Testing

```bash
# Full Docker environment
./run-dev.sh --setup-docker

# Comprehensive health check
./run-dev.sh --health-check

# API testing suite
./api_tests.sh all
```

## Script Maintenance

### Key Components

| Function | Purpose |
|----------|---------|
| `setup_infrastructure()` | Starts core infrastructure containers |
| `setup_docker_environment()` | Complete Docker orchestration |
| `run_service()` | Individual service development mode |
| `check_service_health()` | Health monitoring with retries |
| `display_service_urls()` | Service discovery information |

### Extension Points

To add new services:

1. Add service case to the main switch statement
2. Define service path and port
3. Update service arrays in health check functions
4. Add service to Docker Compose file
5. Update documentation

## Support

For issues with the services runner:

1. **Check Prerequisites**: Verify Docker, .NET SDK, and curl availability
2. **Review Container Logs**: Use `docker compose logs [service-name]`
3. **Validate Paths**: Ensure script runs from project root
4. **Check Ports**: Verify no port conflicts with existing services
5. **Infrastructure Health**: Use `./run-dev.sh --health-check`

## Related Documentation

- [API Testing Script](api-testing-script.md)
- [Docker Compose Configuration](docker-compose.yml)
- [Service Architecture Documentation](architecture/architecture_v2.md)
- [Development Setup Guide](setup/development-setup.md)

---

**Happy Developing! 🚀**