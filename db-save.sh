#!/bin/bash
DB="visualised.db"

# Create table if not exists
sqlite3 $DB << SQL
CREATE TABLE IF NOT EXISTS project_files (
    filename TEXT PRIMARY KEY,
    content TEXT NOT NULL,
    file_type TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    version INTEGER DEFAULT 1
);
SQL

# Function to save file
save_file() {
    local file=$1
    local type=$2
    local content=$(cat "$file" 2>/dev/null | sed "s/'/''/g")
    local timestamp=$(date -u +"%Y-%m-%d %H:%M:%S")
    
    sqlite3 $DB << SQL
INSERT INTO project_files (filename, content, file_type, updated_at, version)
VALUES ('$file', '$content', '$type', '$timestamp', 1)
ON CONFLICT(filename) DO UPDATE SET
    content = '$content',
    updated_at = '$timestamp',
    version = version + 1;
SQL
}

echo "🌳 Saving project state..."
save_file "MainWindow.axaml.cs" "cs"
save_file "designer.vml" "vml"
save_file "DesignerWindow.cs" "cs"

echo "✓ Saved to $DB"
sqlite3 $DB "SELECT filename, version, updated_at FROM project_files;"
