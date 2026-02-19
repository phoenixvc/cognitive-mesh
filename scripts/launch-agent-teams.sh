#!/bin/bash
# launch-agent-teams.sh — Launch parallel Claude Code agent sessions for each team.
#
# Usage:
#   ./scripts/launch-agent-teams.sh [--phase N] [--team NAME] [--dry-run]
#
# Options:
#   --phase 1|2|3|4   Run only the specified phase (default: auto-detect)
#   --team NAME       Run only the named team (foundation|reasoning|metacognitive|agency|business|quality)
#   --dry-run         Print commands without executing
#   --bg              Run sessions in background with log files
#
# Prerequisites:
#   - Claude Code CLI installed (`claude` command available)
#   - Run from the repository root

set -euo pipefail
cd "$(dirname "$0")/.."

PHASE=""
TEAM=""
DRY_RUN=false
BACKGROUND=false
LOG_DIR="logs/agent-teams/$(date +%Y%m%d-%H%M%S)"

while [[ $# -gt 0 ]]; do
    case "$1" in
        --phase)   PHASE="$2"; shift 2 ;;
        --team)    TEAM="$2"; shift 2 ;;
        --dry-run) DRY_RUN=true; shift ;;
        --bg)      BACKGROUND=true; shift ;;
        *)         echo "Unknown option: $1"; exit 1 ;;
    esac
done

launch_team() {
    local team_name="$1"
    local command_name="$2"

    if $DRY_RUN; then
        echo "[DRY RUN] Would launch: claude /team-${command_name}"
        return
    fi

    if $BACKGROUND; then
        mkdir -p "$LOG_DIR"
        local log_file="$LOG_DIR/${team_name}.log"
        echo "Launching Team ${team_name} (log: ${log_file})..."
        nohup claude "/team-${command_name}" > "$log_file" 2>&1 &
        echo "  PID: $!"
    else
        echo "=== Launching Team ${team_name} ==="
        echo "Run in a separate terminal:"
        echo "  cd $(pwd) && claude \"/team-${command_name}\""
        echo ""
    fi
}

launch_orchestrator() {
    local args="${1:-}"

    if $DRY_RUN; then
        echo "[DRY RUN] Would launch: claude /orchestrate ${args}"
        return
    fi

    echo "=== Launching Orchestrator ==="
    echo "  cd $(pwd) && claude \"/orchestrate ${args}\""
    echo ""
}

# If a specific team is requested, launch just that one
if [[ -n "$TEAM" ]]; then
    case "$TEAM" in
        foundation)    launch_team "FOUNDATION" "foundation" ;;
        reasoning)     launch_team "REASONING" "reasoning" ;;
        metacognitive) launch_team "METACOGNITIVE" "metacognitive" ;;
        agency)        launch_team "AGENCY" "agency" ;;
        business)      launch_team "BUSINESS" "business" ;;
        quality)       launch_team "QUALITY" "quality" ;;
        orchestrator)  launch_orchestrator "${PHASE:+--phase $PHASE}" ;;
        *) echo "Unknown team: $TEAM"; exit 1 ;;
    esac
    exit 0
fi

# If a specific phase is requested, launch teams for that phase
if [[ -n "$PHASE" ]]; then
    case "$PHASE" in
        1)
            echo "=== Phase 1: Foundation + Reasoning + Quality (parallel) ==="
            launch_team "FOUNDATION" "foundation"
            launch_team "REASONING" "reasoning"
            launch_team "QUALITY" "quality"
            ;;
        2)
            echo "=== Phase 2: Metacognitive + Agency (parallel) ==="
            launch_team "METACOGNITIVE" "metacognitive"
            launch_team "AGENCY" "agency"
            ;;
        3)
            echo "=== Phase 3: Business Apps ==="
            launch_team "BUSINESS" "business"
            ;;
        4)
            echo "=== Phase 4: Final Quality Sweep ==="
            launch_team "QUALITY" "quality"
            ;;
        *)
            echo "Unknown phase: $PHASE (expected 1-4)"; exit 1 ;;
    esac
    exit 0
fi

# Default: launch orchestrator
echo "=== Cognitive Mesh Agent Team Launcher ==="
echo ""
echo "Option 1: Run the orchestrator (auto-detects phase, dispatches sub-agents):"
echo "  claude \"/orchestrate\""
echo ""
echo "Option 2: Run individual phases:"
echo "  $0 --phase 1          # Foundation + Reasoning + Quality"
echo "  $0 --phase 2          # Metacognitive + Agency"
echo "  $0 --phase 3          # Business Apps"
echo "  $0 --phase 4          # Final Quality Sweep"
echo ""
echo "Option 3: Run a single team:"
echo "  $0 --team foundation"
echo "  $0 --team reasoning"
echo "  $0 --team metacognitive"
echo "  $0 --team agency"
echo "  $0 --team business"
echo "  $0 --team quality"
echo ""
echo "Options:"
echo "  --bg       Run in background with log files"
echo "  --dry-run  Print commands without executing"
