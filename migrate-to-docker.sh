#!/bin/bash

# Migration script for moving data from VPS to Docker volumes
# Usage: ./migrate-to-docker.sh [source_path]

set -e

# Default source path (can be overridden by command line argument)
SOURCE_PATH="${1:-/home/webapp/ParagliderFlightLogDb}"

echo "================================================"
echo "Paraglider Flight Log - Docker Migration Script"
echo "================================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "? Error: Docker is not installed or not in PATH"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "? Error: Docker Compose is not installed"
    exit 1
fi

# Check if source directory exists
if [ ! -d "$SOURCE_PATH" ]; then
    echo "? Error: Source directory $SOURCE_PATH does not exist"
    echo "Usage: ./migrate-to-docker.sh [source_path]"
    exit 1
fi

echo "Source directory: $SOURCE_PATH"
echo ""

# Ask for confirmation
read -p "This will migrate data from $SOURCE_PATH to Docker volumes. Continue? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Migration cancelled."
    exit 0
fi

echo ""
echo "Step 1: Creating Docker volumes..."
docker volume create flightlog-data
docker volume create flightlog-score
docker volume create flightlog-logs

echo "? Volumes created"
echo ""

echo "Step 2: Copying database files and photos..."
docker run --rm \
    -v flightlog-data:/data \
    -v "$SOURCE_PATH":/source:ro \
    alpine sh -c "cp -rv /source/*.db /data/ 2>/dev/null || echo 'No .db files in root'; \
                  if [ -d /source ]; then \
                      find /source -name '*.db' -type f -exec sh -c 'mkdir -p /data/\$(dirname \${1#/source/}) && cp -v \$1 /data/\${1#/source/}' _ {} \;; \
                      find /source -type d -name 'FlightPhotos' -exec sh -c 'mkdir -p /data/\$(dirname \${1#/source/}) && cp -rv \$1 /data/\$(dirname \${1#/source/})/' _ {} \;; \
                  fi"

echo "? Database files and photos copied"
echo ""

echo "Step 3: Setting correct permissions..."
docker run --rm -v flightlog-data:/data alpine chown -R 1000:1000 /data
docker run --rm -v flightlog-score:/score alpine chown -R 1000:1000 /score
docker run --rm -v flightlog-logs:/logs alpine chown -R 1000:1000 /logs
echo "? Permissions set"
echo ""

echo "Step 4: Verifying migration..."
echo ""
echo "Database volume contents:"
docker run --rm -v flightlog-data:/data alpine ls -lha /data

echo ""
echo "================================================"
echo "? Migration completed successfully!"
echo "================================================"
echo ""
echo "Next steps:"
echo "1. Review the copied files above"
echo "2. Start the application: docker-compose up -d"
echo "3. Check logs: docker-compose logs -f"
echo "4. Access the application at http://localhost:5000"
echo ""
echo "Optional: Create a backup of the Docker volumes using:"
echo "  docker run --rm -v flightlog-data:/data -v \$(pwd):/backup alpine tar czf /backup/pre-migration-backup.tar.gz -C /data ."
echo ""
