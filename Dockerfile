# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files
COPY ["src/RmsDemo.csproj", "src/"]
RUN dotnet restore "src/RmsDemo.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src"

# Build application
RUN dotnet build "RmsDemo.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RmsDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Security: Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Security: Install security updates and remove package cache
RUN apk update && apk upgrade && \
    apk add --no-cache \
    ca-certificates \
    curl \
    wget \
    && rm -rf /var/cache/apk/* \
    && rm -rf /tmp/*

WORKDIR /app

# Security: Set proper ownership and permissions
COPY --from=publish --chown=appuser:appgroup /app/publish .

# Security: Use non-root user
USER appuser

# Security: Expose only necessary port
EXPOSE 8080

# Security: Use non-root port
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RmsDemo.dll"]
