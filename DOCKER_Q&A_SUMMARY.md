# Docker Questions & Answers Summary

This document summarizes the questions asked and answers provided during the Docker setup for Paraglider Flight Log.

---

## Q1: How does Docker Compose work?

**Question:** *"I don't know a lot about docker-compose. I am used to run my container using docker command. Can you explain how it works and more specifically with the docker-compose.yml that you wrote?"*

### Answer Summary

**Docker Compose** is a tool that simplifies running Docker containers by defining everything in a YAML file instead of long command-line arguments.

#### Traditional Docker vs Docker Compose

**Traditional Docker Commands:**
```sh
docker network create flightlog-network
docker volume create flightlog-data
docker volume create flightlog-photos
docker volume create flightlog-score
docker volume create flightlog-logs
docker build -t paraglider-flightlog:latest .
docker run -d --name paraglider-flightlog \
  --network flightlog-network \
  -p 5000:8080 -p 5001:8081 \
  -v flightlog-data:/app/data/db \
  -v flightlog-photos:/app/data/photos \
  -v flightlog-score:/app/data/score \
  -v flightlog-logs:/var/log/ParagliderFlightLog \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  --restart unless-stopped \
  paraglider-flightlog
```

**With Docker Compose:**
```sh
docker-compose up -d
```

#### Key Sections Explained

| Section | Purpose |
|---------|---------|
| **`version: '3.8'`** | Docker Compose file format version |
| **`services:`** | Defines containers to run |
| **`build:`** | How to build the image |
| **`ports:`** | Maps host ports to container ports (5000:8080) |
| **`volumes:`** | Persistent storage for data |
| **`environment:`** | Environment variables |
| **`restart:`** | Auto-restart policy |
| **`networks:`** | Custom network for isolation |

#### Benefits

✅ **Declarative** - Define once, use everywhere  
✅ **Version Controlled** - Commit to Git  
✅ **Reproducible** - Same setup on any machine  
✅ **Simple** - No need to remember long commands  
✅ **Multi-Container Ready** - Easy to add services  

---

## Q2: Why are there two volume sections?

**Question:** *"Can you explain why there are two sections about volume?"*

### Answer Summary

The two volume sections serve different purposes:

#### 1. Service-Level Volumes (Under `services:`)

```yaml
services:
  paraglider-flightlog:
    volumes:
      - flightlog-data:/app/data/db
      - flightlog-photos:/app/data/photos
```

**Purpose:** **USES/MOUNTS** volumes into the container
- Specifies **WHERE** inside the container
- Format: `VOLUME_NAME:CONTAINER_PATH`

#### 2. Top-Level Volumes Section

```yaml
volumes:
  flightlog-data:
    driver: local
  flightlog-photos:
    driver: local
```

**Purpose:** **DECLARES/DEFINES** the volumes
- Tells Docker to **CREATE** these volumes
- Specifies **HOW** they are created (driver, options)

#### Why Both Are Needed

It's a two-step process:
1. **Declare the volume exists** (top-level) - like declaring a variable
2. **Use the volume** (service-level) - like using that variable

#### Restaurant Storage Analogy

- **Top-level:** "We have a storage room called 'flightlog-data'"
- **Service-level:** "Put the storage room contents on shelf B (at /app/data/db)"

#### When You Don't Need Top-Level Declaration

**Bind mounts** (like in `docker-compose.dev.yml`) don't need declaration:
```yaml
services:
  paraglider-flightlog:
    volumes:
      - ./dev-data/db:/app/data/db  # Host path, no declaration needed
```

---

## Q3: How do Docker networks work?

**Question:** *"I have never used networks with docker. Can you explain how this works?"*

### Answer Summary

A **Docker network** is an isolated communication channel that allows containers to talk to each other, like a private LAN.

#### Your Network Configuration

```yaml
services:
  paraglider-flightlog:
    networks:
      - flightlog-network

networks:
  flightlog-network:
    driver: bridge
```

#### Network Drivers

| Driver | Purpose | Use Case |
|--------|---------|----------|
| **bridge** | Private internal network on host | Single-host (your VPS) ✅ |
| **host** | Shares host's network stack | Maximum performance |
| **overlay** | Spans multiple Docker hosts | Docker Swarm/Kubernetes |
| **none** | No network access | Isolated containers |

#### Why Use a Network for Single Container?

1. **Future Expansion** - Easy to add databases, cache, etc.
2. **Isolation** - Other containers can't access yours
3. **DNS Resolution** - Containers can reach each other by name
4. **Organization** - Clear which containers belong together

#### Visual Representation

```
┌─────────────────────────────────────────┐
│ Host Machine (Your VPS)                 │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │ flightlog-network (Bridge)         │ │
│  │                                    │ │
│  │  ┌──────────────────────────────┐ │ │
│  │  │ paraglider-flightlog         │ │ │
│  │  │ IP: 172.18.0.2               │ │ │
│  │  │ Ports: 8080 → 5000           │ │ │
│  │  └──────────────────────────────┘ │ │
│  └────────────────────────────────────┘ │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │ Other Networks (isolated)          │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

#### Future Multi-Container Example

```yaml
services:
  paraglider-flightlog:
    networks:
      - flightlog-network
  
  postgres:
    image: postgres:16
    networks:
      - flightlog-network  # Can reach app at "paraglider-flightlog:8080"
  
  redis:
    image: redis:alpine
    networks:
      - flightlog-network  # Can also communicate
```

---

## Q4: What is Alpine in the migration script?

**Question:** *"In migrate-to-docker.sh, what is Alpine?"*

### Answer Summary

**Alpine Linux** is a tiny (~5 MB), security-focused Linux distribution designed for Docker containers.

#### What Alpine Does in Your Script

```sh
docker run --rm \
    -v flightlog-data:/data \
    -v "$SOURCE_PATH":/source:ro \
    alpine sh -c "cp -rv /source/*.db /data/"
```

#### Breaking Down the Command

| Part | Purpose |
|------|---------|
| **`docker run`** | Start a container |
| **`--rm`** | Auto-delete container when done |
| **`-v flightlog-data:/data`** | Mount Docker volume |
| **`-v "$SOURCE_PATH":/source:ro`** | Mount host directory (read-only) |
| **`alpine`** | Use Alpine Linux image |
| **`sh -c "..."`** | Run shell commands |

#### Why Alpine?

| Feature | Details |
|---------|---------|
| **Size** | Only ~5 MB (vs Ubuntu's 70+ MB) |
| **Tools** | Has cp, find, chown, tar, etc. |
| **Fast** | Quick download and startup |
| **Universal** | Works on any OS with Docker |
| **Safe** | Temporary, auto-deleted |

#### What It Does in Migration

1. **Copies database files** from VPS to Docker volume
2. **Copies photo files** from VPS to Docker volume
3. **Sets permissions** (UID 1000 for appuser)
4. **Verifies migration** by listing files

#### Alternative Images (Less Ideal)

```sh
# Ubuntu (70+ MB)
docker run --rm ubuntu ...

# Busybox (2 MB, fewer tools)
docker run --rm busybox ...

# Alpine is the sweet spot! ✅
```

#### Why Not Copy Files Directly?

❌ Docker volume location varies by OS  
❌ Permission issues  
❌ Not portable  
✅ **Using Alpine container works everywhere!**

---

## Q5: What is the purpose of .env.example?

**Question:** *"What is the purpose of the .env.example file?"*

### Answer Summary

**`.env.example`** is a **template file** that shows what environment variables are available, with safe example values.

#### Two Separate Files

| File | Purpose | In Git? |
|------|---------|---------|
| **`.env.example`** | Template with safe defaults | ✅ Yes (committed) |
| **`.env`** | Your actual configuration | ❌ No (gitignored) |

#### Why Separate?

**Security & Privacy:**
- `.env` might contain passwords, API keys, secrets
- `.env.example` only has placeholders and documentation
- Team members see what's needed without exposing secrets

#### How to Use

```sh
# Step 1: Copy template
cp .env.example .env

# Step 2: Customize
nano .env

# Step 3: Docker Compose reads .env automatically
docker-compose up -d
```

#### Variables in Your .env.example

```
# Application Settings
ASPNETCORE_ENVIRONMENT=Production      # Dev/Staging/Production
ASPNETCORE_URLS=http://+:8080         # What the app listens on

# Port Mappings
HTTP_PORT=5000                         # Change if port conflicts
HTTPS_PORT=5001

# Container Name
CONTAINER_NAME=paraglider-flightlog   # Custom container name

# Image Settings
IMAGE_TAG=latest                       # Version tag
IMAGE_NAME=paraglider-flightlog       # Image name

# Logging
LOG_LEVEL=Information                  # Trace/Debug/Info/Warning/Error
```

#### Using Variables in docker-compose.yml

Currently your `docker-compose.yml` has hardcoded values. You could update it to:

```yaml
services:
  paraglider-flightlog:
    container_name: ${CONTAINER_NAME:-paraglider-flightlog}
    ports:
      - "${HTTP_PORT:-5000}:8080"
      - "${HTTPS_PORT:-5001}:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
```

**Syntax:** `${VARIABLE:-default}` uses the variable or default if not set.

---

## Q6: Where should .env be located?

**Question:** *"Where should .env be located to be taken into account by docker-compose?"*

### Answer Summary

The `.env` file must be in the **same directory as `docker-compose.yml`**.

#### Correct Location

```
C:\Users\mar\source\repos\noar12\ParagliderFlightLog\
├── docker-compose.yml          ← Docker Compose file
├── .env                        ← YOUR .env FILE HERE ✅
└── .env.example                ← Template
```

#### How to Create

**Windows PowerShell:**
```powershell
cd C:\Users\mar\source\repos\noar12\ParagliderFlightLog
Copy-Item .env.example .env
notepad .env
```

**Git Bash / WSL:**
```sh
cd /c/Users/mar/source/repos/noar12/ParagliderFlightLog
cp .env.example .env
nano .env
```

#### How Docker Compose Finds .env

1. **Automatically** - Looks in the same directory as `docker-compose.yml`
2. **Custom path** - Use `--env-file` flag:
```sh
docker-compose --env-file .env.production up -d
```

#### Verify It Works

```sh
# View final configuration with variables substituted
docker-compose config
```

Should show your values from `.env`, not `${VARIABLE}` placeholders.

#### Common Issues

| Issue | Solution |
|-------|----------|
| `.env` not found | Make sure it's in project root |
| Wrong filename | Must be exactly `.env` (not `.ENV` or `env.txt`) |
| Using `.env.example` | Docker only reads `.env`, not `.env.example` |
| Wrong directory | Must be where `docker-compose.yml` is |

#### Multiple Environments

```
ParagliderFlightLog/
├── .env                    ← Default (Production)
├── .env.development        ← Development settings
├── .env.staging            ← Staging settings
└── .env.example            ← Template
```

```sh
# Use different env files
docker-compose --env-file .env.development up -d
docker-compose --env-file .env.staging up -d
```

---

## Summary Table

| # | Question | Key Takeaway |
|---|----------|-------------|
| 1 | **Docker Compose** | Simplifies Docker with YAML config instead of long commands |
| 2 | **Two Volume Sections** | Top-level **declares**, service-level **uses** volumes |
| 3 | **Docker Networks** | Isolates containers and enables DNS-based communication |
| 4 | **Alpine in Script** | Tiny Linux image used for temporary file operations |
| 5 | **.env.example Purpose** | Template for environment variables (safe to commit) |
| 6 | **.env Location** | Same directory as `docker-compose.yml` |

---

## Quick Reference Commands

```sh
# Start application
docker-compose up -d

# View logs
docker-compose logs -f

# Stop application
docker-compose down

# Check configuration
docker-compose config

# Run migration
./migrate-to-docker.sh /home/webapp/ParagliderFlightLogDb

# List volumes
docker volume ls

# List networks
docker network ls

# Inspect network
docker network inspect flightlog-network
```

---

## Files Created for Dockerization

### Core Docker Files
1. **Dockerfile** - Multi-stage build with .NET 8, Node.js, and igc-xc-score
2. **docker-compose.yml** - Production deployment configuration
3. **docker-compose.dev.yml** - Development deployment configuration
4. **.dockerignore** - Build optimization

### Configuration Files
5. **appsettings.Production.json** - Docker-optimized application settings
6. **.env.example** - Environment variables template

### Documentation
7. **DOCKER_README.md** - Comprehensive deployment guide (3,500+ words)
8. **DOCKER_QUICKREF.md** - Quick reference guide
9. **DOCKER_DEPLOYMENT_SUMMARY.md** - Getting started overview
10. **DOCKER_DEPLOYMENT_CHECKLIST.md** - Step-by-step deployment checklist
11. **DOCKER_Q&A_SUMMARY.md** - This file

### Utility Scripts
12. **quick-start.sh** - Interactive setup script (Linux/Mac)
13. **migrate-to-docker.sh** - VPS data migration script (Linux/Mac)
14. **migrate-to-docker.bat** - VPS data migration script (Windows)

### Optional Files
15. **.github/workflows/docker-build.yml.disabled** - CI/CD automation template

---

## What Was Dockerized

### Application Components
- ✅ .NET 8 Blazor WebAssembly application
- ✅ SQLite databases (user data, shared data)
- ✅ Flight photos storage
- ✅ Application logs
- ✅ Node.js runtime
- ✅ igc-xc-score npm package (flight scoring)

### Data Persistence
- **Production:** Docker named volumes
- **Development:** Local `./dev-data/` directory

### Network Configuration
- Custom bridge network for isolation
- Port mappings: 5000 (HTTP), 5001 (HTTPS)

---

## Next Steps

1. ✅ Docker setup complete
2. 📖 Read `DOCKER_README.md` for detailed instructions
3. 🚀 Run `./quick-start.sh` (or use docker-compose commands)
4. 🔄 (Optional) Migrate existing data with `./migrate-to-docker.sh`
5. 🌐 Access application at http://localhost:5000

---

**Generated:** January 2025  
**Project:** Paraglider Flight Log  
**Repository:** https://github.com/noar12/ParagliderFlightLog  
**Branch:** dockerization  

**For More Information:**
- **Comprehensive Guide:** `DOCKER_README.md`
- **Quick Reference:** `DOCKER_QUICKREF.md`
- **Deployment Steps:** `DOCKER_DEPLOYMENT_CHECKLIST.md`
- **Getting Started:** `DOCKER_DEPLOYMENT_SUMMARY.md`

