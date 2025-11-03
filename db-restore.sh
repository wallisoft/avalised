#!/bin/bash
DB="visualised.db"

restore_file() {
    local file=$1
    sqlite3 $DB "SELECT content FROM project_files WHERE filename='$file';" > "$file"
    echo "✓ Restored $file"
}

echo "🌳 Restoring from DB..."
restore_file "MainWindow.axaml.cs"
restore_file "designer.vml"
restore_file "DesignerWindow.cs"
echo "✓ All files restored"
