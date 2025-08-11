# Demo Next Steps - Manual Configuration Guide

This document provides detailed step-by-step instructions for completing the manual configuration required after the automated deployment setup.

## Overview

The following manual configurations are required to complete the demo environment setup:

- [Azure Boards: Import Work Items](#azure-boards-import-work-items)
- [Environment Approvals Configuration](#environment-approvals-configuration)
- [GitHub Branch Protection Rules](#github-branch-protection-rules)
- [GitHub Advanced Security (GHAS)](#github-advanced-security-ghas)
- [Monitoring Dashboards](#monitoring-dashboards)

---

## **1. Azure Boards: Import Work Items** 📋

### **Step-by-Step Process:**

1. **Navigate to Azure DevOps**
   - Go to `https://dev.azure.com/[your-org]/[your-project]`
   - Click on **Boards** in the left navigation

2. **Access Import Feature**
   - Click **Boards** → **Work items** 
   - Click **New** → **Import work items**
   - Or use direct URL: `https://dev.azure.com/[your-org]/[your-project]/_workitems/import`

3. **Upload CSV File**
   - Click **Choose file** and select `boards-import.csv`
   - Click **Import**

4. **Configure Field Mapping**
   ```
   CSV Column → Azure DevOps Field
   Title → Title
   Work Item Type → Work Item Type
   Area Path → Area Path
   Iteration Path → Iteration Path  
   Tags → Tags
   ```

5. **Verify Import**
   - Check for 5 work items created:
     - 1 Epic: "Ingest RMS requirements matrix"
     - 2 Issues: "Create REST endpoints", "Dockerize the service"
     - 2 Tasks: "Add unit tests", "Deploy to DEV environment"

### **Expected Result:**
- 5 work items imported with proper hierarchy
- All items tagged and assigned to Sprint 1

---

## **2. Environment Approvals Configuration** 🔐

### **GitHub Environments:**

1. **Navigate to Repository Settings**
   - Go to `https://github.com/msftsean/rms-demo-esri/settings`
   - Click **Environments** in left sidebar

2. **Create Production Environment**
   - Click **New environment**
   - Name: `production`
   - Click **Configure environment**

3. **Add Required Reviewers**
   - Check **Required reviewers**
   - Add `segayle@microsoft.com`
   - Set **Wait timer**: 0 minutes
   - Click **Save protection rules**

4. **Configure Deployment Branches**
   - Under **Deployment branches**
   - Select **Protected branches only**
   - This ensures only main branch can deploy to prod

### **Azure DevOps Environments:**

1. **Navigate to Environments**
   - Go to Azure DevOps → **Pipelines** → **Environments**
   - Click **Create environment**

2. **Create Production Environment**
   - Name: `production`
   - Description: "Production environment with approval gates"
   - Resource: **None** (for now)

3. **Add Approvals**
   - Click on `production` environment
   - Click **Approvals and checks**
   - Click **+** → **Approvals**
   - Add `segayle@microsoft.com` as approver
   - Set **Minimum number of approvers**: 1
   - Check **Requester cannot approve**

---

## **3. GitHub Branch Protection Rules** 🛡️

### **Configure Main Branch Protection:**

1. **Navigate to Branch Settings**
   - Go to `https://github.com/msftsean/rms-demo-esri/settings/branches`
   - Click **Add rule** for main branch

2. **Basic Protection Settings**
   ```
   Branch name pattern: main
   ☑️ Restrict pushes that create files larger than 100 MB
   ☑️ Restrict force pushes
   ☑️ Restrict deletions
   ```

3. **Pull Request Requirements**
   ```
   ☑️ Require a pull request before merging
   ☑️ Require approvals (1 reviewer minimum)
   ☑️ Dismiss stale PR approvals when new commits are pushed
   ☑️ Require review from code owners (if CODEOWNERS file exists)
   ```

4. **Status Check Requirements**
   ```
   ☑️ Require status checks to pass before merging
   ☑️ Require branches to be up to date before merging
   
   Required status checks:
   - Security Analysis
   - Build and Test
   - Container Security Scan
   ```

5. **Additional Restrictions**
   ```
   ☑️ Require conversation resolution before merging
   ☑️ Include administrators (applies rules to admins too)
   ☑️ Allow force pushes (unchecked)
   ☑️ Allow deletions (unchecked)
   ```

---

## **4. GitHub Advanced Security (GHAS)** 🔒

### **Enable Security Features:**

1. **Navigate to Security Settings**
   - Go to `https://github.com/msftsean/rms-demo-esri/settings/security_analysis`

2. **Enable Dependency Graph**
   ```
   ☑️ Dependency graph
   - Automatically enabled for public repos
   - Shows package dependencies and vulnerabilities
   ```

3. **Enable Dependabot**
   ```
   ☑️ Dependabot alerts
   ☑️ Dependabot security updates
   ☑️ Dependabot version updates
   ```

4. **Configure Dependabot** 
   - Create `.github/dependabot.yml`:
   ```yaml
   version: 2
   updates:
     - package-ecosystem: "nuget"
       directory: "/src"
       schedule:
         interval: "weekly"
     - package-ecosystem: "npm"
       directory: "/frontend"
       schedule:
         interval: "weekly"
     - package-ecosystem: "docker"
       directory: "/"
       schedule:
         interval: "weekly"
   ```

5. **Enable Code Scanning**
   ```
   ☑️ Code scanning alerts
   - Already configured via CodeQL workflow
   - Results appear in Security tab
   ```

6. **Enable Secret Scanning**
   ```
   ☑️ Secret scanning alerts
   ☑️ Push protection (prevents secret commits)
   ```

---

## **5. Monitoring Dashboards** 📊

### **GitHub Dashboard:**

1. **Repository Insights**
   - Go to **Insights** tab
   - Review **Pulse**, **Contributors**, **Traffic**
   - Monitor **Security** tab for vulnerabilities

2. **Actions Monitoring**
   - Go to **Actions** tab
   - Monitor workflow success rates
   - Set up workflow notifications

3. **Custom Dashboard** (Optional)
   - Use GitHub CLI or API to create custom views
   - Monitor PR metrics, deployment frequency

### **Azure DevOps Dashboard:**

1. **Create New Dashboard**
   - Go to **Overview** → **Dashboards**
   - Click **New Dashboard**
   - Name: "RMS Demo Monitoring"

2. **Add Essential Widgets**
   ```
   Widgets to Add:
   - Build History (last 30 builds)
   - Release Pipeline Overview
   - Test Results Trend
   - Work Item Query (Sprint progress)
   - Code Coverage
   - Pull Request Status
   ```

3. **Configure Widgets**
   
   **Build History Widget:**
   - Select your build pipeline
   - Show last 30 builds
   - Group by definition

   **Test Results Widget:**
   - Source: Build pipeline
   - Show pass/fail trends
   - Include code coverage

   **Work Item Query:**
   ```sql
   SELECT [System.Id], [System.Title], [System.State]
   FROM WorkItems
   WHERE [System.TeamProject] = @project
   AND [System.AreaPath] = 'rmsdemo'
   ORDER BY [System.Id] DESC
   ```

4. **Set Auto-Refresh**
   - Configure dashboard to refresh every 5 minutes
   - Set as team default dashboard

### **Application Performance Monitoring:**

1. **Application Insights** (if using Azure)
   - Create Application Insights resource
   - Add instrumentation key to appsettings.json
   - Monitor application performance, errors, dependencies

2. **Custom Metrics Dashboard**
   - API response times
   - Database query performance  
   - Container resource usage
   - User activity metrics

---

## **Verification Checklist** ✅

After completing all configurations:

- [ ] **Azure Boards**: 5 work items imported successfully
- [ ] **Environment Approvals**: `segayle@microsoft.com` added as prod approver
- [ ] **Branch Protection**: Main branch requires PR + status checks
- [ ] **GHAS**: All security features enabled with alerts configured
- [ ] **Dashboards**: Monitoring widgets display real-time project metrics

---

## **Support Files Reference**

The following files in this repository support the manual configuration:

- `boards-import.csv` - Work items for Azure Boards import
- `.github/workflows/ci-cd.yml` - GitHub Actions pipeline (already configured)
- `azure-pipelines.yml` - Azure DevOps pipeline (already configured)

---

## **Additional Resources**

- [Azure Boards Documentation](https://docs.microsoft.com/en-us/azure/devops/boards/)
- [GitHub Environments Documentation](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- [GitHub Advanced Security Documentation](https://docs.github.com/en/get-started/learning-about-github/about-github-advanced-security)
- [Azure DevOps Dashboards Documentation](https://docs.microsoft.com/en-us/azure/devops/report/dashboards/)

Each configuration enhances your development workflow security and visibility! 🚀
