# Docker Deployment Checklist

Use this checklist to ensure smooth deployment of Paraglider Flight Log in Docker.

## Pre-Deployment Checklist

### System Requirements
- [ ] Docker Engine 20.10+ installed
- [ ] Docker Compose 2.0+ installed
- [ ] At least 2GB free disk space
- [ ] Ports 5000 and 5001 available

### Verify Installations
```bash
# Check Docker
docker --version

# Check Docker Compose
docker-compose --version
# OR
docker compose version
```

## Development Deployment Checklist

- [ ] Navigate to project directory
- [ ] Run quick start script: `./quick-start.sh` (Linux/Mac)
  - OR: `docker-compose -f docker-compose.dev.yml up -d` (Windows)
- [ ] Wait for container to start (30-60 seconds)
- [ ] Open browser to http://localhost:5000
- [ ] Verify application loads
- [ ] Register a test account
- [ ] Upload a test IGC file
- [ ] Verify igc-xc-score calculates score
- [ ] Check data persists in `./dev-data/` directory

## Production Deployment Checklist (VPS)

### Initial Setup
- [ ] SSH into VPS
- [ ] Install Docker and Docker Compose
- [ ] Clone repository: `git clone https://github.com/noar12/ParagliderFlightLog.git`
- [ ] Navigate to directory: `cd ParagliderFlightLog`

### Data Migration (If Applicable)
- [ ] Locate existing data directory (e.g., `/home/webapp/ParagliderFlightLogDb`)
- [ ] Make script executable: `chmod +x migrate-to-docker.sh`
- [ ] Run migration: `./migrate-to-docker.sh /home/webapp/ParagliderFlightLogDb`
- [ ] Verify data copied successfully
- [ ] Create backup of original data

### Container Deployment
- [ ] Review and customize `.env` file (optional)
- [ ] Build image: `docker-compose build`
- [ ] Start container: `docker-compose up -d`
- [ ] Check logs: `docker-compose logs -f`
- [ ] Verify container is running: `docker-compose ps`

### Application Verification
- [ ] Open browser to http://your-vps-ip:5000
- [ ] Verify application loads
- [ ] Test login with existing account (if migrated)
- [ ] OR register new account
- [ ] Upload test IGC file
- [ ] Verify scoring works
- [ ] Check photos upload correctly
- [ ] Verify all features work

### Post-Deployment Tasks
- [ ] Configure firewall rules (allow ports 5000, 5001)
- [ ] Set up reverse proxy with SSL (recommended)
  - Nginx or Apache with Let's Encrypt
  - See DOCKER_README.md for configuration
- [ ] Configure automatic backups
- [ ] Set up monitoring (optional)
- [ ] Document server access details
- [ ] Test container restart: `docker-compose restart`
- [ ] Test data persistence after restart

## Security Checklist

- [ ] Container runs as non-root user (UID 1000)
- [ ] Only necessary ports exposed
- [ ] SSL/TLS configured (if public)
- [ ] Firewall rules configured
- [ ] Regular security updates enabled
- [ ] Backup strategy in place
- [ ] Access logs reviewed regularly

## Backup Checklist

### Manual Backup
```bash
# Create backup directory
mkdir -p backups

# Backup databases
docker run --rm \
  -v flightlog-data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/db-$(date +%Y%m%d).tar.gz -C /data .

# Backup photos
docker run --rm \
  -v flightlog-photos:/photos \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/photos-$(date +%Y%m%d).tar.gz -C /photos .
```

- [ ] Test backup command
- [ ] Verify backup files created
- [ ] Store backups off-server
- [ ] Document backup location
- [ ] Test restore procedure

### Automated Backup (Recommended)
- [ ] Create backup script (see DOCKER_README.md)
- [ ] Set up cron job for daily backups
- [ ] Configure backup retention policy
- [ ] Test automated backup
- [ ] Set up backup monitoring/alerts

## Upgrade Checklist

When updating the application:

- [ ] Create backup before upgrade
- [ ] Stop container: `docker-compose down`
- [ ] Pull latest code: `git pull`
- [ ] Review changelog for breaking changes
- [ ] Update configuration if needed
- [ ] Rebuild image: `docker-compose build`
- [ ] Start container: `docker-compose up -d`
- [ ] Check logs for errors
- [ ] Test application functionality
- [ ] Verify data integrity
- [ ] Document upgrade date and version

## Troubleshooting Checklist

If issues occur:

- [ ] Check container status: `docker-compose ps`
- [ ] Review logs: `docker-compose logs -f`
- [ ] Verify volumes exist: `docker volume ls`
- [ ] Check disk space: `df -h`
- [ ] Verify port availability: `netstat -tulpn | grep 5000`
- [ ] Test Node.js: `docker exec paraglider-flightlog node --version`
- [ ] Test igc-xc-score: `docker exec paraglider-flightlog npm list -g igc-xc-score`
- [ ] Check file permissions in volumes
- [ ] Review DOCKER_README.md troubleshooting section
- [ ] Check GitHub issues

## Monitoring Checklist

- [ ] Set up log monitoring
- [ ] Configure disk space alerts
- [ ] Monitor container health
- [ ] Track resource usage
- [ ] Set up uptime monitoring
- [ ] Configure error alerts

## Documentation Checklist

- [ ] Document VPS access details
- [ ] Record container configuration
- [ ] Note backup locations
- [ ] Document custom configurations
- [ ] Keep upgrade history
- [ ] Maintain runbook for common tasks

## Final Verification

- [ ] Application accessible from web
- [ ] All features working correctly
- [ ] Data persists across restarts
- [ ] Backups are working
- [ ] Monitoring is active
- [ ] Documentation is complete
- [ ] Team members can access
- [ ] Recovery procedures tested

---

## Quick Commands Reference

### Essential Commands
```bash
# Start
docker-compose up -d

# Stop
docker-compose down

# Restart
docker-compose restart

# Logs
docker-compose logs -f

# Status
docker-compose ps

# Rebuild
docker-compose build

# Update
docker-compose down && git pull && docker-compose build && docker-compose up -d
```

### Maintenance Commands
```bash
# Backup
docker run --rm -v flightlog-data:/data -v $(pwd)/backups:/backup \
  alpine tar czf /backup/backup-$(date +%Y%m%d).tar.gz -C /data .

# Restore
docker run --rm -v flightlog-data:/data -v $(pwd)/backups:/backup \
  alpine tar xzf /backup/backup-YYYYMMDD.tar.gz -C /data

# Access shell
docker exec -it paraglider-flightlog bash

# View volume location
docker volume inspect flightlog-data
```

---

**Note:** Check off items as you complete them. Keep this checklist for future reference and maintenance tasks.
