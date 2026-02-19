#!/bin/bash
# PreToolUse hook: Block destructive Bash commands that cannot be undone.
# Exit 2 = block the operation. Exit 0 = allow.
# Targets: git force-push, git reset --hard, rm -rf outside project, git clean.

export PATH="/usr/bin:/bin:/usr/local/bin:$HOME/.local/bin:$PATH"

if ! command -v jq &>/dev/null; then
    echo "WARNING: jq is not available; guard-destructive-bash hook cannot inspect commands." >&2
    exit 0
fi

INPUT=$(cat)

COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')
JQ_EXIT=$?
if [ $JQ_EXIT -ne 0 ]; then
    echo "WARNING: Failed to parse tool input (jq exit code $JQ_EXIT); allowing command." >&2
    exit 0
fi

if [ -z "$COMMAND" ]; then
    exit 0
fi

# Strip heredoc content so commit messages don't trigger false positives.
# Remove everything between <<'EOF' ... EOF (and <<EOF ... EOF variants).
COMMAND_STRIPPED=$(echo "$COMMAND" | sed '/<<.*EOF/,/^EOF[[:space:]]*$/d; /<<.*HEREDOC/,/^HEREDOC[[:space:]]*$/d')

# Destructive git operations
BLOCKED_GIT_PATTERNS=(
    "git push --force"
    "git push -f"
    "git reset --hard"
    "git clean -f"
    "git clean -df"
    "git clean -fd"
    "git checkout -- ."
    "git checkout ."
    "git restore ."
    "git branch -D"
)

for pattern in "${BLOCKED_GIT_PATTERNS[@]}"; do
    # Escape dots for literal regex matching
    regex=$(printf '%s' "$pattern" | sed 's/\./\\./g')
    # Match pattern followed by whitespace or end-of-string (flag boundary)
    # so e.g. --force does not false-positive on --force-with-lease
    if [[ "$COMMAND_STRIPPED" =~ $regex([[:space:]]|$) ]]; then
        echo "BLOCKED: Destructive git operation detected: '$pattern'. This action is irreversible. Ask the user for explicit confirmation first." >&2
        exit 2
    fi
done

# Block rm -rf on paths outside the project
if [[ "$COMMAND_STRIPPED" == *"rm -rf /"* ]] || [[ "$COMMAND_STRIPPED" == *"rm -rf ~"* ]]; then
    echo "BLOCKED: Cannot rm -rf outside the project directory." >&2
    exit 2
fi

exit 0
