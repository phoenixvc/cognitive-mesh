#!/bin/bash
# SessionStart hook: Verify .NET environment and build state on session start.
# Output goes to Claude's context so it knows the current project state.

set -e
cd "$CLAUDE_PROJECT_DIR" 2>/dev/null || cd "$(dirname "$0")/../.."

echo "=== Cognitive Mesh Session Start ==="

# Check .NET SDK
if command -v dotnet &>/dev/null; then
    SDK_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    echo "SDK: .NET $SDK_VERSION"
else
    echo "WARNING: dotnet SDK not found. Install .NET 9 SDK."
    exit 0
fi

# Restore packages (quiet mode)
echo "Restoring packages..."
if dotnet restore CognitiveMesh.sln --verbosity quiet 2>&1; then
    echo "Packages: OK"
else
    echo "WARNING: Package restore had issues. Run 'dotnet restore' manually."
fi

# Quick build check
echo "Building..."
BUILD_OUTPUT=$(dotnet build CognitiveMesh.sln --no-restore --verbosity quiet 2>&1 || true)
if echo "$BUILD_OUTPUT" | grep -q "Build succeeded"; then
    WARN_COUNT=$(echo "$BUILD_OUTPUT" | grep -E -o '[0-9]+ Warning' | head -1 || echo "0")
    echo "Build: PASSED ($WARN_COUNT)"
else
    ERROR_LINES=$(echo "$BUILD_OUTPUT" | grep -E "error [A-Z]+[0-9]+" | head -5)
    echo "Build: FAILED"
    if [ -n "$ERROR_LINES" ]; then
        echo "Errors:"
        echo "$ERROR_LINES"
    fi
fi

# Git status summary
BRANCH=$(git branch --show-current 2>/dev/null || echo "unknown")
CHANGED=$(git status --porcelain 2>/dev/null | wc -l | tr -d ' ')
echo "Git: branch=$BRANCH, uncommitted=$CHANGED"

echo "=== Ready ==="
