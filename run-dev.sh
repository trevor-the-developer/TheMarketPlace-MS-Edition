#!/bin/bash

# Start infrastructure containers
echo "Starting infrastructure containers (PostgreSQL, OpenSearch, RabbitMQ, MongoDB, MinIO)..."
docker-compose up -d postgres opensearch opensearch-dashboards rabbitmq mongodb minio

# Wait for the containers to be ready
echo "Waiting for infrastructure containers to be ready..."
sleep 10

# Make the script directory the working directory
cd "$(dirname "$0")"

# Function to run a service
run_service() {
  local service_name=$1
  local service_path=$2
  local port=$3
  
  echo "Starting $service_name on http://localhost:$port..."
  cd "$service_path"
  dotnet run --urls=http://localhost:$port
}

# Check which service to run
if [ "$1" == "auth" ]; then
  run_service "AuthenticationService" "src/Services/AuthenticationService/API/AuthenticationService.Api" 5000
elif [ "$1" == "listing" ]; then
  run_service "ListingService" "src/Services/ListingService/API/ListingService.Api" 5001
elif [ "$1" == "search" ]; then
  run_service "SearchService" "src/Services/SearchService/API/SearchService.Api" 5002
elif [ "$1" == "docs" ]; then
  run_service "DocumentProcessor" "src/Functions/DocumentProcessor" 5003
elif [ "$1" == "all" ]; then
  echo "Starting all services with Docker Compose..."
  docker-compose up --build
else
  echo "Usage: $0 [auth|listing|search|docs|all]"
  echo "  auth    - Run the Authentication Service"
  echo "  listing - Run the Listing Service"
  echo "  search  - Run the Search Service"
  echo "  docs    - Run the Document Processor Service"
  echo "  all     - Run all services with Docker Compose"
  exit 1
fi