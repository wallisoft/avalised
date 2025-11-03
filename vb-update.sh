#!/bin/bash
FILE=$1
OLD="$2"
NEW="$3"

# Backup
cp "$FILE" "${FILE}.bak"

# Replace
sed -i "s|$OLD|$NEW|g" "$FILE"

echo "✓ Updated $FILE (backup: ${FILE}.bak)"
