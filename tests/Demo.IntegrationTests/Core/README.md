# Integration Tests

This project contains comprehensive integration tests for the marketplace microservices platform.

## Test Categories

### 1. Database Integration Tests (`DatabaseIntegrationTests.cs`)
- **PostgreSQL**: Connection, CRUD operations, concurrent access
- **MongoDB**: Document operations, collections management
- **MinIO**: File upload/download, bucket operations
- **Multi-database**: Concurrent operations across all databases

### 2. Message Flow Tests (`EndToEndMessageFlowTests.cs`)
- **RabbitMQ**: Connection establishment, message publishing/consuming
- **Event Flow**: Complete pipeline from publisher to consumer
- **MassTransit**: Framework integration with RabbitMQ

### 3. Service Integration Tests (`ServiceIntegrationTests.cs`)
- **Service Startup**: All services can start with test containers
- **API Endpoints**: Health checks, basic operations
- **Concurrent Requests**: Load testing scenarios
- **Error Handling**: Graceful degradation

### 4. Hangfire Integration Tests (`HangfireIntegrationTests.cs`)
- **PostgreSQL Storage**: Schema creation, job persistence
- **Job Processing**: Enqueue, execute, retry logic
- **DocumentProcessor**: PDF generation pipeline testing

## Prerequisites

- .NET 8.0 SDK
- Docker (for Testcontainers)
- Sufficient system resources for running multiple containers

## Running Tests

### Individual Test Categories
```bash
# Database tests
dotnet test --filter "DatabaseIntegrationTests"

# Message flow tests  
dotnet test --filter "EndToEndMessageFlowTests"

# Service tests
dotnet test --filter "ServiceIntegrationTests"

# Hangfire tests
dotnet test --filter "HangfireIntegrationTests"

# Quick verification tests
dotnet test --filter "QuickIntegrationTests"
```

### All Integration Tests
```bash
dotnet test tests/Demo.IntegrationTests/Core/
```

## Test Infrastructure

### Testcontainers
The tests use Testcontainers to spin up real instances of:
- PostgreSQL 15
- RabbitMQ 3 (with management)
- MongoDB (latest)
- MinIO (latest)

### Base Infrastructure
- `IntegrationTestBase`: Common container setup and lifecycle management
- Automatic cleanup after test completion
- Container health checking and readiness waiting

## Test Scenarios Covered

### ✅ Infrastructure Verification
- All databases can be connected to and operated on
- Message bus accepts and routes messages correctly
- File storage supports upload/download operations

### ✅ End-to-End Workflows
- Message publishing flows through to consumers
- Services can start and handle requests
- Hangfire jobs are persisted and processable

### ✅ Error Scenarios
- Invalid configurations are handled gracefully
- Services degrade appropriately when dependencies are unavailable
- Retry logic works as expected

### ✅ Performance & Concurrency
- Multiple services can run simultaneously
- Concurrent database operations work correctly
- Message throughput is acceptable

## Expected Test Duration

- **Quick Tests**: ~1 second
- **Database Tests**: ~10-30 seconds (container startup)
- **Service Tests**: ~30-60 seconds (full service startup)
- **Complete Suite**: ~2-5 minutes

## Troubleshooting

### Container Issues
- Ensure Docker is running and accessible
- Check available system resources (RAM, disk space)
- Verify no port conflicts with existing services

### Test Failures
- Check container logs for startup issues
- Verify network connectivity between containers
- Ensure sufficient time for container readiness

### Performance Issues
- Increase container startup timeouts
- Use fewer concurrent tests
- Consider running tests in sequence rather than parallel

## CI/CD Integration

These tests are designed to run in CI/CD environments with Docker support:
- GitHub Actions
- Azure DevOps
- Jenkins with Docker
- GitLab CI

### Example CI Configuration
```yaml
- name: Run Integration Tests
  run: |
    docker --version
    dotnet test tests/Demo.IntegrationTests/Core/ --logger trx --results-directory TestResults
```