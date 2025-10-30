#!/bin/bash
# Setup Avalised - Initialize database and build project

echo "🎨 Avalised Setup"
echo ""

# Create database directory
DB_DIR=~/.config/Avalised
DB_PATH="$DB_DIR/designer.db"

mkdir -p "$DB_DIR"

# Initialize database
echo "📊 Initializing database..."
sqlite3 "$DB_PATH" < init-database.sql

if [ $? -eq 0 ]; then
    echo "   ✅ Database created: $DB_PATH"
else
    echo "   ❌ Database creation failed"
    exit 1
fi

echo ""

# Build project
echo "🔨 Building Avalised..."
dotnet build

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Setup complete!"
    echo ""
    echo "Run: dotnet run"
else
    echo ""
    echo "❌ Build failed"
    exit 1
fi
