# Docker Deployment Guide for Paraglider Flight Log

This guide explains how to build, deploy, and maintain the Paraglider Flight Log application using Docker containers.

## Overview

The Docker setup includes:
- **.NET 8 Runtime** - For running the Blazor web application
- **Node.js 20 LTS** - For running the igc-xc-score flight scoring engine
- **igc-xc-score** - Automatically installed npm package for flight scoring
- **Persistent Volumes** - For SQLite databases, flight photos, and logs

## Prerequisites

- Docker Engine 20.10 or later
- Docker Compose 2.0 or later (included with Docker Desktop)
- At least 2GB of free disk space

## Project Structure

```
ParagliderFlightLog/
??? Dockerfile                          # Main Dockerfile for building the image
??? docker-compose.yml                  # Production deployment configuration
??? docker-compose.dev.yml              # Development deployment configuration
??? .dockerignore                       # Files to exclude from Docker build
??? DOCKER_README.md                    # This file
??? ParaglidingFlightLogWeb/
    ??? ParaglidingFlightLogWeb/
        ??? appsettings.Production.json # Production configuration with Docker paths
```

## Quick Start

### Development Environment

For development with local data folders:

```bash
# Build and start the container
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop the container
docker-compose -f docker-compose.dev.yml down
```

The development setup creates a `./dev-data/` directory in your project root with subdirectories for databases, photos, scores, and logs.

Access the application at: **http://localhost:5000**

### Production Environment (VPS)

For production deployment with Docker volumes:

```bash
# Build and start the container
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the container
docker-compose down
```

Access the application at: **http://your-vps-ip:5000**

## Building the Docker Image

### Build Locally

```bash
# Build the image with a tag
docker build -t paraglider-flightlog:latest .

# Build with a specific version tag
docker build -t paraglider-flightlog:1.0.0 .
```

### Verify the Build

```bash
# Check that Node.js is installed
docker run --rm paraglider-flightlog:latest node --version

# Check that igc-xc-score is installed
docker run --rm paraglider-flightlog:latest npm list -g igc-xc-score

# Check that .NET is working
docker run --rm paraglider-flightlog:latest dotnet --version
```

## Data Persistence

### Development (docker-compose.dev.yml)

Data is stored in local directories on your machine:
- `./dev-data/db/` - SQLite database files
- `./dev-data/photos/` - Flight photos
- `./dev-data/score/` - Temporary scoring files
- `./dev-data/logs/` - Application logs

### Production (docker-compose.yml)

Data is stored in Docker named volumes:
- `flightlog-data` - SQLite database files
- `flightlog-photos` - Flight photos
- `flightlog-score` - Temporary scoring files
- `flightlog-logs` - Application logs

### Managing Production Volumes

```bash
# List volumes
docker volume ls

# Inspect a volume
docker volume inspect flightlog-data

# Backup a volume
docker run --rm -v flightlog-data:/data -v $(pwd):/backup alpine tar czf /backup/flightlog-data-backup.tar.gz -C /data .

# Restore a volume
docker run --rm -v flightlog-data:/data -v $(pwd):/backup alpine tar xzf /backup/flightlog-data-backup.tar.gz -C /data

# Remove all volumes (?? WARNING: This deletes all data!)
docker-compose down -v
```

## Migrating from VPS to Docker

If you have existing data on your VPS at `/home/webapp/ParagliderFlightLogDb`, follow these steps:

### Option 1: Using Docker Volumes (Recommended for Production)

```bash
# 1. Start the container once to create volumes
docker-compose up -d
docker-compose down

# 2. Copy your existing data to the Docker volume
docker run --rm -v flightlog-data:/data -v /home/webapp/ParagliderFlightLogDb:/source alpine cp -r /source/* /data/

# 3. Fix permissions
docker run --rm -v flightlog-data:/data alpine chown -R 1000:1000 /data

# 4. Start the application
docker-compose up -d
```

### Option 2: Direct Volume Mount (Alternative)

Edit `docker-compose.yml` and replace the volume configuration:

```yaml
volumes:
  - /home/webapp/ParagliderFlightLogDb:/app/data/db
  # ... other volumes
```

## Upgrading the Application

### Method 1: Rebuild and Replace (Zero Downtime)

```bash
# 1. Pull latest code
git pull origin main

# 2. Build new image with a new tag
docker build -t paraglider-flightlog:v2.0.0 .

# 3. Update docker-compose.yml to use new image
# Change: image: paraglider-flightlog:v2.0.0

# 4. Recreate container with new image
docker-compose up -d

# 5. Old data volumes are automatically reused
```

### Method 2: In-Place Upgrade

```bash
# 1. Stop the running container
docker-compose down

# 2. Pull latest code
git pull origin main

# 3. Rebuild the image
docker-compose build

# 4. Start with updated image
docker-compose up -d
```

### Verify Upgrade

```bash
# Check application logs
docker-compose logs -f

# Verify database integrity
docker exec -it paraglider-flightlog ls -la /app/data/db/
```

## Configuration

### Environment Variables

You can override settings using environment variables in `docker-compose.yml`:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:8080
  - ConnectionStrings__UserDataSqlite=Data Source=/app/data/db/UserDbParagliderFlightLog.db
```

### Custom Configuration

To use custom settings:

1. Create `appsettings.Custom.json` in `ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/`
2. Mount it in `docker-compose.yml`:

```yaml
volumes:
  - ./ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/appsettings.Custom.json:/app/appsettings.Custom.json:ro
```

3. Set environment: `ASPNETCORE_ENVIRONMENT=Custom`

## Networking

### Ports

- **5000** - HTTP access (mapped to container port 8080)
- **5001** - HTTPS access (mapped to container port 8081)

### Reverse Proxy Setup (Nginx Example)

```nginx
server {
    listen 80;
    server_name flightlog.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs

# Check if ports are already in use
netstat -tulpn | grep :5000

# Inspect container
docker inspect paraglider-flightlog
```

### Database Connection Issues

```bash
# Check database file permissions
docker exec -it paraglider-flightlog ls -la /app/data/db/

# Check appsettings
docker exec -it paraglider-flightlog cat /app/appsettings.Production.json
```

### igc-xc-score Not Working

```bash
# Verify Node.js installation
docker exec -it paraglider-flightlog node --version

# Verify igc-xc-score installation
docker exec -it paraglider-flightlog npm list -g igc-xc-score

# Test igc-xc-score manually
docker exec -it paraglider-flightlog node /usr/lib/node_modules/igc-xc-score/dist/igc-xc-score.cjs
```

### View Real-Time Logs

```bash
# All logs
docker-compose logs -f

# Specific service
docker-compose logs -f paraglider-flightlog

# Last 100 lines
docker-compose logs --tail=100 paraglider-flightlog
```

## Maintenance

### Regular Backups

Create a backup script `backup.sh`:

```bash
#!/bin/bash
BACKUP_DIR="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p $BACKUP_DIR

echo "Backing up databases..."
docker run --rm -v flightlog-data:/data -v $(pwd)/$BACKUP_DIR:/backup \
  alpine tar czf /backup/db-$DATE.tar.gz -C /data .

echo "Backing up photos..."
docker run --rm -v flightlog-photos:/data -v $(pwd)/$BACKUP_DIR:/backup \
  alpine tar czf /backup/photos-$DATE.tar.gz -C /data .

echo "Backup complete: $BACKUP_DIR"
```

Make it executable and run:

```bash
chmod +x backup.sh
./backup.sh
```

### Update igc-xc-score

```bash
# Access container
docker exec -it paraglider-flightlog bash

# Update npm package
npm update -g igc-xc-score

# Exit container
exit

# Restart to apply changes
docker-compose restart
```

### Clean Up Old Images

```bash
# Remove unused images
docker image prune -a

# Remove all stopped containers
docker container prune

# Remove unused volumes (?? BE CAREFUL!)
docker volume prune
```

## Performance Tuning

### Resource Limits

Add resource limits to `docker-compose.yml`:

```yaml
services:
  paraglider-flightlog:
    # ... other settings
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### Optimize for Production

```bash
# Build with optimizations
docker build --build-arg ASPNETCORE_ENVIRONMENT=Production -t paraglider-flightlog:latest .
```

## Security Considerations

1. **Run as non-root**: The Dockerfile creates and uses `appuser` (UID 1000)
2. **Keep images updated**: Regularly rebuild to get security patches
3. **Use HTTPS**: Configure a reverse proxy with SSL/TLS
4. **Restrict access**: Use firewall rules to limit access
5. **Regular backups**: Automate backups of volumes

## Monitoring

### Health Checks

Add health check to `docker-compose.yml`:

```yaml
services:
  paraglider-flightlog:
    # ... other settings
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Monitor Resource Usage

```bash
# Real-time stats
docker stats paraglider-flightlog

# Disk usage
docker system df
```

## Support

For issues related to:
- **Application**: Check the main README.md and GitHub issues
- **Docker setup**: Review this guide and Docker logs
- **igc-xc-score**: Visit https://github.com/mmomtchev/igc-xc-score

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [ASP.NET Core Docker Documentation](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [igc-xc-score Repository](https://github.com/mmomtchev/igc-xc-score)
