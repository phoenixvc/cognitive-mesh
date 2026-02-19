#!/bin/bash
# launch-agent-teams.sh — Launch parallel Claude Code agent sessions for each team.
#
# Usage:
#   ./scripts/launch-agent-teams.sh [--phase N] [--team NAME] [--dry-run]
#
# Options:
#   --phase 1|2|3|4   Run only the specified phase (default: auto-detect)
#   --team NAME       Run only the named team (see list below)
#   --dry-run         Print commands without executing
#   --bg              Run sessions in background with log files
#
# Teams: foundation, reasoning, metacognitive, agency, business,
#        quality, testing, cicd, infra, orchestrator
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
        --phase)
            [[ -z "${2:-}" ]] && { echo "Error: --phase requires a value (1-4)" >&2; exit 1; }
            PHASE="$2"; shift 2 ;;
        --team)
            [[ -z "${2:-}" ]] && { echo "Error: --team requires a team name" >&2; exit 1; }
            TEAM="$2"; shift 2 ;;
        --dry-run) DRY_RUN=true; shift ;;
        --bg)      BACKGROUND=true; shift ;;
        *)         echo "Unknown option: $1" >&2; exit 1 ;;
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
        echo "  cd $(pwd) && claude \"/team-${command_name}\""
    fi
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
        testing)       launch_team "TESTING" "testing" ;;
        cicd)          launch_team "CICD" "cicd" ;;
        infra)         launch_team "INFRA" "infra" ;;
        orchestrator)
            orch_cmd="/orchestrate${PHASE:+ --phase $PHASE}"
            if $DRY_RUN; then
                echo "[DRY RUN] Would launch: claude \"${orch_cmd}\""
            elif $BACKGROUND; then
                mkdir -p "$LOG_DIR"
                log_file="$LOG_DIR/orchestrator.log"
                echo "Launching Orchestrator (log: ${log_file})..."
                nohup claude "${orch_cmd}" > "$log_file" 2>&1 &
                echo "  PID: $!"
            else
                echo "  cd $(pwd) && claude \"${orch_cmd}\""
            fi
            ;;
        *) echo "Unknown team: $TEAM" >&2; exit 1 ;;
    esac
    exit 0
fi

# If a specific phase is requested, launch teams for that phase
if [[ -n "$PHASE" ]]; then
    case "$PHASE" in
        1)
            echo "=== Phase 1: Foundation + Reasoning + Quality + CI/CD + Infra ==="
            launch_team "FOUNDATION" "foundation"
            launch_team "REASONING" "reasoning"
            launch_team "QUALITY" "quality"
            launch_team "CICD" "cicd"
            launch_team "INFRA" "infra"
            ;;
        2)
            echo "=== Phase 2: Metacognitive + Agency + Testing ==="
            launch_team "METACOGNITIVE" "metacognitive"
            launch_team "AGENCY" "agency"
            launch_team "TESTING" "testing"
            ;;
        3)
            echo "=== Phase 3: Business Apps + Testing ==="
            launch_team "BUSINESS" "business"
            launch_team "TESTING" "testing"
            ;;
        4)
            echo "=== Phase 4: Final Quality + Testing Sweep ==="
            launch_team "QUALITY" "quality"
            launch_team "TESTING" "testing"
            ;;
        *)
            echo "Unknown phase: $PHASE (expected 1-4)"; exit 1 ;;
    esac
    exit 0
fi

# Default: show usage
echo "=== Cognitive Mesh Agent Team Launcher ==="
echo ""
echo "9 Teams Available:"
echo "  Code Teams:   foundation, reasoning, metacognitive, agency, business"
echo "  Support Teams: quality, testing, cicd, infra"
echo ""
echo "Option 1 — Orchestrator (auto-detects phase, dispatches sub-agents):"
echo "  claude \"/orchestrate\""
echo ""
echo "Option 2 — Run a phase (launches multiple teams):"
echo "  $0 --phase 1    # Foundation + Reasoning + Quality + CI/CD + Infra"
echo "  $0 --phase 2    # Metacognitive + Agency + Testing"
echo "  $0 --phase 3    # Business Apps + Testing"
echo "  $0 --phase 4    # Final Quality + Testing sweep"
echo ""
echo "Option 3 — Run a single team:"
echo "  $0 --team foundation"
echo "  $0 --team testing"
echo "  $0 --team cicd"
echo "  $0 --team infra"
echo "  $0 --team orchestrator"
echo ""
echo "Options:"
echo "  --bg       Run in background with log files"
echo "  --dry-run  Print commands without executing"
