#!/usr/bin/env bash
set -euo pipefail

# Prereqs: az devops extension installed, authenticated; git remote 'origin' is GitHub
# Usage: ./setup.sh <azure_org_url> <project_name>
# Example: ./setup.sh https://dev.azure.com/seanbox rmsdemo

AZ_ORG="${1:-}"
AZ_PROJECT="${2:-}"

if [[ -z "$AZ_ORG" || -z "$AZ_PROJECT" ]]; then
  echo "Usage: $0 <azure_org_url> <project_name>"
  exit 1
fi

az devops configure --defaults organization="$AZ_ORG"
if ! az devops project show --project "$AZ_PROJECT" >/dev/null 2>&1; then
  az devops project create --name "$AZ_PROJECT"
fi
az devops configure --defaults project="$AZ_PROJECT"

# Create Azure DevOps repo and mirror push
REPO_NAME="$(basename "$(git rev-parse --show-toplevel)")"
if ! az repos show --repository "$REPO_NAME" >/dev/null 2>&1; then
  az repos create --name "$REPO_NAME" >/dev/null
fi
AZ_URL="$(az repos show --repository "$REPO_NAME" --query webUrl -o tsv | sed 's/_git\//_git\//')"
GIT_PUSH_URL="$(az repos show --repository "$REPO_NAME" --query 'remoteUrl' -o tsv)"

git remote remove azure 2>/dev/null || true
git remote add azure "$GIT_PUSH_URL"
git push azure main:main --force

echo "Azure DevOps repo: $AZ_URL"
echo "Done."
