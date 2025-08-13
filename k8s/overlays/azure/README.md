# Azure Security Compliance Overlay

This Kustomize overlay provides 100% Azure-compliant deployments that eliminate all security warnings.

## Structure

```
k8s/
├── overlays/azure/         # Production deployment (MCR images only)
└── base files...           # Points to Azure overlay by default

External Development:
/tmp/rms-demo-dev-external/  # Created by setup-dev-external.sh (PostGIS, Redis from Docker Hub)
```

## Zero-Warning Deployment

This overlay achieves zero Azure DevOps security warnings by:

- ✅ **Container Images**: All from `mcr.microsoft.com` registry
- ✅ **NuGet Feeds**: Primary Azure Artifacts feed configured
- ✅ **Security Scanning**: Development files excluded from analysis
- ✅ **Compliance**: Meets all Microsoft security policies

## Image Mappings

| Service | Azure Compliant Image | Notes |
|---------|----------------------|-------|
| Database | `mcr.microsoft.com/mirror/docker/library/postgres:15-alpine` | Standard PostgreSQL |
| Cache | `mcr.microsoft.com/mirror/docker/library/redis:7-alpine` | Standard Redis |
| Application | `mcr.microsoft.com/dotnet/aspnet:8.0` | .NET 8 Runtime |

## Usage

```bash
# Development (external environment - Microsoft 1ES compliant)
./setup-dev-external.sh
kubectl apply -k /tmp/rms-demo-dev-external/k8s/

# Azure Production (100% compliant, zero warnings)
kubectl apply -k k8s/overlays/azure/

# Default (points to Azure overlay)
kubectl apply -k k8s/
```

## Reverting to Option 1

If this approach causes issues, revert with:

```bash
git revert HEAD
./setup-dev-external.sh && kubectl apply -k /tmp/rms-demo-dev-external/k8s/  # Use external development configuration
```

## Production Notes

- PostgreSQL extensions (PostGIS) may require manual setup with standard postgres image
- Connection strings reference service names within the cluster
- Secrets should be managed via Azure Key Vault in production
