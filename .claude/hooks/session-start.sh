#!/bin/bash
# SessionStart hook: Verify .NET environment and build state on session start.
# Output goes to Claude's context so it knows the current project state.

set -e
cd "$CLAUDE_PROJECT_DIR" 2>/dev/null || cd "$(dirname "$0")/../.." 2>/dev/null || {
    echo "Failed to change directory to project dir" >&2
    exit 1
}

echo "=== Cognitive Mesh Session Start ==="

# Ensure PATH includes user-local install directories
export PATH="$HOME/.dotnet:$HOME/.local/bin:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

# Detect OS and architecture for platform-specific downloads
KERNEL=$(uname -s)
MACHINE=$(uname -m)

case "$KERNEL" in
    Linux)  JQ_OS="linux"; GH_OS="linux" ;;
    Darwin) JQ_OS="macos"; GH_OS="macOS" ;;
    *)      JQ_OS=""; GH_OS=""
            echo "WARNING: Unsupported OS '$KERNEL'. Skipping binary installs." ;;
esac

case "$MACHINE" in
    x86_64|amd64)   JQ_ARCH="amd64"; GH_ARCH="amd64" ;;
    aarch64|arm64)   JQ_ARCH="arm64"; GH_ARCH="arm64" ;;
    *)               JQ_ARCH=""; GH_ARCH=""
                     echo "WARNING: Unsupported architecture '$MACHINE'. Skipping binary installs." ;;
esac

# Install jq if missing (needed by protect-sensitive.sh hook)
if ! command -v jq &>/dev/null; then
    if [ -n "$JQ_OS" ] && [ -n "$JQ_ARCH" ]; then
        echo "Installing jq for ${JQ_OS}-${JQ_ARCH}..."
        mkdir -p "$HOME/.local/bin"
        curl -fsSL "https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-${JQ_OS}-${JQ_ARCH}" -o "$HOME/.local/bin/jq" \
            && chmod +x "$HOME/.local/bin/jq" \
            && echo "Tools: jq installed" \
            || echo "WARNING: jq installation failed."
    else
        echo "WARNING: Cannot install jq — unsupported platform (${KERNEL}/${MACHINE})."
    fi
fi

# Install .NET 9 SDK if missing
if ! command -v dotnet &>/dev/null; then
    echo "Installing .NET 9 SDK..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0 2>&1
fi

if command -v dotnet &>/dev/null; then
    SDK_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    echo "SDK: .NET $SDK_VERSION"
else
    echo "WARNING: dotnet SDK installation failed."
    exit 0
fi

# Install GitHub CLI if missing
if ! command -v gh &>/dev/null; then
    if [ -n "$GH_OS" ] && [ -n "$GH_ARCH" ]; then
        echo "Installing GitHub CLI for ${GH_OS}-${GH_ARCH}..."
        GH_VERSION=$(curl -fsSL https://api.github.com/repos/cli/cli/releases/latest | grep '"tag_name"' | sed 's/.*"v\(.*\)".*/\1/')
        if [ -n "$GH_VERSION" ]; then
            GH_ARCHIVE="gh_${GH_VERSION}_${GH_OS}_${GH_ARCH}.tar.gz"
            GH_DIR="gh_${GH_VERSION}_${GH_OS}_${GH_ARCH}"
            curl -fsSL "https://github.com/cli/cli/releases/download/v${GH_VERSION}/${GH_ARCHIVE}" -o "/tmp/${GH_ARCHIVE}" \
                && mkdir -p "$HOME/.local/bin" \
                && tar -xzf "/tmp/${GH_ARCHIVE}" -C /tmp \
                && cp "/tmp/${GH_DIR}/bin/gh" "$HOME/.local/bin/gh" \
                && chmod +x "$HOME/.local/bin/gh" \
                && rm -rf "/tmp/${GH_ARCHIVE}" "/tmp/${GH_DIR}"
        fi
    else
        echo "WARNING: Cannot install GitHub CLI — unsupported platform (${KERNEL}/${MACHINE})."
    fi
fi

if command -v gh &>/dev/null; then
    echo "CLI: gh $(gh --version | head -1 | awk '{print $3}')"
else
    echo "WARNING: GitHub CLI installation failed."
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
