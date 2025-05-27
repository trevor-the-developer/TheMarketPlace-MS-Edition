#!/bin/bash

# TheMarketPlace Microservices API Testing Script
# This script tests the various services in the TheMarketPlace microservices architecture

# Color definitions for better readability
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Environment variables
AUTH_SERVICE_URL="http://localhost:5000"
LISTING_SERVICE_URL="http://localhost:5001"
SEARCH_SERVICE_URL="http://localhost:5002"
DOCUMENT_PROCESSOR_URL="http://localhost:5003"

# Test user credentials
ADMIN_EMAIL="admin@example.com"
ADMIN_PASSWORD="Admin123!"
SELLER_EMAIL="seller@example.com"
SELLER_PASSWORD="Seller123!"
CUSTOMER_EMAIL="customer@example.com"
CUSTOMER_PASSWORD="Customer123!"

# Storage for dynamic variables
ACCESS_TOKEN=""
REFRESH_TOKEN=""
CATEGORY_ID=""
LISTING_ID=""
JOB_ID=""
DOCUMENT_ID=""

# Function to display test header
function test_header() {
    echo -e "\n${BLUE}============================================${NC}"
    echo -e "${BLUE}TEST: $1${NC}"
    echo -e "${BLUE}============================================${NC}"
}

# Function to display test result
function test_result() {
    local status=$1
    local message=$2
    if [ "$status" == "PASS" ]; then
        echo -e "${GREEN}✓ PASS${NC}: $message"
    else
        echo -e "${RED}✗ FAIL${NC}: $message"
    fi
}

# Function to extract value from JSON response
function extract_json_value() {
    local json=$1
    local key=$2
    echo $json | grep -o "\"$key\":[^,}]*" | sed -E 's/"'$key'":"|"//g'
}

# Function to test if a service is running
function test_service_availability() {
    local service_url=$1
    local service_name=$2
    
    test_header "Testing $service_name availability"
    
    if curl -s --head "$service_url" > /dev/null; then
        test_result "PASS" "$service_name is available at $service_url"
        return 0
    else
        test_result "FAIL" "$service_name is not available at $service_url"
        return 1
    fi
}

# Function to login and get access token
function test_login() {
    local email=$1
    local password=$2
    local user_type=$3
    
    test_header "Login as $user_type user"
    
    local response=$(curl -s -X POST "$AUTH_SERVICE_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\": \"$email\", \"password\": \"$password\"}")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains an access token
    if [[ $response == *"accessToken"* ]]; then
        ACCESS_TOKEN=$(echo $response | grep -o '"accessToken":"[^"]*' | sed 's/"accessToken":"//g')
        REFRESH_TOKEN=$(echo $response | grep -o '"refreshToken":"[^"]*' | sed 's/"refreshToken":"//g')
        
        test_result "PASS" "Login successful as $user_type user"
        echo -e "${YELLOW}Access Token:${NC} $ACCESS_TOKEN"
        return 0
    else
        test_result "FAIL" "Login failed as $user_type user"
        return 1
    fi
}

# Function to get user profile
function test_get_profile() {
    test_header "Get User Profile"
    
    local response=$(curl -s -X GET "$AUTH_SERVICE_URL/api/auth/profile" \
        -H "Authorization: Bearer $ACCESS_TOKEN")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains user profile data
    if [[ $response == *"userId"* ]]; then
        test_result "PASS" "Successfully retrieved user profile"
        return 0
    else
        test_result "FAIL" "Failed to retrieve user profile"
        return 1
    fi
}

# Function to list categories
function test_list_categories() {
    test_header "List Categories"
    
    local response=$(curl -s -X GET "$LISTING_SERVICE_URL/api/categories")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains categories
    if [[ $response == *"categoryId"* ]]; then
        # Extract the first category ID
        CATEGORY_ID=$(echo $response | grep -o '"categoryId":"[^"]*' | head -1 | sed 's/"categoryId":"//g')
        
        test_result "PASS" "Successfully retrieved categories"
        echo -e "${YELLOW}First Category ID:${NC} $CATEGORY_ID"
        return 0
    else
        test_result "FAIL" "Failed to retrieve categories"
        return 1
    fi
}

# Function to create a listing
function test_create_listing() {
    test_header "Create Listing"
    
    local response=$(curl -s -X POST "$LISTING_SERVICE_URL/api/listings" \
        -H "Authorization: Bearer $ACCESS_TOKEN" \
        -H "Content-Type: application/json" \
        -d "{
            \"title\": \"Test Listing for Terminal\",
            \"description\": \"This is a test listing created through terminal\",
            \"price\": 99.99,
            \"location\": \"Test City\",
            \"categoryId\": \"$CATEGORY_ID\",
            \"tags\": [\"test\", \"terminal\", \"api\"]
        }")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains a listing ID
    if [[ $response == *"listingId"* ]]; then
        LISTING_ID=$(echo $response | grep -o '"listingId":"[^"]*' | sed 's/"listingId":"//g')
        
        test_result "PASS" "Successfully created listing"
        echo -e "${YELLOW}Listing ID:${NC} $LISTING_ID"
        return 0
    else
        test_result "FAIL" "Failed to create listing"
        return 1
    fi
}

# Function to get a listing
function test_get_listing() {
    test_header "Get Listing"
    
    local response=$(curl -s -X GET "$LISTING_SERVICE_URL/api/listings/$LISTING_ID")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains the listing details
    if [[ $response == *"$LISTING_ID"* ]]; then
        test_result "PASS" "Successfully retrieved listing"
        return 0
    else
        test_result "FAIL" "Failed to retrieve listing"
        return 1
    fi
}

# Function to update a listing
function test_update_listing() {
    test_header "Update Listing"
    
    local response=$(curl -s -X PUT "$LISTING_SERVICE_URL/api/listings/$LISTING_ID" \
        -H "Authorization: Bearer $ACCESS_TOKEN" \
        -H "Content-Type: application/json" \
        -d "{
            \"title\": \"Updated Test Listing for Terminal\",
            \"description\": \"This listing has been updated through terminal\",
            \"price\": 129.99,
            \"location\": \"Test City Updated\",
            \"categoryId\": \"$CATEGORY_ID\",
            \"tags\": [\"test\", \"terminal\", \"api\", \"updated\"]
        }")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains the updated listing
    if [[ $response == *"Updated Test Listing"* ]]; then
        test_result "PASS" "Successfully updated listing"
        return 0
    else
        test_result "FAIL" "Failed to update listing"
        return 1
    fi
}

# Function to search listings
function test_basic_search() {
    test_header "Basic Search"
    
    local response=$(curl -s -X GET "$SEARCH_SERVICE_URL/api/search?query=test")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains search results
    if [[ $response == *"items"* ]]; then
        test_result "PASS" "Successfully performed basic search"
        return 0
    else
        test_result "FAIL" "Failed to perform basic search"
        return 1
    fi
}

# Function to perform advanced search
function test_advanced_search() {
    test_header "Advanced Search"
    
    local response=$(curl -s -X POST "$SEARCH_SERVICE_URL/api/search/advanced" \
        -H "Content-Type: application/json" \
        -d "{
            \"query\": \"test\",
            \"filters\": {
                \"minPrice\": 50,
                \"maxPrice\": 150,
                \"categoryIds\": [\"$CATEGORY_ID\"],
                \"location\": \"Test City\",
                \"tags\": [\"terminal\", \"api\"]
            },
            \"sort\": {
                \"field\": \"price\",
                \"direction\": \"asc\"
            },
            \"pagination\": {
                \"pageNumber\": 1,
                \"pageSize\": 10
            }
        }")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains search results
    if [[ $response == *"items"* ]]; then
        test_result "PASS" "Successfully performed advanced search"
        return 0
    else
        test_result "FAIL" "Failed to perform advanced search"
        return 1
    fi
}

# Function to generate a document
function test_generate_document() {
    test_header "Generate Document"
    
    local response=$(curl -s -X POST "$DOCUMENT_PROCESSOR_URL/api/documents/generate" \
        -H "Authorization: Bearer $ACCESS_TOKEN" \
        -H "Content-Type: application/json" \
        -d "{
            \"type\": \"listing_pdf\",
            \"parameters\": {
                \"listingId\": \"$LISTING_ID\",
                \"includeImages\": true,
                \"includeContactInfo\": true
            }
        }")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains a job ID
    if [[ $response == *"jobId"* ]]; then
        JOB_ID=$(echo $response | grep -o '"jobId":"[^"]*' | sed 's/"jobId":"//g')
        
        test_result "PASS" "Successfully submitted document generation job"
        echo -e "${YELLOW}Job ID:${NC} $JOB_ID"
        return 0
    else
        test_result "FAIL" "Failed to submit document generation job"
        return 1
    fi
}

# Function to check document status
function test_check_document_status() {
    test_header "Check Document Status"
    
    local response=$(curl -s -X GET "$DOCUMENT_PROCESSOR_URL/api/documents/jobs/$JOB_ID" \
        -H "Authorization: Bearer $ACCESS_TOKEN")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains a status
    if [[ $response == *"status"* ]]; then
        # Extract document ID if job is completed
        if [[ $response == *"Completed"* ]] && [[ $response == *"documentId"* ]]; then
            DOCUMENT_ID=$(echo $response | grep -o '"documentId":"[^"]*' | sed 's/"documentId":"//g')
            echo -e "${YELLOW}Document ID:${NC} $DOCUMENT_ID"
        fi
        
        test_result "PASS" "Successfully checked document status"
        return 0
    else
        test_result "FAIL" "Failed to check document status"
        return 1
    fi
}

# Function to delete a listing
function test_delete_listing() {
    test_header "Delete Listing"
    
    local response=$(curl -s -X DELETE "$LISTING_SERVICE_URL/api/listings/$LISTING_ID" \
        -H "Authorization: Bearer $ACCESS_TOKEN" \
        -i)
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response status code is 204 (No Content)
    if [[ $response == *"204 No Content"* ]]; then
        test_result "PASS" "Successfully deleted listing"
        return 0
    else
        test_result "FAIL" "Failed to delete listing"
        return 1
    fi
}

# Main function to run all tests
function run_all_tests() {
    echo -e "\n${GREEN}=========================================${NC}"
    echo -e "${GREEN}MARKETPLACE SERVICES TEST SUITE${NC}"
    echo -e "${GREEN}=========================================${NC}"
    
    # Test service availability
    test_service_availability "$AUTH_SERVICE_URL" "Auth Service"
    test_service_availability "$LISTING_SERVICE_URL" "Listing Service"
    test_service_availability "$SEARCH_SERVICE_URL" "Search Service"
    test_service_availability "$DOCUMENT_PROCESSOR_URL" "Document Processor"
    
    # Authentication tests
    test_login "$ADMIN_EMAIL" "$ADMIN_PASSWORD" "Admin"
    test_get_profile
    
    # Listing tests
    test_list_categories
    test_create_listing
    test_get_listing
    test_update_listing
    
    # Search tests
    test_basic_search
    test_advanced_search
    
    # Document processor tests
    test_generate_document
    sleep 2  # Wait a bit for the job to process
    test_check_document_status
    
    # Cleanup
    test_delete_listing
    
    echo -e "\n${GREEN}=========================================${NC}"
    echo -e "${GREEN}TEST SUITE COMPLETED${NC}"
    echo -e "${GREEN}=========================================${NC}"
}

# Function to run specific test groups
function run_auth_tests() {
    test_service_availability "$AUTH_SERVICE_URL" "Auth Service"
    test_login "$ADMIN_EMAIL" "$ADMIN_PASSWORD" "Admin"
    test_get_profile
}

function run_listing_tests() {
    test_service_availability "$LISTING_SERVICE_URL" "Listing Service"
    test_login "$ADMIN_EMAIL" "$ADMIN_PASSWORD" "Admin"
    test_list_categories
    test_create_listing
    test_get_listing
    test_update_listing
    test_delete_listing
}

function run_search_tests() {
    test_service_availability "$SEARCH_SERVICE_URL" "Search Service"
    test_login "$ADMIN_EMAIL" "$ADMIN_PASSWORD" "Admin"
    test_list_categories
    test_create_listing
    test_basic_search
    test_advanced_search
    test_delete_listing
}

function run_document_tests() {
    test_service_availability "$DOCUMENT_PROCESSOR_URL" "Document Processor"
    test_login "$ADMIN_EMAIL" "$ADMIN_PASSWORD" "Admin"
    test_list_categories
    test_create_listing
    test_generate_document
    sleep 2  # Wait a bit for the job to process
    test_check_document_status
    test_delete_listing
}

# Parse command line arguments
case "$1" in
    "auth")
        run_auth_tests
        ;;
    "listing")
        run_listing_tests
        ;;
    "search")
        run_search_tests
        ;;
    "document")
        run_document_tests
        ;;
    *)
        run_all_tests
        ;;
esac

# Make the script executable with: chmod +x api_tests.sh
# Run all tests with: ./api_tests.sh
# Run specific test group with: ./api_tests.sh auth|listing|search|document

