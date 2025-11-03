#!/bin/bash
DB="visualised.db"
BACKUP_DB="visualised_backup_$(date +%s).db"

echo "🌳 Creating backup point: $BACKUP_DB"
cp $DB $BACKUP_DB

echo "Files in DB:"
sqlite3 $DB "SELECT filename, version, updated_at FROM project_files;"
echo ""
echo "To restore, run: ./db-restore.sh"
