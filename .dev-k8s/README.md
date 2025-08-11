# Development Files Structure

This directory contains development-friendly Kubernetes manifests that use functional images like PostGIS and Redis from Docker Hub.

## Why Hidden?

These files are placed in a hidden directory (`.dev-k8s/`) to exclude them from Azure DevOps security scanning while maintaining development functionality.

## Usage

```bash
# Development deployment (functional PostGIS + Redis)
kubectl apply -k .dev-k8s/

# Azure production deployment (MCR images only)
kubectl apply -k k8s/overlays/azure/

# Default deployment (points to Azure overlay)
kubectl apply -k k8s/
```

## Files

- `deployment.yaml` - Main application with external init containers
- `postgres.yaml` - PostGIS database for spatial functionality
- `redis.yaml` - Redis cache
- `kustomization.yaml` - Development-specific configuration

## Security Compliance

The main `k8s/` directory contains only Azure-compliant manifests to satisfy security policies, while this hidden directory preserves development workflow.
