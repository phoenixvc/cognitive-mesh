#!/bin/bash
# PreToolUse hook: Block writes to sensitive files.
# Exit 2 = block the operation. Exit 0 = allow.

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty' 2>/dev/null)

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
