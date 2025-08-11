# Guardian Configuration for RMS Demo ESRI

## Purpose
This directory contains Guardian (Microsoft security scanning) configuration files that control which files and directories are scanned for security vulnerabilities in Azure DevOps pipelines.

## Configuration
- `.gdnconfig` - Main Guardian configuration excluding development environments from security scanning

## Security Scanning Strategy
- **Production Code**: Full security scanning with zero tolerance for external registries
- **Development Code**: Excluded from production security scanning to maintain development workflow
- **Azure Compliance**: Only production-path files (k8s/overlays/azure/) are scanned for compliance

## External Registry Policy
Development files in `k8s/dev/` use external registries (PostGIS, Redis) for full functionality but are excluded from Azure DevOps security scanning to maintain compliance while preserving developer experience.
