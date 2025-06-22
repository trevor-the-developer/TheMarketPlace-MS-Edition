#!/bin/bash

# TheMarketPlace Services Functions
# Enhanced with improved health checks and Docker management

# Color definitions for better readability
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

# Function to check OpenSearch health specifically
function check_opensearch_health() {
    local max_attempts=3
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        # Try OpenSearch health API endpoint
        if curl -s -f "http://localhost:9200/_cluster/health" > /dev/null 2>&1; then
            local health_status=$(curl -s "http://localhost:9200/_cluster/health" | grep -o '"status":"[^"]*"' | cut -d'"' -f4)
            if [[ "$health_status" == "green" ]] || [[ "$health_status" == "yellow" ]]; then
                return 0
            fi
        fi
        
        # Fallback: check if container is running and port is accessible
        if docker compose ps opensearch | grep -q "Up" && \
           timeout 5 bash -c "</dev/tcp/localhost/9200" 2>/dev/null; then
            return 0
        fi
        
        attempt=$((attempt + 1))
        sleep 1
    done
    
    return 1
}

# Function to check PostgreSQL health
function check_postgres_health() {
    # Check if container is healthy or running
    if docker compose ps postgres | grep -q "healthy"; then
        return 0
    elif docker compose ps postgres | grep -q "Up" && \
         timeout 5 bash -c "</dev/tcp/localhost/5432" 2>/dev/null; then
        return 0
    fi
    return 1
}

# Function to check RabbitMQ health
function check_rabbitmq_health() {
    # Check if container is healthy
    if docker compose ps rabbitmq | grep -q "healthy"; then
        return 0
    elif docker compose ps rabbitmq | grep -q "Up" && \
         timeout 5 bash -c "</dev/tcp/localhost/5672" 2>/dev/null; then
        return 0
    fi
    return 1
}

# Function to check MongoDB health
function check_mongodb_health() {
    if docker compose ps mongodb | grep -q "Up" && \
       timeout 5 bash -c "</dev/tcp/localhost/27017" 2>/dev/null; then
        return 0
    fi
    return 1
}

# Function to check MinIO health
function check_minio_health() {
    if docker compose ps minio | grep -q "Up" && \
       timeout 5 bash -c "</dev/tcp/localhost/9000" 2>/dev/null; then
        return 0
    fi
    return 1
}

# Enhanced service health check function
function check_service_health() {
    local service_name=$1
    local port=$2
    
    log_info "Checking $service_name health at http://localhost:$port/health..."
    
    local max_attempts=10
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if curl -s -f "http://localhost:$port/health" > /dev/null 2>&1; then
            log_success "$service_name is healthy!"
            return 0
        fi
        
        attempt=$((attempt + 1))
        if [ $attempt -lt $max_attempts ]; then
            log_info "Waiting for $service_name... (attempt $attempt/$max_attempts)"
            sleep 2
        fi
    done
    
    log_warning "$service_name may not be ready yet"
    return 1
}

# Enhanced infrastructure health check
function perform_health_checks() {
    echo -e "\n${BLUE}=========================================${NC}"
    echo -e "${BLUE}HEALTH CHECK RESULTS${NC}"
    echo -e "${BLUE}=========================================${NC}"
    
    # Check application services
    check_service_health "Authentication Service" 5000
    check_service_health "Listing Service" 5001
    check_service_health "Search Service" 5002
    check_service_health "Document Processor" 5003
    
    echo ""
    log_info "Infrastructure health:"
    
    # PostgreSQL
    if check_postgres_health; then
        log_success "PostgreSQL is running and healthy"
    else
        log_error "PostgreSQL is not running or not healthy"
    fi
    
    # RabbitMQ
    if check_rabbitmq_health; then
        log_success "RabbitMQ is running and healthy"
    else
        log_error "RabbitMQ is not running or not healthy"
    fi
    
    # OpenSearch (improved check)
    if check_opensearch_health; then
        log_success "OpenSearch is running and healthy"
    else
        log_error "OpenSearch is not running or not healthy"
    fi
    
    # MongoDB
    if check_mongodb_health; then
        log_success "MongoDB is running"
    else
        log_warning "MongoDB is not running"
    fi
    
    # MinIO
    if check_minio_health; then
        log_success "MinIO is running"
    else
        log_warning "MinIO is not running"
    fi
    
    echo ""
    log_info "Detailed container status:"
    docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
}

# Function to wait for OpenSearch to be ready
function wait_for_opensearch() {
    log_info "Waiting for OpenSearch to be ready..."
    local max_attempts=30
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if check_opensearch_health; then
            log_success "OpenSearch is ready!"
            
            # Also check cluster status
            local cluster_info=$(curl -s "http://localhost:9200/_cluster/health" 2>/dev/null)
            if [ $? -eq 0 ]; then
                local status=$(echo "$cluster_info" | grep -o '"status":"[^"]*"' | cut -d'"' -f4)
                local nodes=$(echo "$cluster_info" | grep -o '"number_of_nodes":[0-9]*' | cut -d':' -f2)
                log_info "OpenSearch cluster status: $status, nodes: $nodes"
            fi
            return 0
        fi
        
        attempt=$((attempt + 1))
        log_info "Waiting for OpenSearch... (attempt $attempt/$max_attempts)"
        sleep 3
    done
    
    log_error "OpenSearch failed to become ready within expected time"
    
    # Show OpenSearch logs for debugging
    log_info "OpenSearch container logs (last 20 lines):"
    docker compose logs --tail=20 opensearch
    
    return 1
}

# Function to check all services status with detailed output
function check_all_services_status() {
    echo -e "\n${BLUE}=========================================${NC}"
    echo -e "${BLUE}SERVICE STATUS CHECK${NC}"
    echo -e "${BLUE}=========================================${NC}"
    
    local services=("auth:5000" "listing:5001" "search:5002" "docs:5003")
    
    echo -e "${GREEN}Application Services:${NC}"
    for service_info in "${services[@]}"; do
        IFS=':' read -r service port <<< "$service_info"
        
        if curl -s -f "http://localhost:$port/health" > /dev/null 2>&1; then
            log_success "$service service is running (port $port)"
        else
            log_error "$service service is not responding (port $port)"
        fi
    done
    
    echo ""
    echo -e "${GREEN}Infrastructure Services:${NC}"
    
    if check_postgres_health; then
        log_success "PostgreSQL is healthy"
    else
        log_error "PostgreSQL is not healthy"
    fi
    
    if check_rabbitmq_health; then
        log_success "RabbitMQ is healthy" 
    else
        log_error "RabbitMQ is not healthy"
    fi
    
    if check_opensearch_health; then
        log_success "OpenSearch is healthy"
    else
        log_error "OpenSearch is not healthy"
    fi
    
    if check_mongodb_health; then
        log_success "MongoDB is running"
    else
        log_warning "MongoDB is not running"
    fi
    
    if check_minio_health; then
        log_success "MinIO is running"
    else
        log_warning "MinIO is not running"
    fi
    
    echo ""
    log_info "Docker container status:"
    docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
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
            if check_postgres_health && check_rabbitmq_health; then
                log_success "Core infrastructure services are ready!"
                
                # Wait for OpenSearch separately as it takes longer
                wait_for_opensearch
                break
            fi
            
            attempt=$((attempt + 1))
            log_info "Waiting for infrastructure services... (attempt $attempt/$max_attempts)"
            sleep 3
        done
        
        if [ $attempt -eq $max_attempts ]; then
            log_warning "Some infrastructure services may still be starting up"
        fi
    else
        log_error "Failed to start infrastructure containers"
        exit 1
    fi
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