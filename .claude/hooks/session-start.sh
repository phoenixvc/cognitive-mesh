#!/bin/bash
# SessionStart hook: Ensure all required tools are available and the project builds.
# Output goes to Claude's context so it knows the current project state.
#
# Required tools for this repo:
#   - dotnet (.NET 9 SDK) — build, test, restore
#   - git                 — version control
#   - gh                  — GitHub CLI for PR/issue workflows
#   - jq                  — JSON processing (used by protect-sensitive.sh hook)
#   - rg (ripgrep)        — fast code search (used by Claude Code's Grep tool)
#   - curl, tar           — downloading tools
#   - grep, find, ls, etc — standard Unix utilities

# ─── PATH SETUP ──────────────────────────────────────────────────────────────
# CRITICAL: Ensure system bin directories are in PATH FIRST.
# The Claude Code web environment may not include /usr/bin or /bin in PATH,
# which breaks every standard Unix tool (grep, find, ls, curl, git, etc.).
export PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:${HOME}/.dotnet:${HOME}/.local/bin:${PATH}"
export DOTNET_ROOT="${HOME}/.dotnet"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

# Bail on unrecoverable errors, but handle individual tool failures gracefully
set -o pipefail

# Navigate to project directory
if [ -n "$CLAUDE_PROJECT_DIR" ]; then
    cd "$CLAUDE_PROJECT_DIR" || exit 1
elif [ -d "$(dirname "$0")/../.." ]; then
    cd "$(dirname "$0")/../.." || exit 1
else
    echo "ERROR: Cannot determine project directory" >&2
    exit 1
fi

echo "=== Cognitive Mesh Session Start ==="

# ─── VERIFY CORE UNIX TOOLS ─────────────────────────────────────────────────
MISSING_CORE=""
for tool in grep find ls curl tar git tr head wc awk sed; do
    if ! command -v "$tool" &>/dev/null; then
        MISSING_CORE="$MISSING_CORE $tool"
    fi
done
if [ -n "$MISSING_CORE" ]; then
    echo "WARNING: Core tools missing:${MISSING_CORE}"
    echo "PATH=$PATH"
fi

# ─── INSTALL .NET 9 SDK ─────────────────────────────────────────────────────
if ! command -v dotnet &>/dev/null; then
    echo "Installing .NET 9 SDK..."
    if command -v curl &>/dev/null && command -v bash &>/dev/null; then
        if curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh 2>/dev/null; then
            bash /tmp/dotnet-install.sh --channel 9.0 --install-dir "${HOME}/.dotnet" 2>&1 | tail -5
            rm -f /tmp/dotnet-install.sh
            # Re-export in case the installer changed something
            export PATH="${HOME}/.dotnet:${PATH}"
        else
            echo "WARNING: Failed to download .NET install script."
        fi
    else
        echo "WARNING: curl or bash not available — cannot install .NET SDK."
    fi
fi

if command -v dotnet &>/dev/null; then
    SDK_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    echo "SDK: .NET ${SDK_VERSION}"
else
    echo "WARNING: .NET SDK not available. Build/test/restore commands will fail."
    echo "  Manual install: curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0"
fi

# ─── INSTALL GITHUB CLI ─────────────────────────────────────────────────────
if ! command -v gh &>/dev/null; then
    echo "Installing GitHub CLI..."
    ARCH=$(uname -m 2>/dev/null)
    case "$ARCH" in
        x86_64|amd64)  GH_ARCH="amd64" ;;
        aarch64|arm64) GH_ARCH="arm64" ;;
        *)             GH_ARCH="" ;;
    esac

    if [ -n "$GH_ARCH" ] && command -v curl &>/dev/null && command -v tar &>/dev/null; then
        GH_VERSION=$(curl -fsSL https://api.github.com/repos/cli/cli/releases/latest 2>/dev/null \
            | grep '"tag_name"' | sed 's/.*"v\(.*\)".*/\1/')
        if [ -n "$GH_VERSION" ]; then
            GH_ARCHIVE="gh_${GH_VERSION}_linux_${GH_ARCH}.tar.gz"
            GH_DIR="gh_${GH_VERSION}_linux_${GH_ARCH}"
            mkdir -p "${HOME}/.local/bin"
            if curl -fsSL "https://github.com/cli/cli/releases/download/v${GH_VERSION}/${GH_ARCHIVE}" -o "/tmp/${GH_ARCHIVE}" 2>/dev/null; then
                tar -xzf "/tmp/${GH_ARCHIVE}" -C /tmp 2>/dev/null \
                    && cp "/tmp/${GH_DIR}/bin/gh" "${HOME}/.local/bin/gh" \
                    && chmod +x "${HOME}/.local/bin/gh" \
                    && echo "Tools: gh ${GH_VERSION} installed"
                rm -rf "/tmp/${GH_ARCHIVE}" "/tmp/${GH_DIR}"
            else
                echo "WARNING: Failed to download GitHub CLI."
            fi
        fi
    fi
fi

if command -v gh &>/dev/null; then
    echo "CLI: gh $(gh --version 2>/dev/null | head -1 | awk '{print $3}')"
fi

# ─── VERIFY JQ (needed by protect-sensitive.sh hook) ─────────────────────────
if ! command -v jq &>/dev/null; then
    echo "Installing jq..."
    ARCH=$(uname -m 2>/dev/null)
    case "$ARCH" in
        x86_64|amd64)  JQ_ARCH="amd64" ;;
        aarch64|arm64) JQ_ARCH="arm64" ;;
        *)             JQ_ARCH="" ;;
    esac

    if [ -n "$JQ_ARCH" ] && command -v curl &>/dev/null; then
        mkdir -p "${HOME}/.local/bin"
        curl -fsSL "https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-linux-${JQ_ARCH}" \
            -o "${HOME}/.local/bin/jq" 2>/dev/null \
            && chmod +x "${HOME}/.local/bin/jq" \
            && echo "Tools: jq installed" \
            || echo "WARNING: jq installation failed."
    fi
fi

# ─── VERIFY RIPGREP (needed by Claude Code's Grep tool) ─────────────────────
if ! command -v rg &>/dev/null; then
    echo "Installing ripgrep..."
    ARCH=$(uname -m 2>/dev/null)
    case "$ARCH" in
        x86_64|amd64)  RG_ARCH="x86_64-unknown-linux-musl" ;;
        aarch64|arm64) RG_ARCH="aarch64-unknown-linux-gnu" ;;
        *)             RG_ARCH="" ;;
    esac

    if [ -n "$RG_ARCH" ] && command -v curl &>/dev/null && command -v tar &>/dev/null; then
        RG_VERSION=$(curl -fsSL https://api.github.com/repos/BurntSushi/ripgrep/releases/latest 2>/dev/null \
            | grep '"tag_name"' | sed 's/.*"\(.*\)".*/\1/')
        if [ -n "$RG_VERSION" ]; then
            RG_DIR="ripgrep-${RG_VERSION}-${RG_ARCH}"
            mkdir -p "${HOME}/.local/bin"
            curl -fsSL "https://github.com/BurntSushi/ripgrep/releases/download/${RG_VERSION}/${RG_DIR}.tar.gz" \
                -o "/tmp/rg.tar.gz" 2>/dev/null \
                && tar -xzf /tmp/rg.tar.gz -C /tmp 2>/dev/null \
                && cp "/tmp/${RG_DIR}/rg" "${HOME}/.local/bin/rg" \
                && chmod +x "${HOME}/.local/bin/rg" \
                && echo "Tools: ripgrep ${RG_VERSION} installed"
            rm -rf "/tmp/rg.tar.gz" "/tmp/${RG_DIR}"
        fi
    fi
fi

# ─── RESTORE + BUILD ────────────────────────────────────────────────────────
if command -v dotnet &>/dev/null; then
    echo "Restoring packages..."
    if dotnet restore CognitiveMesh.sln --verbosity quiet 2>&1; then
        echo "Packages: OK"
    else
        echo "WARNING: Package restore had issues. Run 'dotnet restore' manually."
    fi

    echo "Building..."
    BUILD_OUTPUT=$(dotnet build CognitiveMesh.sln --no-restore --verbosity quiet 2>&1 || true)
    if echo "$BUILD_OUTPUT" | grep -q "Build succeeded"; then
        WARN_COUNT=$(echo "$BUILD_OUTPUT" | grep -Eo '[0-9]+ Warning' | head -1 || echo "0")
        echo "Build: PASSED (${WARN_COUNT})"
    else
        ERROR_LINES=$(echo "$BUILD_OUTPUT" | grep -E "error [A-Z]+[0-9]+" | head -5)
        echo "Build: FAILED"
        if [ -n "$ERROR_LINES" ]; then
            echo "Errors:"
            echo "$ERROR_LINES"
        fi
    fi
fi

# ─── GIT STATUS ─────────────────────────────────────────────────────────────
if command -v git &>/dev/null; then
    BRANCH=$(git branch --show-current 2>/dev/null || echo "unknown")
    CHANGED=$(git status --porcelain 2>/dev/null | wc -l | tr -d ' ')
    echo "Git: branch=${BRANCH}, uncommitted=${CHANGED}"
fi

# ─── TOOL SUMMARY ───────────────────────────────────────────────────────────
echo "Tools:"
for tool in dotnet git gh jq rg curl make; do
    if command -v "$tool" &>/dev/null; then
        printf "  %-8s OK\n" "$tool"
    else
        printf "  %-8s MISSING\n" "$tool"
    fi
done

echo "=== Ready ==="
