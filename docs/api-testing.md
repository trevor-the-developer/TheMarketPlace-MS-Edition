# API Testing Script Documentation

## Overview

The `api_tests.sh` script provides comprehensive automated testing for TheMarketPlace microservices architecture. It tests all major API endpoints through a complete end-to-end workflow including user registration, authentication, listing management, search functionality, and document processing.

## Features

- **Complete Authentication Flow**: User registration → Email confirmation → Login → JWT token management
- **Full CRUD Testing**: Create, read, update, and delete operations for all entities
- **Docker Management**: Automated container and database lifecycle management
- **Flexible Database Cleaning**: Multiple options for handling test data conflicts
- **Service Health Monitoring**: Automatic service availability checks with retries
- **Smart Error Handling**: Graceful handling of existing users and service failures
- **Modular Test Groups**: Run specific service tests or complete end-to-end testing
- **Flexible JSON Parsing**: Supports both jq and fallback grep/sed methods

## Prerequisites

- **Docker & Docker Compose v2**: Required for container management
- **curl**: For HTTP requests
- **jq**: Optional but recommended for better JSON parsing
- **bash**: Script requires bash shell

### Verify Prerequisites

```bash
# Check Docker Compose v2
docker compose version

# Check curl
curl --version

# Check jq (optional)
jq --version
```

## Quick Start

### 1. Most Common Usage (Recommended)

For daily testing with potential database conflicts:

```bash
# Clean databases and run all tests
./api_tests.sh --clean-databases all
```

This approach:
- ✅ Cleans only database volumes (faster than full cleanup)
- ✅ Handles "Email already exists" errors
- ✅ Keeps other container data intact
- ✅ Tests complete end-to-end workflow

### 2. Fresh Environment Setup

For complete clean environment:

```bash
# Complete fresh start
./api_tests.sh --setup-docker --clean-volumes all
```

### 3. Quick Test with Existing Containers

If containers are already running:

```bash
# Run tests with existing setup
./api_tests.sh all
```

## Command Reference

### Basic Syntax

```bash
./api_tests.sh [OPTIONS] [TEST_GROUP]
```

### Test Groups

| Group | Description |
|-------|-------------|
| `all` | Run all tests (default) |
| `auth` | Authentication service tests only |
| `listing` | Listing service tests only |
| `search` | Search service tests only |
| `document` | Document processor tests only |

### Options

| Option | Description |
|--------|-------------|
| `--setup-docker` | Start Docker containers before testing |
| `--teardown-docker` | Stop Docker containers after testing |
| `--clean-volumes` | Remove all Docker volumes (complete fresh start) |
| `--clean-databases` | Clean databases only (faster, recommended) |
| `--unique-email` | Generate unique email addresses for testing |
| `--skip-registration` | Skip user registration, attempt direct login |
| `--help` | Show help message |

## Configuration

### Service Endpoints

The script tests these default service endpoints:

```bash
AUTH_SERVICE_URL="http://localhost:5000"
LISTING_SERVICE_URL="http://localhost:5001"
SEARCH_SERVICE_URL="http://localhost:5002"
DOCUMENT_PROCESSOR_URL="http://localhost:5003"
```

### Test User Credentials

```bash
# Primary test user (Seller role)
TEST_USER_EMAIL="testuser@example.com"
TEST_USER_PASSWORD="Password123!"
TEST_USER_ROLE=1  # Seller role for listing operations

# Additional test users
ADMIN_EMAIL="admin@example.com"
ADMIN_PASSWORD="Admin123!"
SELLER_EMAIL="seller@example.com"
SELLER_PASSWORD="Seller123!"
CUSTOMER_EMAIL="customer@example.com"
CUSTOMER_PASSWORD="Customer123!"
```

## Usage Examples

### Database Management

```bash
# Clean databases only (fastest, handles user conflicts)
./api_tests.sh --clean-databases

# Complete fresh start with clean volumes
./api_tests.sh --setup-docker --clean-volumes all

# Clean databases and run specific test group
./api_tests.sh --clean-databases auth
```

### User Management Strategies

```bash
# Handle existing users with unique emails
./api_tests.sh --unique-email all

# Skip registration for existing users
./api_tests.sh --skip-registration all

# Combine approaches
./api_tests.sh --unique-email --setup-docker auth
```

### Service-Specific Testing

```bash
# Test only authentication service
./api_tests.sh auth

# Test only listing service with fresh database
./api_tests.sh --clean-databases listing

# Test search service with unique email
./api_tests.sh --unique-email search

# Test document processor with existing containers
./api_tests.sh document
```

### CI/CD Integration

```bash
# Complete automated testing pipeline
./api_tests.sh --setup-docker --clean-volumes --teardown-docker all

# Quick integration test
./api_tests.sh --clean-databases all
```

## Testing Workflow

The script follows this complete workflow:

### 1. Pre-Test Setup
- Configuration validation and display
- Docker environment setup (if requested)
- Database cleaning (if requested)
- Service health verification with retry logic

### 2. Authentication Phase
```
Register User → Confirm Email → Login → Get JWT Token (securityToken)
```

**Important**: The script expects the login response to contain a `securityToken` field, not `accessToken`.

### 3. Service Testing Phase

#### Complete Test Suite (`all`):
```
Service Availability Checks →
Authentication Flow →
List Categories → 
Create Listing → 
Get Listing → 
Update Listing → 
Basic Search → 
Advanced Search → 
Generate Document → 
Check Document Status → 
Delete Listing
```

#### Individual Test Groups:
- **Auth Tests**: Service availability + complete authentication flow
- **Listing Tests**: Auth + categories + CRUD operations
- **Search Tests**: Auth + listing setup + search operations + cleanup
- **Document Tests**: Auth + listing setup + document operations + cleanup

### 4. Post-Test Cleanup
- Resource cleanup
- Container management (if requested)
- Final status reporting

## Database Cleaning Details

The `--clean-databases` option provides intelligent database volume cleanup:

### Volume Patterns Cleaned
```bash
# Common volume naming patterns
themarketplace-ms-edition_postgres-data
themarketplace-ms-edition_mongodb-data
themarketplace-ms-edition_opensearch-data
marketplace_postgres-data
marketplace_mongodb-data
marketplace_opensearch-data

# Plus any volumes matching database keywords:
# postgres, mongo, mysql, redis, elastic, opensearch
```

### Process
1. Stop database-related services
2. Remove identified database volumes
3. Restart services with clean databases
4. Wait for service initialization (30 seconds + health checks)

## Token Management

The script handles JWT tokens correctly:

```bash
# Login response extracts:
ACCESS_TOKEN=$(extract_json_value "$response" "securityToken")  # Note: securityToken, not accessToken
REFRESH_TOKEN=$(extract_json_value "$response" "refreshToken")

# Used in subsequent requests:
-H "Authorization: Bearer $ACCESS_TOKEN"
```

## Error Handling

### Smart Registration Logic

The script includes intelligent handling for existing users:

```bash
# Registration outcomes:
# Return 0: New user registered successfully
# Return 2: User already exists (proceeds to login)
# Return 1: Registration failed
```

### Service Availability Checks

Multi-endpoint health checking:
```bash
# Tries multiple health check patterns:
/health
/api/health
Basic connectivity (HEAD request)
```

### Retry Logic

- **Service Availability**: 5 attempts with 2-second delays
- **Docker Startup**: 15-20 attempts with 5-second delays
- **Database Cleanup**: 15 attempts with 5-second delays

## Troubleshooting

### Common Issues and Solutions

#### ❌ "Email already exists" Error

**Problem**: User email conflicts from previous test runs

**Solutions** (in order of preference):
```bash
# Option 1: Clean databases (recommended)
./api_tests.sh --clean-databases all

# Option 2: Use unique emails
./api_tests.sh --unique-email all

# Option 3: Skip registration
./api_tests.sh --skip-registration all

# Option 4: Complete volume cleanup
./api_tests.sh --setup-docker --clean-volumes all
```

#### ❌ Services Not Available

**Problem**: Services not responding or starting

**Solutions**:
```bash
# Check container status
docker compose ps

# Restart with setup
./api_tests.sh --setup-docker all

# Check logs
docker compose logs

# Complete reset
./api_tests.sh --setup-docker --clean-volumes all
```

#### ❌ Authentication Failures

**Problem**: JWT token issues or login failures

**Common causes**:
- Service expects `securityToken` field in login response
- Email confirmation not completed
- Invalid credentials

**Solutions**:
```bash
# Use fresh database
./api_tests.sh --clean-databases auth

# Check service logs
docker compose logs auth-svc

# Test with unique email
./api_tests.sh --unique-email auth
```

#### ❌ Docker Compose Command Not Found

**Problem**: Using Docker Compose v1 format

**Solution**: Ensure you have Docker Compose v2 installed:
```bash
# Check version
docker compose version

# Update Docker Desktop or install Compose v2
```

### Test Output Interpretation

The script provides detailed output for each test phase:

#### Success Indicators
```bash
✓ PASS: Registration successful for Seller user
✓ PASS: Successfully retrieved categories
✓ PASS: Successfully created listing
✓ PASS: Successfully performed basic search
```

#### Failure Indicators  
```bash
✗ FAIL: Login failed for Seller user
✗ FAIL: Failed to create listing
✗ FAIL: Services failed to start within expected time
```

#### Information Messages
```bash
ℹ INFO: User already exists, skipping registration for Seller user
```

#### Status Tracking
```bash
# Dynamic variables populated during execution:
Access Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
First Category ID: 550e8400-e29b-41d4-a716-446655440000
Listing ID: 6ba7b810-9dad-11d1-80b4-00c04fd430c8
```

## Performance Considerations

### Speed Comparison

| Approach | Time | Use Case |
|----------|------|----------|
| `./api_tests.sh` | ~30s | Existing healthy containers |
| `--clean-databases` | ~60s | Handle user conflicts |
| `--setup-docker` | ~90s | Container restart needed |
| `--clean-volumes` | ~120s | Complete fresh start |

### Optimization Tips

1. **Use `--clean-databases`** for daily testing (fastest conflict resolution)
2. **Avoid `--clean-volumes`** unless necessary (slowest)
3. **Run specific test groups** during feature development
4. **Use `--unique-email`** for parallel testing scenarios
5. **Install jq** for faster JSON parsing

## Advanced Usage

### Standalone Database Cleaning

```bash
# Clean databases only (without running tests)
./api_tests.sh --clean-databases
# Will exit after cleaning if no other options specified
```

### Parallel Testing

For parallel test execution:

```bash
# Terminal 1: Auth tests
./api_tests.sh --unique-email auth

# Terminal 2: Search tests  
./api_tests.sh --unique-email search

# Terminal 3: Document tests
./api_tests.sh --unique-email document
```

### Custom Workflows

```bash
# Development workflow
./api_tests.sh --clean-databases listing  # Test specific service
./api_tests.sh --unique-email --skip-registration search  # Quick search test

# Integration testing
./api_tests.sh --setup-docker --clean-volumes --teardown-docker all
```

## Integration with Development Workflow

### Daily Development Testing

```bash
# Quick test during development
./api_tests.sh --clean-databases all
```

### Feature Development

```bash
# Test specific service while developing
./api_tests.sh --clean-databases listing

# Test with fresh data
./api_tests.sh --unique-email auth
```

### Code Review / PR Testing

```bash
# Complete validation
./api_tests.sh --setup-docker --clean-volumes all
```

### CI/CD Pipeline Integration

```bash
#!/bin/bash
# CI/CD testing script

# Start environment
./api_tests.sh --setup-docker --clean-volumes all

# Run tests and capture exit code
EXIT_CODE=$?

# Cleanup
./api_tests.sh --teardown-docker

exit $EXIT_CODE
```

## Script Architecture

### Key Functions

| Function | Purpose |
|----------|---------|
| `run_complete_auth_flow()` | Handles registration, confirmation, and login |
| `extract_json_value()` | JSON parsing with jq fallback |
| `test_service_availability()` | Multi-pattern health checking |
| `clean_databases()` | Intelligent database volume cleanup |
| `setup_docker_environment()` | Container lifecycle management |

### Configuration Display

The script shows current configuration before execution:
```bash
=========================================
CONFIGURATION
=========================================
Test Group: all
Setup Docker: false
Clean Volumes: false
Clean Databases Only: false
Unique Email: false
Skip Registration: false
Test User Email: testuser@example.com
=========================================
```

## Script Maintenance

### TODO Items

The script includes this TODO for future improvement:
```bash
# TODO: TK 26/05/2025 - move functions into a separate file for reusability and cleaner code
```

### Regular Updates

Keep the script aligned with:
- API endpoint changes (especially authentication token field names)
- New service additions
- Authentication flow modifications
- Docker container updates
- Database volume naming patterns

### Extending the Script

To add new test scenarios:

1. Add new test functions following existing patterns
2. Update the test group cases in the main switch statement
3. Add command-line options if needed
4. Update this documentation
5. Follow the existing color-coded output format

## Support

For issues with the testing script:

1. **Check Prerequisites**: Ensure Docker Compose v2 and curl are available
2. **Review Logs**: Use `docker compose logs` for service-specific issues
3. **Try Clean Start**: Use `--clean-volumes` for persistent issues
4. **Check Service Health**: Verify individual service endpoints manually
5. **Verify Token Format**: Ensure services return `securityToken` in login responses

## Related Documentation

- [Postman Testing Guide](postman/postman-testing.md)
- [API Quick Reference](postman/postman-testing-quick-ref.md)
- [Architecture Documentation](architecture/architecture_v2.md)
- [Technical Design](design/technical-design_v2.md)

---

**Happy Testing! 🚀**