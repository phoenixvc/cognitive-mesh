#!/bin/bash
# Script to delete branches from the remote repository
#
# ⚠️ WARNING: These branches contain working functionality!
#
# These branches were associated with PRs that were closed without merging:
# - PR #36: jules-transparency-report-logic-11997309168661708357
#   - Implements transparency report generation (JSON/Markdown)
#   - Adds Knowledge Graph integration for reasoning traces
#   - 519 lines of working, tested code
#
# - PR #31: jules-uncertainty-mitigation-strategies-10837117013490994765
#   - Implements 4 uncertainty mitigation strategies
#   - Integrates with CollaborationManager and TransparencyManager
#   - 373 lines of working, tested code
#
# ⚠️ IMPORTANT: Review ANSWER.md and BRANCH_DELETION_REPORT.md first!
# These branches contain ~900 lines of complete, tested functionality.
# Only delete if you've confirmed this functionality is not needed.

set -e  # Exit on error

echo "========================================"
echo "Branch Deletion Script"
echo "========================================"
echo ""
echo "⚠️  WARNING: These branches contain working functionality!"
echo ""
echo "The following branches will be deleted:"
echo "  1. jules-transparency-report-logic-11997309168661708357"
echo "     - Transparency report generation (JSON/Markdown formats)"
echo "     - Knowledge Graph integration"
echo "     - 519 lines of code with tests"
echo ""
echo "  2. jules-uncertainty-mitigation-strategies-10837117013490994765"
echo "     - 4 uncertainty mitigation strategies"
echo "     - CollaborationManager integration"
echo "     - 373 lines of code with tests"
echo ""
echo "Total: ~900 lines of working, tested code will be lost"
echo ""
echo "❓ Have you reviewed ANSWER.md and BRANCH_DELETION_REPORT.md?"
echo "❓ Are you sure this functionality is not needed?"
echo ""

# Confirm before deletion
read -p "Type 'DELETE' to confirm deletion (anything else to cancel): " confirmation

if [ "$confirmation" != "DELETE" ]; then
    echo ""
    echo "❌ Branch deletion cancelled."
    echo "✅ Good choice! Review the functionality before deciding."
    exit 0
fi

echo ""
echo "Deleting branches..."

# Delete first branch
echo "Deleting: jules-transparency-report-logic-11997309168661708357"
git push origin --delete jules-transparency-report-logic-11997309168661708357

# Delete second branch
echo "Deleting: jules-uncertainty-mitigation-strategies-10837117013490994765"
git push origin --delete jules-uncertainty-mitigation-strategies-10837117013490994765

echo ""
echo "✅ Both branches have been deleted from the remote repository."
echo ""
echo "Note: The functionality from these branches is now lost."
echo "If you need it later, you can find it in the closed PRs:"
echo "  - PR #36: https://github.com/JustAGhosT/cognitive-mesh/pull/36"
echo "  - PR #31: https://github.com/JustAGhosT/cognitive-mesh/pull/31"
echo ""
echo "You may also want to clean up local copies:"
echo "  git branch -D jules-transparency-report-logic-11997309168661708357"
echo "  git branch -D jules-uncertainty-mitigation-strategies-10837117013490994765"
