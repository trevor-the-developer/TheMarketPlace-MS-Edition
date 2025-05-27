#!/bin/bash

# TheMarketPlace Microservices API Testing Script
# This script tests the various services in the TheMarketPlace microservices architecture
# Aligned with postman collection and documentation

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

# Test user credentials (aligned with postman environment)
TEST_USER_EMAIL="testuser@example.com"
TEST_USER_PASSWORD="Password123!"
TEST_USER_ROLE=1  # Seller role for listing operations
ADMIN_EMAIL="admin@example.com"
ADMIN_PASSWORD="Admin123!"
SELLER_EMAIL="seller@example.com"
SELLER_PASSWORD="Seller123!"
CUSTOMER_EMAIL="customer@example.com"
CUSTOMER_PASSWORD="Customer123!"

# Storage for dynamic variables
ACCESS_TOKEN=""
REFRESH_TOKEN=""
CONFIRMATION_TOKEN=""
CATEGORY_ID=""
LISTING_ID=""
JOB_ID=""
DOCUMENT_ID=""

# Docker management flags
SETUP_DOCKER=false
TEARDOWN_DOCKER=false
CLEAN_VOLUMES=false
USE_UNIQUE_EMAIL=false
SKIP_REGISTRATION=false
CLEAN_DATABASES_ONLY=false

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
    elif [ "$status" == "INFO" ]; then
        echo -e "${YELLOW}ℹ INFO${NC}: $message"
    else
        echo -e "${RED}✗ FAIL${NC}: $message"
    fi
}

# Function to check if Docker is running and supports compose v2
function check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}ERROR: Docker is not running. Please start Docker and try again.${NC}"
        exit 1
    fi
    
    # Check if Docker Compose v2 is available
    if ! docker compose version > /dev/null 2>&1; then
        echo -e "${RED}ERROR: Docker Compose v2 is not available. Please update Docker Desktop or install Docker Compose v2.${NC}"
        echo -e "${YELLOW}Note: This script requires 'docker compose' (v2) not 'docker-compose' (v1)${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✓ Docker and Docker Compose v2 are available${NC}"
}

# Function to clean databases only (without full container restart)
function clean_databases() {
    test_header "Cleaning Databases"
    
    check_docker
    
    echo -e "${YELLOW}Stopping services to clean databases...${NC}"
    docker compose stop auth-svc listing-svc search-svc document-processor 2>/dev/null || true
    
    echo -e "${YELLOW}Removing database volumes...${NC}"
    # Try common volume naming patterns
    docker volume rm themarketplace-ms-edition_postgres-data 2>/dev/null || true
    docker volume rm themarketplace-ms-edition_mongodb-data 2>/dev/null || true
    docker volume rm themarketplace-ms-edition_opensearch-data 2>/dev/null || true
    docker volume rm marketplace_postgres-data 2>/dev/null || true
    docker volume rm marketplace_mongodb-data 2>/dev/null || true
    docker volume rm marketplace_opensearch-data 2>/dev/null || true
    
    # List volumes and try to remove any that contain common database keywords
    echo -e "${YELLOW}Checking for additional database volumes...${NC}"
    docker volume ls --format "{{.Name}}" | grep -E "(postgres|mongo|mysql|redis|elastic|opensearch)" | while read volume; do
        echo -e "${YELLOW}Removing volume: $volume${NC}"
        docker volume rm "$volume" 2>/dev/null || true
    done
    
    echo -e "${YELLOW}Restarting services with clean databases...${NC}"
    docker compose up --detach
    
    echo -e "${YELLOW}Waiting for services to initialize with clean databases...${NC}"
    sleep 30
    
    # Wait for services to be ready
    local max_attempts=15
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if curl -s http://localhost:5000/health > /dev/null && \
           curl -s http://localhost:5001/api/categories > /dev/null; then
            echo -e "${GREEN}✓ Services ready with clean databases!${NC}"
            return 0
        fi
        
        attempt=$((attempt + 1))
        echo -e "${YELLOW}Waiting for services to be ready... (attempt $attempt/$max_attempts)${NC}"
        sleep 5
    done
    
    echo -e "${RED}✗ Services failed to start after database cleaning${NC}"
    return 1
}

# Function to setup Docker environment
function setup_docker_environment() {
    if [ "$SETUP_DOCKER" = true ]; then
        test_header "Setting up Docker Environment"
        
        check_docker
        
        echo -e "${YELLOW}Checking existing containers...${NC}"
        if docker compose ps --status running | grep -q "running"; then
            echo -e "${YELLOW}Some containers are already running. Stopping them first...${NC}"
            docker compose down
        fi
        
        if [ "$CLEAN_VOLUMES" = true ]; then
            echo -e "${YELLOW}Cleaning volumes for fresh start...${NC}"
            docker compose down --volumes --remove-orphans
            docker system prune -f --volumes
        fi
        
        echo -e "${YELLOW}Starting all services...${NC}"
        docker compose up --detach
        
        echo -e "${YELLOW}Waiting for services to be ready...${NC}"
        sleep 30
        
        # Wait for services to be healthy
        local max_attempts=20
        local attempt=0
        
        while [ $attempt -lt $max_attempts ]; do
            if curl -s http://localhost:5000/health > /dev/null && \
               curl -s http://localhost:5001/api/categories > /dev/null && \
               curl -s http://localhost:5002/api/search > /dev/null; then
                echo -e "${GREEN}✓ All services are ready!${NC}"
                break
            fi
            
            attempt=$((attempt + 1))
            echo -e "${YELLOW}Waiting for services... (attempt $attempt/$max_attempts)${NC}"
            sleep 5
        done
        
        if [ $attempt -eq $max_attempts ]; then
            echo -e "${RED}✗ Services failed to start within expected time${NC}"
            echo -e "${YELLOW}Showing service logs for debugging:${NC}"
            docker compose logs --tail=50
            exit 1
        fi
    fi
}

# Function to teardown Docker environment
function teardown_docker_environment() {
    if [ "$TEARDOWN_DOCKER" = true ]; then
        test_header "Tearing down Docker Environment"
        
        echo -e "${YELLOW}Stopping all services...${NC}"
        docker compose down
        
        if [ "$CLEAN_VOLUMES" = true ]; then
            echo -e "${YELLOW}Removing volumes and orphaned containers...${NC}"
            docker compose down --volumes --remove-orphans
        fi
        
        echo -e "${GREEN}✓ Docker environment cleaned up${NC}"
    fi
}

# Function to extract value from JSON response using jq if available, fallback to grep
function extract_json_value() {
    local json=$1
    local key=$2
    
    if command -v jq &> /dev/null; then
        echo "$json" | jq -r ".data.$key // .data.${key} // .$key // empty"
    else
        # Fallback to grep/sed method
        echo "$json" | grep -o "\"$key\":[^,}]*" | sed -E 's/"'$key'":"|"//g'
    fi
}

# Function to test if a service is running
function test_service_availability() {
    local service_url=$1
    local service_name=$2
    
    test_header "Testing $service_name availability"
    
    local max_attempts=5
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        # Try health endpoint first, then fallback to basic connectivity
        if curl -s "$service_url/health" > /dev/null 2>&1 || \
           curl -s "$service_url/api/health" > /dev/null 2>&1 || \
           curl -s --head "$service_url" > /dev/null 2>&1; then
            test_result "PASS" "$service_name is available at $service_url"
            return 0
        fi
        
        attempt=$((attempt + 1))
        echo -e "${YELLOW}Attempt $attempt/$max_attempts: Waiting for $service_name...${NC}"
        sleep 2
    done
    
    test_result "FAIL" "$service_name is not available at $service_url after $max_attempts attempts"
    return 1
}

# Function to register a new user
function test_register_user() {
    local email=$1
    local password=$2
    local role=$3
    local user_type=$4
    
    test_header "Register $user_type User"
    
    local response=$(curl -s -X POST "$AUTH_SERVICE_URL/api/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"firstName\": \"Test\",
            \"lastName\": \"User\",
            \"email\": \"$email\",
            \"password\": \"$password\",
            \"role\": $role,
            \"dateOfBirth\": \"1990-01-01T00:00:00Z\"
        }")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains registration data
    if [[ $response == *"confirmationToken"* ]]; then
        CONFIRMATION_TOKEN=$(extract_json_value "$response" "confirmationToken")
        
        test_result "PASS" "Registration successful for $user_type user"
        echo -e "${YELLOW}Confirmation Token:${NC} $CONFIRMATION_TOKEN"
        return 0
    elif [[ $response == *"Email already exists"* ]] || [[ $response == *"already registered"* ]]; then
        test_result "INFO" "User already exists, skipping registration for $user_type user"
        return 2  # Special return code for existing user
    else
        test_result "FAIL" "Registration failed for $user_type user"
        echo -e "${RED}Error details: $response${NC}"
        return 1
    fi
}

# Function to confirm email
function test_confirm_email() {
    local email=$1
    local user_type=$2
    
    test_header "Confirm Email for $user_type User"
    
    local response=$(curl -s -G "$AUTH_SERVICE_URL/api/confirm-email" \
        --data-urlencode "email=$email" \
        --data-urlencode "token=$CONFIRMATION_TOKEN")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains success confirmation
    if [[ $response == *"succeeded"* ]] && [[ $response == *"true"* ]]; then
        test_result "PASS" "Email confirmation successful for $user_type user"
        return 0
    else
        test_result "FAIL" "Email confirmation failed for $user_type user"
        return 1
    fi
}

# Function to login and get access token
function test_login() {
    local email=$1
    local password=$2
    local user_type=$3
    
    test_header "Login as $user_type user"
    
    local response=$(curl -s -X POST "$AUTH_SERVICE_URL/api/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\": \"$email\", \"password\": \"$password\"}")
    
    echo -e "${YELLOW}Response:${NC} $response"
    
    # Check if the response contains a security token (corrected from accessToken)
    if [[ $response == *"securityToken"* ]]; then
        ACCESS_TOKEN=$(extract_json_value "$response" "securityToken")
        REFRESH_TOKEN=$(extract_json_value "$response" "refreshToken")
        
        test_result "PASS" "Login successful as $user_type user"
        echo -e "${YELLOW}Access Token:${NC} ${ACCESS_TOKEN:0:50}..."
        return 0
    else
        test_result "FAIL" "Login failed as $user_type user"
        return 1
    fi
}

# Function to list categories
function test_list_categories() {
    test_header "List Categories"
    
    local response=$(curl -s -X GET "$LISTING_SERVICE_URL/api/categories")
    
    echo -e "${YELLOW}Response:${NC} ${response:0:500}..."
    
    # Check if the response contains categories (updated to check for 'id' instead of 'categoryId')
    if [[ $response == *"\"id\":"* ]]; then
        # Extract the first category ID
        CATEGORY_ID=$(extract_json_value "$response" "id")
        if [ -z "$CATEGORY_ID" ]; then
            # Fallback extraction for nested data structure
            CATEGORY_ID=$(echo "$response" | grep -o '"id":"[^"]*' | head -1 | sed 's/"id":"//g')
        fi
        
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
    if [[ $response == *"\"id\":"* ]]; then
        LISTING_ID=$(extract_json_value "$response" "id")
        
        test_result "PASS" "Successfully created listing"
        echo -e "${YELLOW}Listing ID:${NC} $LISTING_ID"
        return 0
    else
        test_result "FAIL" "Failed to create listing"
        echo -e "${RED}Error: $response${NC}"
        return 1
    fi
}

# Function to get a listing
function test_get_listing() {
    test_header "Get Listing"
    
    local response=$(curl -s -X GET "$LISTING_SERVICE_URL/api/listings/$LISTING_ID")
    
    echo -e "${YELLOW}Response:${NC} ${response:0:500}..."
    
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
    if [[ $response == *"Updated Test Listing"* ]] || [[ $response == *"\"title\":"* ]]; then
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
    
    # Check if the response contains search results structure
    if [[ $response == *"data"* ]] && [[ $response == *"status"* ]]; then
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
    
    # Check if the response contains search results structure
    if [[ $response == *"data"* ]] && [[ $response == *"status"* ]]; then
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
        JOB_ID=$(extract_json_value "$response" "jobId")
        
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
            DOCUMENT_ID=$(extract_json_value "$response" "documentId")
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
        -w "%{http_code}" -o /dev/null)
    
    echo -e "${YELLOW}HTTP Status Code:${NC} $response"
    
    # Check if the response status code is 204 (No Content) or 200 (OK)
    if [[ "$response" == "204" ]] || [[ "$response" == "200" ]]; then
        test_result "PASS" "Successfully deleted listing"
        return 0
    else
        test_result "FAIL" "Failed to delete listing (HTTP $response)"
        return 1
    fi
}

# Function to run complete authentication flow
function run_complete_auth_flow() {
    local email=$1
    local password=$2
    local role=$3
    local user_type=$4
    
    echo -e "\n${GREEN}======== COMPLETE AUTHENTICATION FLOW ========${NC}"
    
    # If using unique email, generate one
    if [ "$USE_UNIQUE_EMAIL" = true ]; then
        local timestamp=$(date +%s)
        email="${email%%@*}_${timestamp}@${email#*@}"
        echo -e "${YELLOW}Using unique email: $email${NC}"
    fi
    
    # If skip registration flag is set, try login first
    if [ "$SKIP_REGISTRATION" = true ]; then
        echo -e "${YELLOW}Skipping registration, attempting direct login...${NC}"
        test_login "$email" "$password" "$user_type"
        return $?
    fi
    
    # Standard registration flow
    test_register_user "$email" "$password" "$role" "$user_type"
    local reg_result=$?
    
    if [ $reg_result -eq 0 ]; then
        # New user registered successfully
        test_confirm_email "$email" "$user_type"
        if [ $? -eq 0 ]; then
            test_login "$email" "$password" "$user_type"
            return $?
        fi
    elif [ $reg_result -eq 2 ]; then
        # User already exists, try to login directly
        echo -e "${YELLOW}User exists, attempting login...${NC}"
        test_login "$email" "$password" "$user_type"
        return $?
    else
        # Registration failed
        return 1
    fi
    
    return 1
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
    
    # Complete authentication flow with Seller user (required for listing operations)
    run_complete_auth_flow "$TEST_USER_EMAIL" "$TEST_USER_PASSWORD" "$TEST_USER_ROLE" "Seller"
    
    if [ $? -eq 0 ]; then
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
        sleep 3  # Wait a bit for the job to process
        test_check_document_status
        
        # Cleanup
        test_delete_listing
    else
        echo -e "${RED}Authentication flow failed. Skipping remaining tests.${NC}"
    fi
    
    echo -e "\n${GREEN}=========================================${NC}"
    echo -e "${GREEN}TEST SUITE COMPLETED${NC}"
    echo -e "${GREEN}=========================================${NC}"
}

# Function to run specific test groups
function run_auth_tests() {
    echo -e "\n${GREEN}======== AUTHENTICATION TESTS ========${NC}"
    test_service_availability "$AUTH_SERVICE_URL" "Auth Service"
    run_complete_auth_flow "$TEST_USER_EMAIL" "$TEST_USER_PASSWORD" "$TEST_USER_ROLE" "Seller"
}

function run_listing_tests() {
    echo -e "\n${GREEN}======== LISTING TESTS ========${NC}"
    test_service_availability "$LISTING_SERVICE_URL" "Listing Service"
    run_complete_auth_flow "$TEST_USER_EMAIL" "$TEST_USER_PASSWORD" "$TEST_USER_ROLE" "Seller"
    if [ $? -eq 0 ]; then
        test_list_categories
        test_create_listing
        test_get_listing
        test_update_listing
        test_delete_listing
    fi
}

function run_search_tests() {
    echo -e "\n${GREEN}======== SEARCH TESTS ========${NC}"
    test_service_availability "$SEARCH_SERVICE_URL" "Search Service"
    run_complete_auth_flow "$TEST_USER_EMAIL" "$TEST_USER_PASSWORD" "$TEST_USER_ROLE" "Seller"
    if [ $? -eq 0 ]; then
        test_list_categories
        test_create_listing
        test_basic_search
        test_advanced_search
        test_delete_listing
    fi
}

function run_document_tests() {
    echo -e "\n${GREEN}======== DOCUMENT PROCESSOR TESTS ========${NC}"
    test_service_availability "$DOCUMENT_PROCESSOR_URL" "Document Processor"
    run_complete_auth_flow "$TEST_USER_EMAIL" "$TEST_USER_PASSWORD" "$TEST_USER_ROLE" "Seller"
    if [ $? -eq 0 ]; then
        test_list_categories
        test_create_listing
        test_generate_document
        sleep 3  # Wait a bit for the job to process
        test_check_document_status
        test_delete_listing
    fi
}

# Function to display configuration
function display_configuration() {
    echo -e "\n${BLUE}=========================================${NC}"
    echo -e "${BLUE}CONFIGURATION${NC}"
    echo -e "${BLUE}=========================================${NC}"
    echo -e "Test Group: ${YELLOW}${TEST_GROUP:-all}${NC}"
    echo -e "Setup Docker: ${YELLOW}$SETUP_DOCKER${NC}"
    echo -e "Clean Volumes: ${YELLOW}$CLEAN_VOLUMES${NC}"
    echo -e "Clean Databases Only: ${YELLOW}$CLEAN_DATABASES_ONLY${NC}"
    echo -e "Unique Email: ${YELLOW}$USE_UNIQUE_EMAIL${NC}"
    echo -e "Skip Registration: ${YELLOW}$SKIP_REGISTRATION${NC}"
    echo -e "Test User Email: ${YELLOW}$TEST_USER_EMAIL${NC}"
    if [ "$USE_UNIQUE_EMAIL" = true ]; then
        echo -e "${YELLOW}Note: Actual email will be generated with timestamp${NC}"
    fi
    echo -e "${BLUE}=========================================${NC}"
}

# Function to display usage
function display_usage() {
    echo -e "${BLUE}TheMarketPlace Microservices API Testing Script${NC}"
    echo ""
    echo "Usage: $0 [OPTIONS] [TEST_GROUP]"
    echo ""
    echo "Test Groups:"
    echo "  all       Run all tests (default)"
    echo "  auth      Run authentication tests only"
    echo "  listing   Run listing service tests only"
    echo "  search    Run search service tests only"
    echo "  document  Run document processor tests only"
    echo ""
    echo "Options:"
    echo "  --setup-docker        Start Docker containers before testing"
    echo "  --teardown-docker     Stop Docker containers after testing"
    echo "  --clean-volumes       Remove Docker volumes (use with --setup-docker or --teardown-docker)"
    echo "  --clean-databases     Clean databases only (faster than full volume cleanup)"
    echo "  --unique-email        Generate unique email addresses for testing"
    echo "  --skip-registration   Skip user registration and attempt direct login"
    echo "  --help               Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Run all tests with existing containers"
    echo "  $0 --setup-docker                    # Start containers and run all tests"
    echo "  $0 --setup-docker auth               # Start containers and run auth tests only"
    echo "  $0 --clean-volumes --setup-docker    # Fresh start with clean volumes"
    echo "  $0 --clean-databases all             # Clean databases and run tests"
    echo "  $0 --unique-email                    # Use unique email addresses"
    echo "  $0 --skip-registration               # Skip registration, try direct login"
    echo "  $0 --teardown-docker                 # Stop containers after tests"
    echo ""
    echo "Requirements:"
    echo "  - Docker with Compose v2 (docker compose command)"
    echo "  - curl command"
    echo "  - Optional: jq for better JSON parsing"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --setup-docker)
            SETUP_DOCKER=true
            shift
            ;;
        --teardown-docker)
            TEARDOWN_DOCKER=true
            shift
            ;;
        --clean-volumes)
            CLEAN_VOLUMES=true
            shift
            ;;
        --clean-databases)
            CLEAN_DATABASES_ONLY=true
            shift
            ;;
        --unique-email)
            USE_UNIQUE_EMAIL=true
            shift
            ;;
        --skip-registration)
            SKIP_REGISTRATION=true
            shift
            ;;
        --help)
            display_usage
            exit 0
            ;;
        auth|listing|search|document|all)
            TEST_GROUP=$1
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            display_usage
            exit 1
            ;;
    esac
done

# Default test group if none specified
if [ -z "$TEST_GROUP" ]; then
    TEST_GROUP="all"
fi

# Handle --clean-databases option (can be standalone or combined)
if [ "$CLEAN_DATABASES_ONLY" = true ]; then
    clean_databases
    if [ $? -ne 0 ]; then
        echo -e "${RED}Database cleaning failed. Exiting.${NC}"
        exit 1
    fi
    
    # If this was the only action requested, exit
    if [ "$SETUP_DOCKER" = false ] && [ "$TEST_GROUP" = "all" ] && [ $# -eq 0 ]; then
        echo -e "${GREEN}✓ Database cleaning completed successfully${NC}"
        exit 0
    fi
fi

# Display configuration
display_configuration

# Setup Docker environment if requested
setup_docker_environment

# Trap to ensure cleanup on script exit
trap 'teardown_docker_environment' EXIT

# Run the appropriate test group
case "$TEST_GROUP" in
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
    "all"|*)
        run_all_tests
        ;;
esac

# Manual teardown if not using trap
if [ "$TEARDOWN_DOCKER" = true ]; then
    teardown_docker_environment
fi

echo -e "\n${GREEN}🎉 Testing completed! Check the results above.${NC}"