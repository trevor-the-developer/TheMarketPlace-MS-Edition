# TheMarketPlace Microservices API Testing Guide

## Prerequisites
- All services running (Auth, Listing, Search, Document Processor)
- PostgreSQL database running
- RabbitMQ running for inter-service communication
- OpenSearch for search functionality
- MongoDB for document metadata storage
- MinIO for object storage
- Environment variables set correctly

## Postman Setup
1. Import the collection file: `TheMarketPlace-MS-Collection.postman_collection.json`
2. Import the environment file: `marketplace-postman-environment.json`
3. Select the "TheMarketPlace Microservices" environment from the dropdown
4. You're ready to test the APIs!

## Testing Sequence Overview

For successful testing, follow this sequence:
1. Register a user with appropriate role
2. Confirm the user's email (mandatory step)
3. Login to get authentication tokens
4. Use these tokens for service-specific operations
5. Refresh token if needed
6. Logout when finished

## User Roles and Capabilities

The Marketplace supports three user roles, each with different capabilities:

### Admin User (Role Value: 2)
- Email: `admin@example.com`
- Password: `Admin123!`
- Capabilities:
  - Manage all users and listings
  - View system statistics
  - Configure system settings
  - Access all APIs

### Seller User (Role Value: 1)
- Email: `seller@example.com`
- Password: `Seller123!`
- Capabilities:
  - Create and manage their own listings
  - View listing statistics
  - Generate listing documents
  - **Required for listing creation and management**

### Customer User (Role Value: 0)
- Email: `customer@example.com`
- Password: `Customer123!`
- Capabilities:
  - Browse and search listings
  - View listing details
  - Download listing documents

> **IMPORTANT**: When registering users, specify the appropriate role value (0, 1, or 2) to ensure proper permissions.

## Authentication Service Tests

### 1. User Registration
```http
POST {{authServiceUrl}}/api/register
Content-Type: application/json

{
  "firstName": "{{firstName}}",
  "lastName": "{{lastName}}",
  "email": "{{userEmail}}",
  "password": "{{userPassword}}",
  "role": {{userRole}},
  "dateOfBirth": "{{dateOfBirth}}"
}
```
**Expected Response:**
- Status: 200 OK
- Registration data in response body including:
  ```json
  {
    "status": 0,
    "message": "Operation completed successfully",
    "data": {
      "registrationStepOne": true,
      "email": "testuser@example.com",
      "confirmationToken": "123456",
      "apiError": null,
      "errors": null
    }
  }
  ```
- The confirmation token is automatically saved to the environment variable for the next step

### 2. Email Confirmation (REQUIRED)
```http
GET {{authServiceUrl}}/api/confirm-email?email={{userEmail}}&token={{confirmationToken}}
```
**Expected Response:**
- Status: 200 OK
- Confirmation response including:
  ```json
  {
    "status": 0,
    "message": "Operation completed successfully",
    "data": {
      "succeeded": true,
      "email": "testuser@example.com",
      "confirmationCode": "79d21e5c-f559-4320-82df-cca869b69cd6",
      "apiError": null,
      "errors": null
    }
  }
  ```

### 3. User Login
```http
POST {{authServiceUrl}}/api/login
Content-Type: application/json

{
  "email": "{{userEmail}}",
  "password": "{{userPassword}}"
}
```
**Expected Response:**
- Status: 200 OK
- Login response including:
  ```json
  {
    "status": 0,
    "message": "Operation completed successfully",
    "data": {
      "succeeded": true,
      "id": "b31069dd-a1c7-4b55-a620-39f962743a9e",
      "email": "testuser@example.com",
      "securityToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiration": "2025-05-26T19:32:37Z",
      "refreshToken": "u8gy4ZqkXxAn55enmFxj738u6zi+hhN4LpuTjeE+N28=",
      "apiError": null,
      "errors": null
    }
  }
  ```
- Tokens are automatically saved to environment variables for subsequent requests

### 4. Get User Profile
```http
GET {{authServiceUrl}}/api/profile
Authorization: Bearer {{accessToken}}
```
**Expected Response:**
- Status: 200 OK
- User profile information including userId, username, and email
### 5. Refresh Token
```http
POST {{authServiceUrl}}/api/refresh
Content-Type: application/json

{
  "refreshToken": "{{refreshToken}}"
}
```
**Expected Response:**
- Status: 200 OK
- New access token and refresh token in the same format as the login response
- Tokens are automatically updated in environment variables

### 6. Logout
```http
POST {{authServiceUrl}}/api/logout
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "refreshToken": "{{refreshToken}}"
}
```
**Expected Response:**
- Status: 200 OK
- Tokens are revoked and automatically cleared from environment variables
- Response format:
  ```json
  {
    "status": 0,
    "message": "Operation completed successfully",
    "data": {
      "succeeded": true
    }
  }
  ```

## Listing Service Tests
> **IMPORTANT**: Creating and managing listings requires a user with the Seller role (role value: 1)

### 5. List Categories
```http
GET {{listingServiceUrl}}/api/categories
```
**Expected Response:**
- Status: 200 OK
- Array of categories
- First category ID is automatically saved to the categoryId environment variable

### 6. Create Listing
```http
POST {{listingServiceUrl}}/api/listings
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "title": "Test Listing for Postman",
  "description": "This is a test listing created through Postman",
  "price": 99.99,
  "location": "Test City",
  "categoryId": "{{categoryId}}",
  "tagNames": ["test", "postman", "api"]
}
```
**Expected Response:**
- Status: 201 Created
- Listing details with listingId
- Listing ID is automatically saved to the listingId environment variable

### 7. Get Listing
```http
GET {{listingServiceUrl}}/api/listings/{{listingId}}
```
**Expected Response:**
- Status: 200 OK
- Detailed listing information

### 8. Update Listing
```http
PUT {{listingServiceUrl}}/api/listings/{{listingId}}
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "title": "Updated Test Listing for Postman",
  "description": "This listing has been updated through Postman",
  "price": 129.99,
  "location": "Test City Updated",
  "categoryId": "{{categoryId}}",
  "tagNames": ["test", "postman", "api", "updated"]
}
```
**Expected Response:**
- Status: 200 OK
- Updated listing details
### 9. Delete Listing
```http
DELETE {{listingServiceUrl}}/api/listings/{{listingId}}
Authorization: Bearer {{accessToken}}
```
**Expected Response:**
- Status: 204 No Content

## Search Service Tests

### 10. Basic Search
```http
GET {{searchServiceUrl}}/api/search?query=test
```
**Expected Response:**
- Status: 200 OK
- Search results matching the query with pagination information

### 11. Advanced Search
```http
POST {{searchServiceUrl}}/api/search/advanced
Content-Type: application/json

{
  "query": "test",
  "filters": {
    "minPrice": 50,
    "maxPrice": 150,
    "categoryIds": ["{{categoryId}}"],
    "location": "Test City",
    "tagNames": ["postman", "api"]
  },
  "sort": {
    "field": "price",
    "direction": "asc"
  },
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10
  }
}
```
**Expected Response:**
- Status: 200 OK
- Filtered and sorted search results with pagination information

## Document Processor Tests

> **NOTE**: The Document Processor is a background service that operates via message-driven architecture (RabbitMQ), not REST API endpoints. Document generation is triggered automatically when certain events occur (e.g., listing creation). Direct REST API endpoints for document generation are not currently implemented.

### Document Processing Flow
1. When a listing is created or updated, a message is sent to the Document Processor
2. The processor generates documents in the background using Hangfire
3. Generated documents are stored in MinIO object storage
4. Document status can be monitored via the Hangfire dashboard at `{{hangfireDashboardUrl}}`

### Monitoring Document Jobs
- Access the Hangfire Dashboard: `{{hangfireDashboardUrl}}` (http://localhost:5003/hangfire)
- View job queue status, success/failure rates, and job history
- Retry failed jobs manually if needed
## Automated Scripts

The Postman collection includes pre-request and test scripts that automate several tasks:

### Pre-request Scripts
- Automatically check if access token exists
- If no token exists, it attempts to login using the credentials in environment variables
- Sets necessary headers and parameters

### Test Scripts
- Validate response status codes and data structure
- Extract important values (IDs, tokens) and save them to environment variables
- Handle token refresh when a 401 Unauthorized response is received

## Troubleshooting Common Issues

### 1. Authentication Failures
- **Issue**: 401 Unauthorized responses even with valid credentials
- **Solution**: Ensure the user has completed email confirmation. Without confirmation, login will fail.

### 2. Permission Denied for Listing Operations
- **Issue**: 403 Forbidden when trying to create or modify listings
- **Solution**: Ensure the user has the Seller role (role value: 1). Customer users (role 0) cannot create listings.

### 3. User ID Not Recognized
- **Issue**: "User ID could not be determined from authenticated user" error
- **Solution**: This indicates a claim type mismatch between services. Ensure you're using the latest version of all services.

### 4. Invalid Role Assignment
- **Issue**: "Role X does not exist" error during registration
- **Solution**: Use only the valid role values: 0 (Customer), 1 (Seller), or 2 (Admin)

### 5. Email Already Exists
- **Issue**: 409 Conflict response when registering
- **Solution**: Use a different email address or login with the existing account

## Supporting Services Dashboards

### RabbitMQ Management Console
- URL: {{rabbitmqManagementUrl}} (http://localhost:15672)
- Username: guest
- Password: guest
- Features:
  - Monitor queues and exchanges
  - View message rates and statistics
  - Manage users and permissions

### OpenSearch Dashboards
- URL: {{openSearchDashboardsUrl}} (http://localhost:5601)
- Features:
  - View and search indexed data
  - Create visualizations and dashboards
  - Monitor search performance

### MinIO Console
- URL: {{minioConsoleUrl}} (http://localhost:9001)
- Username: minioadmin
- Password: minioadmin
- Features:
  - Browse and manage buckets and objects
  - Upload and download files
  - Configure access policies

### Hangfire Dashboard
- URL: {{hangfireDashboardUrl}} (http://localhost:5003/hangfire)
- Features:
  - Monitor background jobs
  - View job history and statistics
  - Retry failed jobs

## Environment Variables

The Postman environment includes the following variables:

```
authServiceUrl=http://localhost:5000
listingServiceUrl=http://localhost:5001
searchServiceUrl=http://localhost:5002
documentProcessorUrl=http://localhost:5003
username=testuser
userEmail=testuser@example.com
userPassword=Password123!
accessToken=
refreshToken=
categoryId=
listingId=
jobId=
documentId=
adminEmail=admin@example.com
adminPassword=Admin123!
sellerEmail=seller@example.com
sellerPassword=Seller123!
customerEmail=customer@example.com
customerPassword=Customer123!
rabbitmqManagementUrl=http://localhost:15672
openSearchDashboardsUrl=http://localhost:5601
minioConsoleUrl=http://localhost:9001
hangfireDashboardUrl=http://localhost:5003/hangfire
```

## Tips for Testing

- Variables like {{accessToken}}, {{categoryId}}, {{listingId}} are automatically managed by the test scripts
- All requests requiring authentication use the Bearer token scheme
- Different user roles (Admin, Seller, Customer) have different permissions
- The order of operations is important as some operations depend on data from previous steps
- If you receive a 401 Unauthorized error, the collection will automatically attempt to refresh the token

## Complete Testing Workflow

For the most reliable testing experience, follow this sequence:

1. **Authentication Setup**
   - Register a new user with appropriate role (Seller for listing management)
   - Confirm email using the token received during registration
   - Login to get access and refresh tokens

2. **Listing Operations**
   - List categories and note a category ID
   - Create a new listing (requires Seller role)
   - Get the listing details
   - Update the listing
   - (Optional) Delete the listing when done

3. **Search Operations**
   - Perform basic search
   - Try advanced search with filters

4. **Document Operations**
   - Generate a document from a listing
   - Check document status
   - Download the document when ready

5. **Cleanup**
   - Logout to revoke tokens

