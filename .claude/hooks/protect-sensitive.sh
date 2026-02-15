#!/bin/bash
# PreToolUse hook: Block writes to sensitive files.
# Exit 2 = block the operation. Exit 0 = allow.
# Fail-closed: if jq is missing or parsing fails, the hook blocks the operation.

if ! command -v jq &>/dev/null; then
    echo "BLOCKED: jq is required for the protect-sensitive hook but is not installed." >&2
    exit 2
fi

INPUT=$(cat)

FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')
JQ_EXIT=$?
if [ $JQ_EXIT -ne 0 ]; then
    echo "BLOCKED: Failed to parse tool input (jq exit code $JQ_EXIT). Refusing to proceed." >&2
    exit 2
fi

if [ -z "$FILE_PATH" ]; then
    exit 0
fi

# Block patterns
BLOCKED_PATTERNS=(
    ".env"
    "appsettings.Production.json"
    "appsettings.Staging.json"
    "secrets.json"
    ".pfx"
    ".key"
    ".pem"
    "credentials"
    ".azure/config"
)

for pattern in "${BLOCKED_PATTERNS[@]}"; do
    if [[ "$FILE_PATH" == *"$pattern"* ]]; then
        echo "BLOCKED: Cannot modify sensitive file: $FILE_PATH" >&2
        exit 2
    fi
done

exit 0
