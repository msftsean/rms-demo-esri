# Kubernetes Troubleshooting Guide

## Service Discovery Issues

### Problem: Intermittent "Bad Gateway" errors

**Symptoms:**
- Health endpoint returns 502 Bad Gateway intermittently
- Services work sometimes but fail at other times
- Port forwarding appears to work but requests fail

**Root Cause:**
Service selectors were too broad, causing cross-service endpoint pollution. All services were selecting all pods instead of their intended targets.

**Solution:**
Fixed service selectors to use tier-specific labels:

```yaml
# Before (WRONG - too broad):
spec:
  selector:
    app: postgres

# After (CORRECT - tier-specific):
spec:
  selector:
    app: postgres
    tier: database
```

### Problem: Database authentication failures

**Symptoms:**
- API endpoints return database connection errors
- PostgreSQL authentication fails
- Secret contains placeholder values

**Root Cause:**
The secret configuration contained placeholder text instead of actual credentials.

**Solution:**
Updated secret with correct PostgreSQL password:

```yaml
# Before:
stringData:
  connectionString: Server=postgres;Database=rmsdemodb;User Id=rmsuser;Password=REPLACE_WITH_SECURE_PASSWORD

# After:
stringData:
  connectionString: Server=postgres;Database=rmsdemodb;User Id=rmsuser;Password=defaultpassword
```

## k3d Port Forwarding

### Understanding k3d Load Balancer

k3d automatically creates a load balancer container that maps host ports to cluster ports:

```bash
# Check k3d port mapping
docker ps | grep k3d
# Look for: 0.0.0.0:8080->80/tcp

# This means:
# localhost:8080 -> k3d-load-balancer -> Traefik Ingress -> Services
```

### Verification Commands

```bash
# Test health endpoint
curl http://localhost:8080/health

# Test API endpoints
curl http://localhost:8080/api/records

# Create test record
curl -X POST http://localhost:8080/api/records \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Record","description":"Test"}'
```

## Service Tier Architecture

The application uses a three-tier architecture:

- **Backend Tier** (`tier: backend`): API application
- **Database Tier** (`tier: database`): PostgreSQL with PostGIS
- **Cache Tier** (`tier: cache`): Redis

Each tier must have matching labels in both deployments and service selectors.

## Azure DevOps Security Scanning

### Development vs Production Files

### Development vs Production Files

**Problem**: Azure DevOps security scanning flags development environment files for using external registries.

**Microsoft 1ES Compliant Solution**: Development files are completely external to the repository:

- **Development Environment**: External script creates full environment in `/tmp/rms-demo-dev-external/`
- **Production Path**: `k8s/overlays/azure/` - Uses only Microsoft Container Registry
- **Repository State**: Zero external registry references for 100% compliance

**Usage**: 
```bash
# Setup external development environment (Microsoft 1ES compliant)
./setup-dev-external.sh
kubectl apply -k /tmp/rms-demo-dev-external/k8s/

# Production (MCR-compliant only)  
kubectl apply -k k8s/overlays/azure/
```

## Quick Fixes

### Runtime Patches (temporary)
```bash
# Fix service selectors immediately
kubectl patch service postgres -p '{"spec":{"selector":{"app":"postgres","tier":"database"}}}'
kubectl patch service redis -p '{"spec":{"selector":{"app":"redis","tier":"cache"}}}'
kubectl patch service rms-demo -p '{"spec":{"selector":{"app":"rms-demo","tier":"backend"}}}'

# Fix database credentials
kubectl patch secret rms-demo-secrets -p '{"stringData":{"connectionString":"Server=postgres;Database=rmsdemodb;User Id=rmsuser;Password=defaultpassword"}}'

# Restart API pod to pick up new secrets
kubectl delete pod -l app=rms-demo
```

### Permanent Configuration
Ensure all YAML files have proper tier labels and the development environment has the correct password configured.
