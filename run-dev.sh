#!/bin/bash

# TheMarketPlace Services Runner Script
# Enhanced with Docker management, service control, and comprehensive help

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

# Function to display colored output
function log_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

function log_success() {
    echo -e "${GREEN}✓${NC} $1"
}

function log_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

function log_error() {
    echo -e "${RED}✗${NC} $1"
}

# Function to check if Docker is running and supports compose v2
function check_docker() {
    if ! docker info > /dev/null 2>&1; then
        log_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    
    # Check if Docker Compose v2 is available
    if ! docker compose version > /dev/null 2>&1; then
        log_error "Docker Compose v2 is not available. Please update Docker Desktop or install Docker Compose v2."
        log_warning "Note: This script requires 'docker compose' (v2) not 'docker-compose' (v1)"
        exit 1
    fi
    
    log_success "Docker and Docker Compose v2 are available"
}

# Function to setup Docker infrastructure
function setup_infrastructure() {
    if [ "$SKIP_INFRASTRUCTURE" = true ]; then
        log_info "Skipping infrastructure setup..."
        return 0
    fi

    log_info "Starting infrastructure containers (PostgreSQL, OpenSearch, RabbitMQ, MongoDB, MinIO)..."
    
    check_docker
    
    # Start infrastructure services
    docker compose up -d postgres opensearch opensearch-dashboards rabbitmq mongodb minio
    
    if [ $? -eq 0 ]; then
        log_success "Infrastructure containers started successfully"
        log_info "Waiting for infrastructure containers to be ready..."
        sleep 15
        
        # Check if key services are responding
        local max_attempts=10
        local attempt=0
        
        while [ $attempt -lt $max_attempts ]; do
            if curl -s http://localhost:5432 > /dev/null 2>&1 || \
               docker compose ps postgres | grep -q "healthy\|running"; then
                log_success "Infrastructure services are ready!"
                break
            fi
            
            attempt=$((attempt + 1))
            log_info "Waiting for infrastructure services... (attempt $attempt/$max_attempts)"
            sleep 3
        done
        
        if [ $attempt -eq $max_attempts ]; then
            log_warning "Infrastructure services may still be starting up"
        fi
    else
        log_error "Failed to start infrastructure containers"
        exit 1
    fi
}

# Function to setup complete Docker environment
function setup_docker_environment() {
    if [ "$SETUP_DOCKER" = true ]; then
        log_info "Setting up complete Docker environment..."
        
        check_docker
        
        log_info "Checking existing containers..."
        if docker compose ps --status running | grep -q "running"; then
            log_warning "Some containers are already running. Stopping them first..."
            docker compose down
        fi
        
        if [ "$CLEAN_VOLUMES" = true ]; then
            log_warning "Cleaning volumes for fresh start..."
            docker compose down --volumes --remove-orphans
            docker system prune -f --volumes
        fi
        
        log_info "Starting all services with Docker Compose..."
        docker compose up --detach --build
        
        if [ $? -eq 0 ]; then
            log_success "All services started successfully"
            display_service_urls
        else
            log_error "Failed to start services"
            exit 1
        fi
    fi
}

# Function to teardown Docker environment
function teardown_docker_environment() {
    if [ "$TEARDOWN_DOCKER" = true ]; then
        log_info "Tearing down Docker environment..."
        
        docker compose down
        
        if [ "$CLEAN_VOLUMES" = true ]; then
            log_warning "Removing volumes and orphaned containers..."
            docker compose down --volumes --remove-orphans
        fi
        
        log_success "Docker environment cleaned up"
    fi
}

# Function to display service URLs and information
function display_service_urls() {
    echo -e "\n${BLUE}=========================================${NC}"
    echo -e "${BLUE}SERVICE INFORMATION${NC}"
    echo -e "${BLUE}=========================================${NC}"
    echo -e "${GREEN}Service URLs:${NC}"
    echo -e "  Authentication: ${YELLOW}http://localhost:5000/swagger${NC}"
    echo -e "  Listing:        ${YELLOW}http://localhost:5001/swagger${NC}"
    echo -e "  Search:         ${YELLOW}http://localhost:5002/swagger${NC}"
    echo -e "  Document Proc:  ${YELLOW}http://localhost:5003/swagger${NC}"
    echo ""
    echo -e "${GREEN}Health Checks:${NC}"
    echo -e "  Authentication: ${YELLOW}http://localhost:5000/health${NC}"
    echo -e "  Listing:        ${YELLOW}http://localhost:5001/health${NC}"
    echo -e "  Search:         ${YELLOW}http://localhost:5002/health${NC}"
    echo -e "  Document Proc:  ${YELLOW}http://localhost:5003/health${NC}"
    echo ""
    echo -e "${GREEN}Management Dashboards:${NC}"
    echo -e "  Hangfire:       ${YELLOW}http://localhost:5003/hangfire${NC}"
    echo -e "  RabbitMQ:       ${YELLOW}http://localhost:15672${NC} (guest/guest)"
    echo -e "  OpenSearch:     ${YELLOW}http://localhost:5601${NC}"
    echo -e "  MinIO Console:  ${YELLOW}http://localhost:9001${NC} (minioadmin/minioadmin)"
    echo ""
    echo -e "${GREEN}Authentication Flow:${NC}"
    echo -e "  1. POST /api/register (with role: 0=Customer, 1=Seller, 2=Admin)"
    echo -e "  2. GET /api/confirm-email?email={email}&token={token} ${RED}(REQUIRED)${NC}"
    echo -e "  3. POST /api/login"
    echo -e "  4. Use Bearer token for authenticated requests"
    echo -e "${BLUE}=========================================${NC}"
}

# Make the script directory the working directory
cd "$(dirname "$0")"

# Function to run a service in development mode
function run_service() {
    local service_name=$1
    local service_path=$2
    local port=$3
    
    log_info "Starting $service_name on http://localhost:$port..."
    echo -e "  Health check: ${YELLOW}http://localhost:$port/health${NC}"
    echo -e "  Swagger UI:   ${YELLOW}http://localhost:$port/swagger${NC}"
    echo ""
    
    # Check if the service path exists
    if [ ! -d "$service_path" ]; then
        log_error "Service path '$service_path' does not exist"
        exit 1
    fi
    
    # Check if .NET is available
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET CLI is not available. Please install .NET SDK"
        exit 1
    fi
    
    cd "$service_path"
    log_info "Running: dotnet run --urls=http://localhost:$port"
    dotnet run --urls=http://localhost:$port
}

# Function to check service health
function check_service_health() {
    local service_name=$1
    local port=$2
    
    log_info "Checking $service_name health at http://localhost:$port/health..."
    
    local max_attempts=10
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if curl -s http://localhost:$port/health > /dev/null 2>&1; then
            log_success "$service_name is healthy!"
            return 0
        fi
        
        attempt=$((attempt + 1))
        log_info "Waiting for $service_name... (attempt $attempt/$max_attempts)"
        sleep 2
    done
    
    log_warning "$service_name may not be ready yet"
    return 1
}

# Function to display usage
function display_usage() {
    echo -e "${BLUE}TheMarketPlace Services Runner Script${NC}"
    echo ""
    echo "Usage: $0 [OPTIONS] [SERVICE]"
    echo ""
    echo "Services:"
    echo "  auth       Run the Authentication Service (port 5000)"
    echo "  listing    Run the Listing Service (port 5001)"
    echo "  search     Run the Search Service (port 5002)"
    echo "  docs       Run the Document Processor Service (port 5003)"
    echo "  all        Run all services with Docker Compose"
    echo "  status     Check status of all services"
    echo ""
    echo "Docker Management Options:"
    echo "  --setup-docker          Start all services with Docker Compose"
    echo "  --teardown-docker       Stop all Docker containers"
    echo "  --clean-volumes         Remove Docker volumes (use with --setup-docker or --teardown-docker)"
    echo "  --skip-infrastructure   Skip starting infrastructure containers"
    echo "  --health-check         Check health of all running services"
    echo "  --help                 Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 auth                              # Run auth service in development mode"
    echo "  $0 --setup-docker                   # Start all services with Docker"
    echo "  $0 --setup-docker --clean-volumes   # Fresh start with clean volumes"
    echo "  $0 --teardown-docker                # Stop all services"
    echo "  $0 --teardown-docker --clean-volumes # Stop and remove volumes"
    echo "  $0 status                           # Check service status"
    echo "  $0 --health-check                   # Check service health"
    echo ""
    echo "Development Workflow:"
    echo "  1. $0 --setup-docker                # Start infrastructure and services"
    echo "  2. Develop and test your changes"
    echo "  3. $0 --teardown-docker             # Clean shutdown"
    echo ""
    echo "Requirements:"
    echo "  - Docker with Compose v2 (for --setup-docker)"
    echo "  - .NET SDK (for individual service development)"
    echo "  - curl (for health checks)"
}

# Function to check service status
function check_all_services_status() {
    echo -e "\n${BLUE}=========================================${NC}"
    echo -e "${BLUE}SERVICE STATUS CHECK${NC}"
    echo -e "${BLUE}=========================================${NC}"
    
    local services=("auth:5000" "listing:5001" "search:5002" "docs:5003")
    
    for service_info in "${services[@]}"; do
        IFS=':' read -r service port <<< "$service_info"
        
        if curl -s http://localhost:$port/health > /dev/null 2>&1; then
            log_success "$service service is running (port $port)"
        else
            log_error "$service service is not responding (port $port)"
        fi
    done
    
    echo ""
    log_info "Docker container status:"
    docker compose ps
}

# Function to perform health checks
function perform_health_checks() {
    echo -e "\n${BLUE}=========================================${NC}"
    echo -e "${BLUE}HEALTH CHECK RESULTS${NC}"
    echo -e "${BLUE}=========================================${NC}"
    
    check_service_health "Authentication Service" 5000
    check_service_health "Listing Service" 5001
    check_service_health "Search Service" 5002
    check_service_health "Document Processor" 5003
    
    echo ""
    log_info "Infrastructure health:"
    if docker compose ps postgres | grep -q "healthy\|running"; then
        log_success "PostgreSQL is running"
    else
        log_error "PostgreSQL is not running"
    fi
    
    if docker compose ps rabbitmq | grep -q "healthy\|running"; then
        log_success "RabbitMQ is running"
    else
        log_error "RabbitMQ is not running"
    fi
    
    if docker compose ps opensearch | grep -q "healthy\|running"; then
        log_success "OpenSearch is running"
    else
        log_error "OpenSearch is not running"
    fi
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