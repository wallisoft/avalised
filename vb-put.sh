#!/bin/bash
FILE=$1
COMMENT=${2:-"Updated"}

# Backup current version
sqlite3 vb-source.db << SQL
INSERT INTO source_history (filename, content, version, comment)
SELECT filename, content, version, '$COMMENT'
FROM source_files WHERE filename='$FILE';

UPDATE source_files 
SET content = readfile('$FILE'),
    version = version + 1,
    comment = '$COMMENT'
WHERE filename='$FILE';

-- Keep only last 30 versions
DELETE FROM source_history 
WHERE filename='$FILE' 
AND version < (
    SELECT MAX(version) - 30 
    FROM source_history 
    WHERE filename='$FILE'
);
SQL
echo "✓ Stored $FILE (version backup created)"
