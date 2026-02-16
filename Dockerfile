# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ParagliderFlightLog/ParagliderFlightLog.csproj", "ParagliderFlightLog/"]
COPY ["ParagliderFlightLogTest/ParagliderFlightLogTest.csproj", "ParagliderFlightLogTest/"]
COPY ["ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.csproj", "ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/"]
COPY ["ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.Client/ParaglidingFlightLogWeb.Client.csproj", "ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.Client/"]

# Restore dependencies
RUN dotnet restore "ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ParaglidingFlightLogWeb/ParaglidingFlightLogWeb"
RUN dotnet build "ParaglidingFlightLogWeb.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ParaglidingFlightLogWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install Node.js 24.x (LTS)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_24.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Install igc-xc-score globally
RUN npm install -g igc-xc-score

# Create directories for data storage
RUN mkdir -p /app/data/db && \
    mkdir -p /app/data/photos && \
    mkdir -p /app/data/score && \
    mkdir -p /var/log/ParagliderFlightLog

# Install graphicsmagick
RUN apt-get update && \
    apt-get install -y graphicsmagick && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create a non-root user for running the application
RUN useradd -m -u 1001 appuser && \
    chown -R appuser:appuser /app /var/log/ParagliderFlightLog
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ParaglidingFlightLogWeb.dll"]
