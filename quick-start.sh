#!/bin/bash

# Quick start script for Paraglider Flight Log Docker deployment
# This script helps you get started quickly

set -e

echo "================================================"
echo "Paraglider Flight Log - Quick Start"
echo "================================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "? Error: Docker is not installed"
    echo "Please install Docker from: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "? Error: Docker Compose is not installed"
    echo "Please install Docker Compose from: https://docs.docker.com/compose/install/"
    exit 1
fi

echo "? Docker is installed"
echo ""

# Ask user which environment
echo "Select deployment environment:"
echo "1) Development (stores data in ./dev-data/ folder)"
echo "2) Production (stores data in Docker volumes)"
echo ""
read -p "Enter choice [1-2]: " choice

case $choice in
    1)
        COMPOSE_FILE="docker-compose.dev.yml"
        ENV_TYPE="Development"
        echo ""
        echo "Selected: Development environment"
        echo "Data will be stored in: ./dev-data/"
        echo ""
        
        # Create dev-data directories if they don't exist
        mkdir -p ./dev-data/db
        mkdir -p ./dev-data/photos
        mkdir -p ./dev-data/score
        mkdir -p ./dev-data/logs
        echo "? Created dev-data directories"
        ;;
    2)
        COMPOSE_FILE="docker-compose.yml"
        ENV_TYPE="Production"
        echo ""
        echo "Selected: Production environment"
        echo "Data will be stored in Docker volumes"
        echo ""
        ;;
    *)
        echo "Invalid choice. Exiting."
        exit 1
        ;;
esac

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        echo "Creating .env file from .env.example..."
        cp .env.example .env
        echo "? .env file created"
        echo ""
    fi
fi

echo "Building Docker image..."
echo "This may take several minutes on first run..."
echo ""

docker-compose -f "$COMPOSE_FILE" build

echo ""
echo "? Build complete"
echo ""

echo "Starting container..."
docker-compose -f "$COMPOSE_FILE" up -d

echo ""
echo "? Container started"
echo ""

# Wait a moment for the container to start
sleep 3

# Check if container is running
if docker-compose -f "$COMPOSE_FILE" ps | grep -q "Up"; then
    echo "================================================"
    echo "? SUCCESS! Application is running"
    echo "================================================"
    echo ""
    echo "Access the application at: http://localhost:5000"
    echo ""
    echo "Useful commands:"
    echo "  View logs:           docker-compose -f $COMPOSE_FILE logs -f"
    echo "  Stop application:    docker-compose -f $COMPOSE_FILE down"
    echo "  Restart application: docker-compose -f $COMPOSE_FILE restart"
    echo "  Check status:        docker-compose -f $COMPOSE_FILE ps"
    echo ""
    
    if [ "$ENV_TYPE" = "Development" ]; then
        echo "Development data location: ./dev-data/"
    else
        echo "Production data is stored in Docker volumes"
        echo "  View volumes:        docker volume ls | grep flightlog"
        echo "  Backup data:         See DOCKER_README.md for backup instructions"
    fi
    echo ""
    
    read -p "Would you like to see the logs now? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose -f "$COMPOSE_FILE" logs -f
    fi
else
    echo "? Error: Container failed to start"
    echo ""
    echo "Checking logs..."
    docker-compose -f "$COMPOSE_FILE" logs --tail=50
    exit 1
fi
