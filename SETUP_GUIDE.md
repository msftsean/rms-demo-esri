# RMS Demo ESRI Setup Guide

## Overview
This document captures the complete setup process for the RMS Demo ESRI project, including both Azure DevOps and GitHub Enterprise configurations.

## Initial Setup Completed

### 🏢 Azure DevOps Configuration
- **Organization**: https://dev.azure.com/seanbox
- **Project**: rmsdemo
- **Pipeline**: rms-demo-esri-ci (ID: 1)
- **Environments**: dev, prod
- **Service Connection**: GitHub integration configured

### 🐙 GitHub Repository Configuration
- **Repository**: https://github.com/msftsean/rms-demo-esri
- **Branch**: main
- **GitHub Project**: https://github.com/users/msftsean/projects/3

## Files Created

### GitHub Actions & Workflows
- `.github/workflows/ci-cd.yml` - Main CI/CD pipeline
- `.github/workflows/security.yml` - Security scanning workflow
- `.github/codeql/codeql-config.yml` - CodeQL configuration

### Issue Templates
- `.github/ISSUE_TEMPLATE/bug_report.yml`
- `.github/ISSUE_TEMPLATE/feature_request.yml`
- `.github/ISSUE_TEMPLATE/security_vulnerability.yml`

### Project Configuration
- `.github/pull_request_template.md`
- `.github/dependabot.yml`
- `SECURITY.md`
- `Dockerfile`
- `docker-compose.yml`

### Azure DevOps Integration
- `setup.sh` - Azure DevOps setup script
- `azure-pipelines.yml` - Azure Pipelines configuration
- `boards-import.csv` - Work items for Azure Boards

## GitHub Project Content

### Created Issues (8 total)
1. **Issue #1**: [EPIC] RMS Data Integration with ESRI ArcGIS
2. **Issue #2**: [SECURITY] Implement OAuth 2.0 Authentication
3. **Issue #3**: [BUG] API Rate Limiting Not Working Correctly
4. **Issue #7**: [FEATURE] Interactive Map Dashboard
5. **Issue #8**: [TASK] Set up Continuous Integration Pipeline
6. **Issue #9**: [BUG] Memory Leak in Map Rendering Component
7. **Issue #10**: [DOCUMENTATION] API Documentation and Developer Guide
8. **Issue #11**: [ENHANCEMENT] Performance Optimization for Large Datasets

## Key Commands Used

### Azure DevOps Setup
```bash
# Configure Azure DevOps
az devops configure --defaults organization="https://dev.azure.com/seanbox"
az devops project create --name "rmsdemo"
az devops configure --defaults project="rmsdemo"

# Create environments
echo '{"name":"dev"}' > /tmp/dev-env.json
az devops invoke --route-parameters project="rmsdemo" --http-method POST --area distributedtask --resource environments --in-file /tmp/dev-env.json

echo '{"name":"prod"}' > /tmp/prod-env.json
az devops invoke --route-parameters project="rmsdemo" --http-method POST --area distributedtask --resource environments --in-file /tmp/prod-env.json

# Create pipeline
export AZURE_DEVOPS_EXT_GITHUB_PAT="your_github_pat"
az pipelines create --name "rms-demo-esri-ci" --repository "https://github.com/msftsean/rms-demo-esri" --branch main --yml-path azure-pipelines.yml --skip-run true --repository-type GitHub
```

### GitHub Setup
```bash
# Create repository
gh repo create msftsean/rms-demo-esri --public --description "RMS Demo ESRI project for Azure DevOps integration"

# Create issues
gh issue create --title "[EPIC] RMS Data Integration with ESRI ArcGIS" --body "..." --assignee msftsean

# Create project
gh auth refresh -s project
gh project create --title "RMS Demo ESRI - Sprint Planning" --owner msftsean

# Add items to project
gh project item-add 3 --owner msftsean --url https://github.com/msftsean/rms-demo-esri/issues/1
```

## GitHub PAT Configuration

### Required Scopes
- `repo` - Full repository access
- `admin:repo_hook` - Webhook management
- `user` - User information
- `project` - Project management

### Environment Variable
```bash
export AZURE_DEVOPS_EXT_GITHUB_PAT="your_github_pat_here"
```

## Next Steps for Demo

### Manual Configuration Required
1. **Azure Boards**: Import work items using `boards-import.csv`
2. **Environment Approvals**: Add segayle@microsoft.com as approver for prod environment
3. **GitHub Branch Protection**: Configure protection rules for main branch
4. **GitHub Advanced Security**: Enable GHAS features
5. **Dashboards**: Create monitoring dashboards in both platforms

### Troubleshooting Notes
- Work item types in Azure DevOps Basic template: Epic, Issue, Task
- GitHub PAT needs admin rights on repository for webhook creation
- Service connection must be properly configured for pipeline creation

## Comparison Points for Demo

### GitHub vs Azure DevOps
- **Project Management**: GitHub Projects vs Azure Boards
- **CI/CD**: GitHub Actions vs Azure Pipelines
- **Security**: GHAS vs Microsoft Defender for DevOps
- **Integration**: Different workflows and capabilities
- **Enterprise Features**: Both platforms offer enterprise-grade features

## Security Features Demonstrated

### GitHub Advanced Security (GHAS)
- CodeQL static analysis
- Secret scanning
- Dependency review
- Security advisories

### DevOps Security
- Automated security scanning
- Container security
- Infrastructure as code security
- Compliance documentation

## Repository Structure
```
rms-demo-esri/
├── .github/
│   ├── workflows/
│   │   ├── ci-cd.yml
│   │   └── security.yml
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.yml
│   │   ├── feature_request.yml
│   │   └── security_vulnerability.yml
│   ├── codeql/
│   │   └── codeql-config.yml
│   ├── dependabot.yml
│   └── pull_request_template.md
├── README.md
├── SECURITY.md
├── Dockerfile
├── docker-compose.yml
├── azure-pipelines.yml
├── setup.sh
└── boards-import.csv
```

## Contact Information
- **Repository**: https://github.com/msftsean/rms-demo-esri
- **Azure DevOps**: https://dev.azure.com/seanbox/rmsdemo
- **GitHub Project**: https://github.com/users/msftsean/projects/3

---

**Note**: This setup guide serves as a complete reference for recreating or modifying the RMS Demo ESRI project configuration.
