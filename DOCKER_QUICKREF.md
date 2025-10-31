# Docker Deployment - Quick Reference

## Files Created

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage build with .NET 8 + Node.js + igc-xc-score |
| `docker-compose.yml` | Production deployment with Docker volumes |
| `docker-compose.dev.yml` | Development deployment with local folders |
| `.dockerignore` | Excludes unnecessary files from Docker build |
| `appsettings.Production.json` | Configuration with Docker-friendly paths |
| `DOCKER_README.md` | Comprehensive deployment guide |
| `migrate-to-docker.sh` | Linux/Mac migration script from VPS |
| `migrate-to-docker.bat` | Windows migration script |
| `.env.example` | Environment variables template |
| `quick-start.sh` | Interactive setup script |

## Quick Start Commands

### Development (Local Machine)
```bash
# Linux/Mac
chmod +x quick-start.sh
./quick-start.sh

# Windows
docker-compose -f docker-compose.dev.yml up -d
```

### Production (VPS)
```bash
# Build and start
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

### Migrate Existing Data
```bash
# Linux/Mac - Migrate from /home/webapp/ParagliderFlightLogDb
chmod +x migrate-to-docker.sh
./migrate-to-docker.sh /home/webapp/ParagliderFlightLogDb

# Windows
migrate-to-docker.bat C:\path\to\data
```

## Data Storage

### Development Mode
```
./dev-data/
??? db/          # SQLite databases
??? photos/      # Flight photos
??? score/       # Temp scoring files
??? logs/        # Application logs
```

### Production Mode
- Docker volumes: `flightlog-data`, `flightlog-photos`, `flightlog-score`, `flightlog-logs`
- Location: `/var/lib/docker/volumes/` on Linux
- Managed by Docker, persistent across container restarts

## Port Mapping
- HTTP: http://localhost:5000 ? container:8080
- HTTPS: http://localhost:5001 ? container:8081

## What's Included in the Image

? .NET 8 Runtime  
? Node.js 20 LTS  
? igc-xc-score (npm package)  
? SQLite support  
? All required dependencies  

## Volume Mount Points

| Container Path | Purpose | Volume Name (Production) | Dev Path |
|----------------|---------|-------------------------|----------|
| `/app/data/db` | SQLite databases | `flightlog-data` | `./dev-data/db` |
| `/app/data/photos` | Flight photos | `flightlog-photos` | `./dev-data/photos` |
| `/app/data/score` | Score temp files | `flightlog-score` | `./dev-data/score` |
| `/var/log/ParagliderFlightLog` | Application logs | `flightlog-logs` | `./dev-data/logs` |

## Backup & Restore

### Backup
```bash
# Backup all data
docker run --rm \
  -v flightlog-data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/db-backup-$(date +%Y%m%d).tar.gz -C /data .
```

### Restore
```bash
# Restore data
docker run --rm \
  -v flightlog-data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar xzf /backup/db-backup-YYYYMMDD.tar.gz -C /data
```

## Upgrading

```bash
# 1. Stop container
docker-compose down

# 2. Pull latest code
git pull

# 3. Rebuild image
docker-compose build

# 4. Start with new version
docker-compose up -d

# Data volumes are preserved automatically
```

## Troubleshooting

### View Logs
```bash
docker-compose logs -f
```

### Check Container Status
```bash
docker-compose ps
```

### Access Container Shell
```bash
docker exec -it paraglider-flightlog bash
```

### Verify Node.js & igc-xc-score
```bash
docker exec paraglider-flightlog node --version
docker exec paraglider-flightlog npm list -g igc-xc-score
```

### Reset Everything (?? Deletes all data!)
```bash
docker-compose down -v
```

## Configuration Paths in Container

| Setting | Path in Container |
|---------|-------------------|
| Database files | `/app/data/db/*.db` |
| User directories | `/app/data/db/{UserId}/` |
| Flight photos | `/app/data/db/{UserId}/FlightPhotos/` |
| Score temp files | `/app/data/score/` |
| Application logs | `/var/log/ParagliderFlightLog/log.txt` |
| Node.js | `/usr/bin/node` |
| igc-xc-score | `/usr/lib/node_modules/igc-xc-score/dist/igc-xc-score.cjs` |

## Next Steps

1. ? Files created - Docker setup complete
2. ?? Read `DOCKER_README.md` for detailed instructions
3. ?? Run `./quick-start.sh` to deploy
4. ?? (Optional) Run migration script if you have existing data
5. ?? Access application at http://localhost:5000

For detailed documentation, see **DOCKER_README.md**
