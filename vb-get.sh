#!/bin/bash
FILE=$1
sqlite3 vb-source.db "SELECT content FROM source_files WHERE filename='$FILE'" > "$FILE"
echo "✓ Extracted $FILE"
