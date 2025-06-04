#!/bin/bash

# TheMarketPlace Services Runner Script
# Enhanced with Docker management, service control, and comprehensive help

# Import run-dev-functions.sh
source ./run-dev-functions.sh

# Color definitions for better readability
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Docker management flags
SETUP_DOCKER=false
TEARDOWN_DOCKER=false
CLEAN_VOLUMES=false
SKIP_INFRASTRUCTURE=false

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
        --skip-infrastructure)
            SKIP_INFRASTRUCTURE=true
            shift
            ;;
        --health-check)
            perform_health_checks
            exit 0
            ;;
        --help)
            display_usage
            exit 0
            ;;
        auth|listing|search|docs|all|status)
            SERVICE=$1
            shift
            ;;
        *)
            log_error "Unknown option: $1"
            display_usage
            exit 1
            ;;
    esac
done

# Handle Docker setup/teardown first
if [ "$TEARDOWN_DOCKER" = true ]; then
    teardown_docker_environment
    exit 0
fi

if [ "$SETUP_DOCKER" = true ]; then
    setup_docker_environment
    exit 0
fi

# Handle service-specific actions
case "$SERVICE" in
    "auth")
        setup_infrastructure
        run_service "AuthenticationService" "src/Services/AuthenticationService/API/AuthenticationService.Api" 5000
        ;;
    "listing")
        setup_infrastructure
        run_service "ListingService" "src/Services/ListingService/API/ListingService.Api" 5001
        ;;
    "search")
        setup_infrastructure
        run_service "SearchService" "src/Services/SearchService/API/SearchService.Api" 5002
        ;;
    "docs")
        setup_infrastructure
        run_service "DocumentProcessor" "src/Functions/DocumentProcessor" 5003
        ;;
    "all")
        log_info "Starting all services with Docker Compose..."
        check_docker
        docker compose up --build
        ;;
    "status")
        check_all_services_status
        ;;
    *)
        display_usage
        exit 1
        ;;
esac

# Make the script directory the working directory
cd "$(dirname "$0")"

