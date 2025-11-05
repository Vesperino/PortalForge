# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PortalForge.Api/PortalForge.csproj", "PortalForge.Api/"]
RUN dotnet restore "PortalForge.Api/PortalForge.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/PortalForge.Api"
RUN dotnet build "PortalForge.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PortalForge.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create storage directory and non-root user
RUN useradd -m -u 1000 appuser && \
    mkdir -p /app/storage && \
    chown -R appuser:appuser /app
USER appuser

# Create volume mount point for persistent storage
VOLUME ["/app/storage"]

# Expose port
EXPOSE 5000

# Health check - check on port 5000 since container internal port is 5000
# The external port mapping 22022:5000 is done by docker run
# PathBase is not set in container, so health endpoint is at /health not /portalforge/be/health
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "PortalForge.dll"]
