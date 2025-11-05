@echo off
REM Migration script for Windows - Moving data from existing installation to Docker volumes
REM Usage: migrate-to-docker.bat [source_path]

setlocal EnableDelayedExpansion

REM Default source path (can be overridden by command line argument)
set SOURCE_PATH=%1
if "%SOURCE_PATH%"=="" set SOURCE_PATH=C:\ParagliderFlightLogDb

echo ================================================
echo Paraglider Flight Log - Docker Migration Script
echo ================================================
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo Error: Docker is not installed or not in PATH
    exit /b 1
)

REM Check if source directory exists
if not exist "%SOURCE_PATH%" (
    echo Error: Source directory %SOURCE_PATH% does not exist
    echo Usage: migrate-to-docker.bat [source_path]
    exit /b 1
)

echo Source directory: %SOURCE_PATH%
echo.

REM Ask for confirmation
set /p CONFIRM="This will migrate data from %SOURCE_PATH% to Docker volumes. Continue? (y/N): "
if /i not "%CONFIRM%"=="y" (
    echo Migration cancelled.
    exit /b 0
)

echo.
echo Step 1: Creating Docker volumes...
docker volume create flightlog-data
docker volume create flightlog-photos
docker volume create flightlog-score
docker volume create flightlog-logs
echo Volumes created
echo.

echo Step 2: Copying database files...
docker run --rm -v flightlog-data:/data -v "%SOURCE_PATH%":/source:ro alpine sh -c "cp -rv /source/*.db /data/ 2>/dev/null || echo 'No .db files in root'; find /source -name '*.db' -type f -exec sh -c 'mkdir -p /data/$(dirname ${1#/source/}) && cp -v $1 /data/${1#/source/}' _ {} \;"
echo Database files copied
echo.

echo Step 3: Copying photo files (if any)...
docker run --rm -v flightlog-photos:/photos -v "%SOURCE_PATH%":/source:ro alpine sh -c "find /source -type d -name 'FlightPhotos' -exec cp -rv {} /photos/ \; 2>/dev/null || echo 'No FlightPhotos directories found'"
echo Photo files copied
echo.

echo Step 4: Setting correct permissions...
docker run --rm -v flightlog-data:/data alpine chown -R 1000:1000 /data
docker run --rm -v flightlog-photos:/photos alpine chown -R 1000:1000 /photos
docker run --rm -v flightlog-score:/score alpine chown -R 1000:1000 /score
docker run --rm -v flightlog-logs:/logs alpine chown -R 1000:1000 /logs
echo Permissions set
echo.

echo Step 5: Verifying migration...
echo.
echo Database volume contents:
docker run --rm -v flightlog-data:/data alpine ls -lha /data
echo.

echo ================================================
echo Migration completed successfully!
echo ================================================
echo.
echo Next steps:
echo 1. Review the copied files above
echo 2. Start the application: docker-compose up -d
echo 3. Check logs: docker-compose logs -f
echo 4. Access the application at http://localhost:5000
echo.

endlocal
