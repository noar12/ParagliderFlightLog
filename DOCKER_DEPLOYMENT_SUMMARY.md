# ?? Docker Deployment Summary

## ? What Has Been Created

Your Paraglider Flight Log application is now fully dockerized! Here's what was set up:

### ?? Core Docker Files

1. **Dockerfile** - Multi-stage build configuration
   - Base: .NET 8 Runtime
   - Includes: Node.js 20 LTS
   - Pre-installed: igc-xc-score npm package
   - Security: Runs as non-root user (UID 1000)

2. **docker-compose.yml** - Production deployment
   - Uses Docker named volumes for data persistence
   - Maps ports 5000 (HTTP) and 5001 (HTTPS)
   - Automatic restart policy
   - Isolated network

3. **docker-compose.dev.yml** - Development deployment
   - Uses local `./dev-data/` directory
   - Easier for development and debugging
   - Same port mapping as production

4. **.dockerignore** - Build optimization
   - Excludes build artifacts, git files, and test data
   - Reduces image size and build time

### ?? Configuration Files

5. **appsettings.Production.json** - Docker-optimized settings
   - Database paths: `/app/data/db/`
   - Photos directory: `/app/data/db/{UserId}/FlightPhotos/`
   - Score temp files: `/app/data/score/`
   - Logs: `/var/log/ParagliderFlightLog/`
   - Node.js: `/usr/bin/node`
   - igc-xc-score: `/usr/lib/node_modules/igc-xc-score/dist/igc-xc-score.cjs`

6. **.env.example** - Environment variables template
   - Copy to `.env` and customize for your needs

### ?? Documentation

7. **DOCKER_README.md** - Comprehensive guide (3,500+ words)
   - Installation instructions
   - Configuration options
   - Troubleshooting guide
   - Backup and restore procedures
   - Upgrade instructions
   - Security best practices
   - Performance tuning
   - Monitoring setup

8. **DOCKER_QUICKREF.md** - Quick reference guide
   - Common commands
   - File structure
   - Data locations
   - Quick troubleshooting

9. **THIS FILE** - Summary and next steps

### ??? Utility Scripts

10. **quick-start.sh** - Interactive setup script (Linux/Mac)
    - Checks prerequisites
    - Guides through environment selection
    - Creates necessary directories
    - Builds and starts container
    - Shows useful commands

11. **migrate-to-docker.sh** - Data migration script (Linux/Mac)
    - Migrates existing data from VPS to Docker volumes
    - Default path: `/home/webapp/ParagliderFlightLogDb`
    - Can be customized with command line argument
    - Sets correct permissions automatically

12. **migrate-to-docker.bat** - Data migration script (Windows)
    - Same functionality as Linux version
    - Windows-compatible commands

### ?? Optional Files

13. **.github/workflows/docker-build.yml.disabled** - CI/CD workflow
    - Automated Docker builds on GitHub Actions
    - Pushes to GitHub Container Registry
    - Disabled by default (rename to enable)

---

## ?? Getting Started

### Option 1: Quick Start (Recommended for first-time users)

**Linux/Mac:**
```bash
chmod +x quick-start.sh
./quick-start.sh
```

**Windows:**
```powershell
docker-compose -f docker-compose.dev.yml up -d
```

### Option 2: Manual Start

**Development:**
```bash
docker-compose -f docker-compose.dev.yml up -d
```

**Production:**
```bash
docker-compose up -d
```

### Option 3: Migrate Existing Data

If you have existing data at `/home/webapp/ParagliderFlightLogDb`:

```bash
chmod +x migrate-to-docker.sh
./migrate-to-docker.sh /home/webapp/ParagliderFlightLogDb
docker-compose up -d
```

---

## ?? Access Your Application

Once running, access the application at:
- **HTTP:** http://localhost:5000
- **HTTPS:** http://localhost:5001

On VPS, replace `localhost` with your server IP address.

---

## ?? Data Storage

### Development Mode
```
./dev-data/
??? db/          ? SQLite databases
??? photos/      ? Flight photos
??? score/       ? Temporary scoring files
??? logs/        ? Application logs
```

### Production Mode
Data is stored in Docker volumes:
- `flightlog-data` - Databases
- `flightlog-photos` - Photos
- `flightlog-score` - Score temp files
- `flightlog-logs` - Logs

---

## ?? What's Included in the Docker Image

? **.NET 8 Runtime** - For running your Blazor webapp  
? **Node.js 20 LTS** - For running igc-xc-score  
? **igc-xc-score** - Flight scoring engine (pre-installed)  
? **SQLite** - Database support  
? **All dependencies** - NuGet packages and runtime libraries  

---

## ?? Common Commands

### View Logs
```bash
docker-compose logs -f
```

### Stop Application
```bash
docker-compose down
```

### Restart Application
```bash
docker-compose restart
```

### Check Status
```bash
docker-compose ps
```

### Update Application
```bash
docker-compose down
git pull
docker-compose build
docker-compose up -d
```

### Backup Data
```bash
docker run --rm \
  -v flightlog-data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/backup-$(date +%Y%m%d).tar.gz -C /data .
```

---

## ? Key Features

### ?? Security
- Runs as non-root user (UID 1000)
- Isolated Docker network
- No unnecessary packages installed
- Regular security updates via base images

### ?? Data Persistence
- All user data stored in volumes
- Survives container restarts and updates
- Easy to backup and restore
- Can be moved between hosts

### ?? Performance
- Multi-stage build for smaller image size
- Optimized layer caching
- Pre-installed dependencies
- Fast startup time

### ??? Maintenance
- Simple upgrade process
- Automated with docker-compose
- No manual configuration needed
- Logs automatically rotated

---

## ?? Next Steps

### For Development

1. ? Run `./quick-start.sh` or `docker-compose -f docker-compose.dev.yml up -d`
2. ?? Open http://localhost:5000
3. ?? Register a new account
4. ?? Upload your first IGC file
5. ?? Data is saved in `./dev-data/`

### For VPS Production

1. ?? Push your code to GitHub
2. ??? SSH into your VPS
3. ?? Clone the repository: `git clone https://github.com/noar12/ParagliderFlightLog.git`
4. ?? Navigate to directory: `cd ParagliderFlightLog`
5. ?? (Optional) Migrate existing data: `./migrate-to-docker.sh /home/webapp/ParagliderFlightLogDb`
6. ?? Start application: `docker-compose up -d`
7. ?? Access at: http://your-vps-ip:5000
8. ?? (Recommended) Set up reverse proxy with SSL (see DOCKER_README.md)

### For Continuous Deployment

1. ?? Enable GitHub Actions workflow (rename `.github/workflows/docker-build.yml.disabled`)
2. ?? Configure GitHub Container Registry
3. ?? Automatic builds on every push
4. ?? Pull latest image on VPS: `docker pull ghcr.io/your-username/paragliderflightlog:latest`

---

## ?? Troubleshooting

### Container won't start?
```bash
docker-compose logs
```

### Database connection issues?
```bash
docker exec -it paraglider-flightlog ls -la /app/data/db/
```

### igc-xc-score not working?
```bash
docker exec -it paraglider-flightlog node --version
docker exec -it paraglider-flightlog npm list -g igc-xc-score
```

### Need more help?
See **DOCKER_README.md** for detailed troubleshooting guide.

---

## ?? Documentation

- **DOCKER_README.md** - Full documentation (read this!)
- **DOCKER_QUICKREF.md** - Quick reference
- **README.md** - Application documentation

---

## ?? Migration from VPS

Your current setup on VPS:
- Data location: `/home/webapp/ParagliderFlightLogDb`
- SQLite database files
- Flight photos in user directories

After Docker migration:
- Data in Docker volumes or `./dev-data/`
- Same functionality
- Easier updates and backups
- Portable between hosts
- Node.js and igc-xc-score included

The migration script (`migrate-to-docker.sh`) handles all the data transfer automatically!

---

## ? Verification Checklist

Before deploying to production, verify:

- [ ] Docker and Docker Compose are installed
- [ ] Ports 5000 and 5001 are available
- [ ] Firewall allows access to these ports
- [ ] (Optional) Existing data has been migrated
- [ ] Application starts without errors
- [ ] Can register and login
- [ ] Can upload IGC files
- [ ] igc-xc-score calculates scores correctly
- [ ] Photos can be uploaded
- [ ] Data persists after container restart

---

## ?? Support

For issues:
1. Check **DOCKER_README.md** troubleshooting section
2. Review container logs: `docker-compose logs -f`
3. Check GitHub issues: https://github.com/noar12/ParagliderFlightLog/issues
4. Create a new issue if needed

---

## ?? Success!

Your Paraglider Flight Log is now containerized and ready for easy deployment and maintenance!

**Happy flying! ????**

---

Generated on: $(date)
