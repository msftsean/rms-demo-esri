# Azure Security Compliance Overlay

This Kustomize overlay provides Azure-compliant container images that satisfy Microsoft security policies while maintaining development flexibility.

## Purpose

Azure DevOps security policies require approved container registries. This overlay automatically replaces development-friendly images with Microsoft Container Registry (MCR) approved alternatives during Azure deployments.

## Image Mappings

| Development Image | Azure Approved Image | Purpose |
|------------------|---------------------|---------|
| `postgis/postgis:15-3.3-alpine` | `mcr.microsoft.com/mirror/docker/library/postgres:15-alpine` | Database (Note: PostGIS extensions may need manual setup) |
| `redis:7-alpine` | `mcr.microsoft.com/mirror/docker/library/redis:7-alpine` | Cache |
| `rms-demo:local` | `mcr.microsoft.com/dotnet/aspnet:8.0` | Application runtime |

## Usage

```bash
# Development deployment (uses original images)
kubectl apply -k k8s/

# Azure deployment (uses approved images)
kubectl apply -k k8s/overlays/azure/
```

## Notes

- Development environments can use the base configuration
- Azure production deployments should use this overlay
- PostGIS functionality may require additional configuration with the standard postgres image
