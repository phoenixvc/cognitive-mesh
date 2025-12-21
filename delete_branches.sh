#!/bin/bash
# Script to delete unnecessary branches from the remote repository
# 
# These branches were associated with PRs that were closed without merging:
# - PR #36: jules-transparency-report-logic-11997309168661708357
# - PR #31: jules-uncertainty-mitigation-strategies-10837117013490994765
#
# Both branches are safe to delete as they contain no merged code.

set -e  # Exit on error

echo "Branch Deletion Script"
echo "====================="
echo ""
echo "This script will delete the following branches from the remote repository:"
echo "  1. jules-transparency-report-logic-11997309168661708357"
echo "  2. jules-uncertainty-mitigation-strategies-10837117013490994765"
echo ""

# Confirm before deletion
read -p "Are you sure you want to delete these branches? (yes/no): " confirmation

if [ "$confirmation" != "yes" ]; then
    echo "Branch deletion cancelled."
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
echo "✅ Both branches have been successfully deleted from the remote repository."
echo ""
echo "You may also want to clean up local copies of these branches:"
echo "  git branch -D jules-transparency-report-logic-11997309168661708357"
echo "  git branch -D jules-uncertainty-mitigation-strategies-10837117013490994765"
