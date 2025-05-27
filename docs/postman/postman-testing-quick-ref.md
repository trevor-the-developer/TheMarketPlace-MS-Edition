# TheMarketPlace API Quick Reference

## Auth Service (localhost:5000)
```
POST   /api/register      - Register new user
POST   /api/login         - Login and get tokens
GET    /api/profile       - Get current user profile
POST   /api/refresh       - Refresh access token
```

## Listing Service (localhost:5001)
```
GET    /api/categories                - List all categories
POST   /api/listings                  - Create new listing
GET    /api/listings/{id}             - Get listing by ID
PUT    /api/listings/{id}             - Update listing
DELETE /api/listings/{id}             - Delete listing
```

## Search Service (localhost:5002)
```
GET    /api/search?query={term}       - Basic search
POST   /api/search/advanced           - Advanced search with filters
```

## Document Processor (localhost:5003)
```
GET    /health                       - Health check endpoint
GET    /hangfire                     - Background jobs dashboard
```
> **NOTE**: Document generation is message-driven (no REST API). Use Hangfire dashboard to monitor jobs.

## Supporting Services
```
RabbitMQ:    http://localhost:15672   - Message broker dashboard (guest/guest)
OpenSearch:  http://localhost:5601    - Search engine dashboard
MinIO:       http://localhost:9001    - Object storage (minioadmin/minioadmin)
Hangfire:    http://localhost:5003/hangfire - Background jobs dashboard
```

## Authentication Flow
1. Register (with appropriate role value) → 2. Confirm Email (required) → 3. Login → 4. Use Access Token in other requests → 5. Refresh token when needed → 6. Logout

## Database Initialization

The system uses Entity Framework Core migrations for database schema management and data seeding:

```
- Authentication Service: Migrations create identity tables and SeedRoleData seeds predefined roles
- Listing Service: Migrations create listing tables and SeedCategoryData seeds categories
- Search Service: Uses OpenSearch for indexing listing data
```

## User Roles

```
Admin (Role Value: 2):    admin@example.com / Admin123!     - Full system access
Seller (Role Value: 1):   seller@example.com / Seller123!   - Manage own listings
Customer (Role Value: 0): customer@example.com / Customer123! - Browse and search
```

> **IMPORTANT**: When registering users, you must specify the appropriate role value (0, 1, or 2) and complete email confirmation before login.

## Common Headers
```
Authorization: Bearer {{accessToken}}  - Required for authenticated endpoints
Content-Type: application/json         - Required for POST/PUT requests
```
## Key Request Bodies

### Register User
```json
{
  "firstName": "Test",
  "lastName": "User",
  "email": "testuser@example.com",
  "password": "Password123!",
  "role": 1,
  "dateOfBirth": "1990-01-01T00:00:00Z"
}
```
> Note: Use role value 1 for Seller permissions (required for listing operations)

### Create Listing
```json
{
  "title": "Test Listing for Postman",
  "description": "This is a test listing created through Postman",
  "price": 99.99,
  "location": "Test City",
  "categoryId": "{{categoryId}}",
  "tagNames": ["test", "postman", "api"]
}
```

### Advanced Search
```json
{
  "query": "test",
  "filters": {
    "minPrice": 50, "maxPrice": 150,
    "categoryIds": ["{{categoryId}}"],
    "location": "Test City", "tagNames": ["postman", "api"]
  },
  "sort": { "field": "price", "direction": "asc" },
  "pagination": { "pageNumber": 1, "pageSize": 10 }
}
```

### Generate Document
```json
{
  "type": "listing_pdf",
  "parameters": {
    "listingId": "{{listingId}}",
    "includeImages": true,
    "includeContactInfo": true
  }
}
```

## Automated Scripts

### Pre-request Script Features
```
- Automatic login when token is missing
- Dynamic variable substitution
- Request preparation and validation
```

### Test Script Features
```
- Response validation
- Environment variable extraction and storage
- Automatic token refresh on 401 responses
- Test result reporting
```

## Environment Variables

### Postman Environment Variables
```
# Service URLs
authServiceUrl=http://localhost:5000
listingServiceUrl=http://localhost:5001
searchServiceUrl=http://localhost:5002
documentProcessorUrl=http://localhost:5003

# Management Console URLs
rabbitmqManagementUrl=http://localhost:15672
openSearchDashboardsUrl=http://localhost:5601
minioConsoleUrl=http://localhost:9001
hangfireDashboardUrl=http://localhost:5003/hangfire

# Standard User
username=testuser
userEmail=testuser@example.com
userPassword=Password123!

# Role-specific Users
adminEmail=admin@example.com
adminPassword=Admin123!
sellerEmail=seller@example.com
sellerPassword=Seller123!
customerEmail=customer@example.com
customerPassword=Customer123!

# Dynamic Variables (populated by tests)
accessToken=
refreshToken=
categoryId=
listingId=
jobId=
documentId=
```
### Container Environments (from docker-compose.yml)

#### Auth Service
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
AuthenticationService__ConnectionString=Server=postgres;Database=MarketplaceAuth;User Id=postgres;Password=postgrespw;
```

#### Listing Service
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5001
ConnectionStrings__DefaultConnection=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=listings
ApplicationConfiguration__RabbitMQSettings__Host=rabbitmq
ApplicationConfiguration__PostgresSqlSettings__ConnectionString=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=listings
```

#### Search Service
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5002
OpenSearch__Uri=http://opensearch:9200
ApplicationConfiguration__RabbitMQSettings__Host=rabbitmq
ApplicationConfiguration__MongoDbSettings__ConnectionString=mongodb://root:mongopw@mongodb:27017
ApplicationConfiguration__OpenSearchSettings__Uri=http://opensearch:9200
```

#### Document Processor
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5003
PostgresSqlSettings__ConnectionString=Server=postgres:5432;Database=hangfire;User Id=postgres;Password=postgrespw;
RabbitMQSettings__Host=rabbitmq
MongoDbSettings__ConnectionString=mongodb://root:mongopw@mongodb:27017
MinIOSettings__Endpoint=minio:9000
```

## Testing Workflow

1. **Start the Environment**
   ```bash
   docker compose up
   ```

2. **Database Initialization**
   - Databases are automatically created and migrated on service startup
   - Authentication roles (User, Seller, Admin) are seeded
   - Listing categories are seeded
   - No manual setup required

3. **Import Postman Files**
   - TheMarketPlace-MS-Collection.postman_collection.json
   - marketplace-postman-environment.json

4. **Run Tests**
   - Select the "TheMarketPlace Microservices" environment
   - Run the collection or individual requests
   - **Important**: Follow the authentication sequence:
     1. Register user (with role=1 for Seller access)
     2. Confirm email (mandatory step)
     3. Login to get tokens
   - Tests will automatically manage tokens and variables

5. **Monitor Results**
   - Check test results in Postman
   - View service logs in Docker
   - Access management dashboards for deeper inspection
