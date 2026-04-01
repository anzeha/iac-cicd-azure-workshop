#!/bin/bash

set -euo pipefail

# Script to update Helm chart image versions and optionally create a commit
# Usage: ./update-version.sh <version> [--values-file <path>] [--no-commit]

VERSION="${1:-}"
VALUES_FILE=""
SKIP_COMMIT=false

if [[ -z "$VERSION" ]]; then
  echo "Error: Version parameter is required"
  echo "Usage: $0 <version> [--values-file <path>] [--no-commit]"
  echo ""
  echo "Examples:"
  echo "  $0 1.2.3                                                # Update to v1.2.3 and commit (default path)"
  echo "  $0 1.2.3 --values-file helm/workshop-app/values.yaml   # Update specific file and commit"
  echo "  $0 1.2.3 --no-commit                                    # Update to v1.2.3 without committing"
  echo "  $0 1.2.3 --values-file helm/workshop-app/values.yaml --no-commit  # Update specific file without committing"
  exit 1
fi

# Parse optional arguments
shift
while [[ $# -gt 0 ]]; do
  case "$1" in
    --values-file)
      VALUES_FILE="$2"
      shift 2
      ;;
    --no-commit)
      SKIP_COMMIT=true
      shift
      ;;
    *)
      echo "Error: Unknown argument: $1"
      exit 1
      ;;
  esac
done

# If values file not provided, use default path
if [[ -z "$VALUES_FILE" ]]; then
  SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
  VALUES_FILE="$REPO_ROOT/helm/workshop-app/values.yaml"
fi

if [[ ! -f "$VALUES_FILE" ]]; then
  echo "Error: values.yaml not found at $VALUES_FILE"
  exit 1
fi

echo "Updating image tags to version: $VERSION"

# Update the API image tag (images.api.tag)
sed -i.bak "/^  api:$/,/^  [a-z]/ s/^\(    tag: \).*/\1$VERSION/" "$VALUES_FILE"

# Update the consumer image tag (images.consumer.tag)
# Stop at next top-level section to avoid updating kafkaUi tags
sed -i.bak "/^  consumer:$/,/^[a-z]/ s/^\(    tag: \).*/\1$VERSION/" "$VALUES_FILE"

# Remove backup file
rm -f "$VALUES_FILE.bak"

echo "✓ Updated API image tag to: $VERSION"
echo "✓ Updated consumer image tag to: $VERSION"

# Verify the changes
echo ""
echo "Verification:"
grep -A 3 "^  api:" "$VALUES_FILE" | grep "tag:"
grep -A 3 "^  consumer:" "$VALUES_FILE" | grep "tag:"

if [[ "$SKIP_COMMIT" == false ]]; then
  echo ""
  echo "Creating commit..."
  
  # Determine repo root from values file location
  REPO_ROOT="$(cd "$(dirname "$VALUES_FILE")/../.." && pwd)"
  cd "$REPO_ROOT"
  
  if git diff --quiet "$VALUES_FILE" 2>/dev/null; then
    echo "ℹ No changes detected in values.yaml"
  else
    git add "$VALUES_FILE"
    git commit -m "chore: [skip ci] bump image versions to $VERSION"
    echo "✓ Commit created with message: 'chore: [skip ci] bump image versions to $VERSION'"
  fi
else
  echo ""
  echo "✓ Skipped git commit (--no-commit flag was set)"
fi

echo ""
echo "Done!"
