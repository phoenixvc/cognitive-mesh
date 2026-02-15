#!/bin/bash
# Stop hook: Verify build still succeeds before Claude finishes responding.
# Catches regressions introduced during the conversation.

cd "$CLAUDE_PROJECT_DIR" 2>/dev/null || cd "$(dirname "$0")/../.." 2>/dev/null || {
    echo "Failed to change to project directory" >&2
    exit 1
}

BUILD_OUTPUT=$(dotnet build CognitiveMesh.sln --no-restore --verbosity quiet 2>&1 || true)
if echo "$BUILD_OUTPUT" | grep -q "Build succeeded"; then
    echo "Build check: PASSED"
else
    ERROR_COUNT=$(echo "$BUILD_OUTPUT" | grep -E "error [A-Z]+[0-9]+" | wc -l)
    echo "Build check: FAILED ($ERROR_COUNT errors)"
    echo "Fix build errors before finishing."
    echo "$BUILD_OUTPUT" | grep -E "error [A-Z]+[0-9]+" | head -10
fi
