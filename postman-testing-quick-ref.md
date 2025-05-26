# TheMarketPlace API Quick Reference

## Auth Service (localhost:5000)
```
POST   /api/register         - Register new user
GET    /api/confirm-email    - Confirm email with token
POST   /api/login            - Login and get tokens
POST   /api/refresh          - Refresh access token
POST   /api/logout           - Logout and revoke token
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
POST   /api/documents/generate        - Generate document from listing
GET    /api/documents/jobs/{jobId}    - Check document generation status
GET    /api/documents/download/{id}   - Download generated document
```

## Authentication Flow
1. Register → 2. Confirm Email → 3. Login → 4. Use Access Token in other requests

## Common Headers
```
Authorization: Bearer {access_token}  - Required for authenticated endpoints
Content-Type: application/json        - Required for POST/PUT requests
```

## Key Request Bodies

### Register User
```json
{
  "firstName": "Test", "lastName": "User",
  "email": "test@example.com", "password": "Test@123",
  "dateOfBirth": "2000-01-01T00:00:00Z", "role": 1
}
```

### Create Listing
```json
{
  "title": "Test Listing", "description": "Description",
  "price": 99.99, "location": "Test City",
  "categoryId": "{categoryId}", "tags": ["test", "api"]
}
```

### Advanced Search
```json
{
  "query": "test",
  "filters": {
    "minPrice": 50, "maxPrice": 150,
    "categoryIds": ["{categoryId}"],
    "location": "Test City", "tags": ["api"]
  },
  "sort": { "field": "price", "direction": "asc" },
  "pagination": { "pageNumber": 1, "pageSize": 10 }
}
```

## Environment Variables

### Postman Environment Variables
```
authServiceUrl=http://localhost:5000
listingServiceUrl=http://localhost:5001
searchServiceUrl=http://localhost:5002
documentProcessorUrl=http://localhost:5003
username=testuser
userEmail=testuser@example.com
userPassword=Test@123
accessToken=
refreshToken=
categoryId=
listingId=
jobId=
documentId=
```

### Auth Service Container Environment
```
AuthenticationService__ConnectionString=Server=postgres;Database=MarketplaceAuth;User Id=postgres;Password=postgrespw;
JwtSettings__Key=your_jwt_secret_key_should_be_at_least_32_characters_long
JwtSettings__Issuer=MarketplaceIdentity
JwtSettings__Audience=MarketplaceApi
JwtSettings__DurationInMinutes=60
```

### Listing Service Container Environment
```
ListingService__ConnectionString=Server=postgres;Database=MarketplaceListings;User Id=postgres;Password=postgrespw;
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

### Search Service Container Environment
```
SearchService__ConnectionString=Server=postgres;Database=MarketplaceSearch;User Id=postgres;Password=postgrespw;
OpenSearch__Uri=http://opensearch:9200
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

### Document Processor Container Environment
```
DocumentProcessor__ConnectionString=Server=postgres;Database=MarketplaceDocuments;User Id=postgres;Password=postgrespw;
MinIO__Endpoint=minio:9000
MinIO__AccessKey=minioadmin
MinIO__SecretKey=minioadmin
MinIO__BucketName=documents
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

