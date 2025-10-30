#!/bin/bash
# ═══════════════════════════════════════════════════════════════════
# Avalised GitHub Setup Script
# Run this from your avalised project directory
# ═══════════════════════════════════════════════════════════════════

set -e  # Exit on error

REPO_NAME="avalised"
GITHUB_USER="wallisoft"  # Change this to your GitHub username
REPO_URL="git@github.com:${GITHUB_USER}/${REPO_NAME}.git"

echo "╔════════════════════════════════════════════════════════════════╗"
echo "║  Avalised GitHub Setup                                         ║"
echo "║  Initializing git repository and preparing first push          ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

# Check if we're in the right directory
if [ ! -f "PROJECT-STATUS.yaml" ]; then
    echo "❌ Error: PROJECT-STATUS.yaml not found!"
    echo "   Please run this script from the avalised project directory."
    exit 1
fi

echo "📂 Current directory: $(pwd)"
echo ""

# Initialize git if not already done
if [ ! -d ".git" ]; then
    echo "🔧 Initializing git repository..."
    git init
    git branch -M main
    echo "✅ Git initialized with main branch"
else
    echo "✅ Git repository already initialized"
fi

echo ""
echo "📝 Adding files to git..."
git add .
echo "✅ Files staged"

echo ""
echo "💾 Creating initial commit..."
git commit -m "🎉 Initial commit - Avalised V0.9.5

Features:
- AVMLLoader: Recursive YAML → UI loader
- AVMLExporter: UI → YAML export  
- DesignerLayout: Full visual designer with drag/drop/resize
- Test files proving AVML format works

Next: Create visual-designer.avml for self-loading!"
echo "✅ Initial commit created"

echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "  READY FOR GITHUB!"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "Next steps:"
echo ""
echo "1. Create repository on GitHub:"
echo "   - Go to https://github.com/new"
echo "   - Repository name: ${REPO_NAME}"
echo "   - Description: The Visual Designer That Designs Itself"
echo "   - Make it PUBLIC (or private if preferred)"
echo "   - DON'T initialize with README (we have one!)"
echo ""
echo "2. After creating the repo on GitHub, run:"
echo ""
echo "   git remote add origin ${REPO_URL}"
echo "   git push -u origin main"
echo ""
echo "3. Your project will be live at:"
echo "   https://github.com/${GITHUB_USER}/${REPO_NAME}"
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "🎊 Git repository ready for GitHub!"
echo ""

# Show what will be committed
echo "Files to be pushed:"
git ls-files | head -20
FILE_COUNT=$(git ls-files | wc -l)
if [ $FILE_COUNT -gt 20 ]; then
    echo "... and $((FILE_COUNT - 20)) more files"
fi

echo ""
echo "📊 Repository stats:"
git log --oneline | wc -l | xargs echo "  Commits:"
git ls-files | wc -l | xargs echo "  Files:"
du -sh .git | cut -f1 | xargs echo "  Size:"

echo ""
echo "✨ All done! Create your GitHub repo and push!"
