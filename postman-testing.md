# TheMarketPlace Microservices API Testing Guide

## Prerequisites
- All services running (Auth, Listing, Search, Document Processor)
- PostgreSQL database running
- RabbitMQ running for inter-service communication
- Environment variables set correctly

## Authentication Service Tests

### 1. User Registration
```http
POST http://localhost:5000/api/register
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "User",
  "email": "testuser@example.com",
  "password": "Test@123",
  "dateOfBirth": "2000-01-01T00:00:00Z",
  "role": 1
}
```
**Expected Response:**
- Status: 200 OK
- Confirmation token in response body

### 2. Email Confirmation
```http
GET http://localhost:5000/api/confirm-email?email=testuser@example.com&token={token}
```
**Expected Response:**
- Status: 200 OK
- Success message and confirmation code

### 3. User Login
```http
POST http://localhost:5000/api/login
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Test@123"
}
```
**Expected Response:**
- Status: 200 OK
- Access token, refresh token, and expiration time
- Save the access token for subsequent requests

## Listing Service Tests

### 4. List Categories
```http
GET http://localhost:5001/api/categories
Authorization: Bearer {access_token}
```
**Expected Response:**
- Status: 200 OK
- Array of categories
- Save a categoryId for later use

### 5. Create Listing
```http
POST http://localhost:5001/api/listings
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "title": "Test Listing for Postman",
  "description": "This is a test listing created through Postman",
  "price": 99.99,
  "location": "Test City",
  "categoryId": "{categoryId}",
  "tags": ["test", "postman", "api"]
}
```
**Expected Response:**
- Status: 201 Created
- Listing details with listingId
- Save the listingId for later use

### 6. Get Listing
```http
GET http://localhost:5001/api/listings/{listingId}
Authorization: Bearer {access_token}
```
**Expected Response:**
- Status: 200 OK
- Detailed listing information

### 7. Update Listing
```http
PUT http://localhost:5001/api/listings/{listingId}
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "title": "Updated Test Listing for Postman",
  "description": "This listing has been updated through Postman",
  "price": 129.99,
  "location": "Test City Updated",
  "categoryId": "{categoryId}",
  "tags": ["test", "postman", "api", "updated"]
}
```
**Expected Response:**
- Status: 200 OK
- Updated listing details

## Search Service Tests

### 8. Basic Search
```http
GET http://localhost:5002/api/search?query=test
Authorization: Bearer {access_token}
```
**Expected Response:**
- Status: 200 OK
- Search results matching the query

### 9. Advanced Search
```http
POST http://localhost:5002/api/search/advanced
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "query": "test",
  "filters": {
    "minPrice": 50,
    "maxPrice": 150,
    "categoryIds": ["{categoryId}"],
    "location": "Test City",
    "tags": ["postman", "api"]
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
- Filtered and sorted search results

## Document Processor Tests

### 10. Generate Document
```http
POST http://localhost:5003/api/documents/generate
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "type": "listing_pdf",
  "parameters": {
    "listingId": "{listingId}",
    "includeImages": true,
    "includeContactInfo": true
  }
}
```
**Expected Response:**
- Status: 202 Accepted
- Job ID for document generation
- Save the jobId for checking status

### 11. Check Document Status
```http
GET http://localhost:5003/api/documents/jobs/{jobId}
Authorization: Bearer {access_token}
```
**Expected Response:**
- Status: 200 OK
- Job status (Processing, Completed, Failed)
- If completed, a documentId will be provided
- Save the documentId for downloading

### 12. Download Document (when ready)
```http
GET http://localhost:5003/api/documents/download/{documentId}
Authorization: Bearer {access_token}
```
**Expected Response:**
- Status: 200 OK
- PDF document in response body
- Content-Type: application/pdf

## Additional Auth Operations

### 13. Refresh Token
```http
POST http://localhost:5000/api/refresh
Content-Type: application/json

{
  "token": "{current_token}",
  "refreshToken": "{refresh_token}"
}
```
**Expected Response:**
- Status: 200 OK
- New access token and refresh token

### 14. Logout
```http
POST http://localhost:5000/api/logout
Content-Type: application/json

{
  "email": "testuser@example.com"
}
```
**Expected Response:**
- Status: 200 OK
- Success message

## Tips for Testing

- Replace `{access_token}`, `{categoryId}`, `{listingId}`, etc. with actual values received from previous responses
- All requests except registration and login require a valid Bearer token in the Authorization header
- Some operations may require specific user roles (Admin, User, Seller)
- The order of operations is important as some operations depend on data from previous steps
- If you receive a 401 Unauthorized error, your token may have expired - use the refresh token endpoint to get a new one

