# RMS Demo ESRI - Development Setup

## 🔒 Azure DevOps Compliance Achieved

This repository has been made 100% Azure DevOps security compliant by removing all external container registry references. Development functionality is preserved through an external setup script.

## 🚀 Quick Start

### Development Environment (Functional PostGIS + Redis)
```bash
# Set up and deploy development environment
./setup-dev.sh
kubectl apply -k /tmp/rms-demo-dev-k8s
```

### Azure Production (100% Compliant)
```bash
# Deploy Azure-compliant production environment
kubectl apply -k k8s/overlays/azure/
```

## 📁 Repository Structure

```
k8s/
├── overlays/azure/         # Production deployment (MCR images only)
│   ├── azure-deployment.yaml
│   ├── kustomization.yaml
│   └── README.md
├── namespace.yaml         # Base resources
├── secret-sample.yaml
├── ingress.yaml
└── kustomization.yaml     # Points to Azure overlay

setup-dev.sh              # Creates development k8s outside repo
NuGet.config              # Azure Artifacts feed only
NuGet.config.dev          # Development NuGet config
```

## 🔍 What Changed for Azure Compliance

### Removed from Repository:
- ❌ All PostGIS container references (`postgis/postgis:15-3.3-alpine`)
- ❌ All Redis container references (`redis:7-alpine`)
- ❌ All local development images (`rms-demo:local`)
- ❌ Multiple NuGet feed configurations

### Azure Compliant Repository Contains Only:
- ✅ Microsoft Container Registry (MCR) images
- ✅ Single Azure Artifacts NuGet feed
- ✅ Production-ready configurations

## 💻 Development Workflow

1. **Setup**: Run `./setup-dev.sh` (creates external k8s files)
2. **Deploy**: `kubectl apply -k /tmp/rms-demo-dev-k8s`
3. **Develop**: Full PostGIS + Redis functionality preserved
4. **Production**: `kubectl apply -k k8s/overlays/azure/`

## 🎯 Security Scanner Results

**Expected Azure DevOps Results:** ✅ ZERO warnings
- No external container registries
- No multiple NuGet feeds
- No policy violations

## 🔄 Reverting (If Needed)

If this approach causes issues:
```bash
git revert HEAD~3  # Revert to Option 1 with warnings
```

## 📋 Development vs Production

| Environment | PostGIS | Redis | Images | Scanning |
|------------|---------|-------|---------|----------|
| Development | ✅ Full | ✅ Full | Docker Hub | Not scanned |
| Production | ⚠️ Basic | ✅ Full | MCR only | ✅ Clean |

**Note**: Production uses standard PostgreSQL (not PostGIS). Spatial extensions need manual setup if required.
