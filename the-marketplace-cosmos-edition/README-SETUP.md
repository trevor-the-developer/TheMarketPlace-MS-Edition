# The Marketplace Microservices API Setup Guide

This guide will help you run the Marketplace microservices solution on your development machine.

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- Git

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd the-marketplace-msapi
```

### 2. Running the Solution

You have several options to run the solution:

#### Option 1: Using Docker Compose (Recommended)

This will start all services and their dependencies:

```bash
# Start all services with Docker Compose
./run-dev.sh all
```

#### Option 2: Start Infrastructure with Docker and Run Services Individually

```bash
# Start the infrastructure containers only
docker-compose up -d postgres opensearch opensearch-dashboards

# In separate terminals, run each service:
# Terminal 1: Authentication Service
./run-dev.sh auth

# Terminal 2: Listing Service
./run-dev.sh listing

# Terminal 3: Search Service
./run-dev.sh search
```

#### Option 3: Manual Setup

```bash
# Start infrastructure
docker-compose up -d postgres opensearch opensearch-dashboards

# In separate terminals:
# Terminal 1
cd src/Services/AuthenticationService/API/AuthenticationService.Api
dotnet run --urls=http://localhost:5000

# Terminal 2
cd src/Services/ListingService/API/ListingService.Api
dotnet run --urls=http://localhost:5001

# Terminal 3
cd src/Services/SearchService/API/SearchService.Api
dotnet run --urls=http://localhost:5002
```

### 3. Accessing the Services

- Authentication Service: http://localhost:5000/swagger
- Listing Service: http://localhost:5001/swagger  
- Search Service: http://localhost:5002/swagger
- OpenSearch Dashboard: http://localhost:5601

### 4. Authentication Flow

1. Register a user:
   ```
   POST to http://localhost:5000/api/register
   {
     "username": "testuser",
     "email": "test@example.com",
     "password": "Password123!"
   }
   ```

2. Login to get a JWT token:
   ```
   POST to http://localhost:5000/api/login
   {
     "email": "test@example.com",
     "password": "Password123!"
   }
   ```

3. Use the token for authenticated endpoints:
   - In Swagger: Click "Authorize" and enter: `Bearer {your-token}`
   - In Postman/cURL: Add header `Authorization: Bearer {your-token}`

### 5. Troubleshooting

If you encounter issues:

- **OpenSearch errors**: Try removing the volume and starting again:
  ```bash
  docker-compose down
  docker volume rm the-marketplace-msapi_opensearch-data
  docker-compose up
  ```

- **Database errors**: Ensure PostgreSQL is running and accessible:
  ```bash
  docker ps | grep postgres
  ```

- **Build errors**: Make sure you have the correct .NET SDK version:
  ```bash
  dotnet --version  # Should show 8.0.x
  ```

## Architecture Overview

The solution follows a microservices architecture with:

- **AuthenticationService**: Handles user authentication and authorization
- **ListingService**: Manages marketplace listings and categories
- **SearchService**: Provides search functionality using OpenSearch

Each service follows Clean Architecture principles with:
- Domain layer (entities)
- Application layer (commands/queries)
- Infrastructure layer (persistence)
- API layer (controllers)