#!/bin/bash

# TheMarketPlace Microservices API Testing Script
# This script tests the various services in the TheMarketPlace microservices architecture
# Aligned with postman collection and documentation

# Source the functions file
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/api-tests-functions.sh"

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