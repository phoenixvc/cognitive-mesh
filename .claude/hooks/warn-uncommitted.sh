#!/bin/bash
# PostToolUse hook: After file edits, remind about uncommitted changes.
# Non-blocking — informational only (always exits 0).

export PATH="/usr/bin:/bin:/usr/local/bin:$HOME/.local/bin:$PATH"

cd "$CLAUDE_PROJECT_DIR" 2>/dev/null || exit 0

if ! command -v git &>/dev/null; then
    exit 0
fi

CHANGED=$(git status --porcelain 2>/dev/null | wc -l | tr -d ' ')

if [ "$CHANGED" -ge 10 ]; then
    echo "WARNING: $CHANGED uncommitted changes. Consider committing to avoid losing work if context is shortened." >&2
fi

exit 0
