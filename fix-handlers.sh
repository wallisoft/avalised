#!/bin/bash
DB="visualised.db"

# Get current content
sqlite3 $DB "SELECT content FROM project_files WHERE filename='MainWindow.axaml.cs';" > /tmp/current.cs

# Use awk to restore handler bodies
awk '
/private void HandleSelectAll\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleSelectAll(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Select All NYI\"; }"
    next
}
/private void HandlePropertiesWindow\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandlePropertiesWindow(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Toggle Props\"; }"
    next
}
/private void HandleToolboxWindow\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleToolboxWindow(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Toggle Toolbox\"; }"
    next
}
/private void HandleZoomIn\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleZoomIn(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Zoom In\"; }"
    next
}
/private void HandleZoomOut\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleZoomOut(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Zoom Out\"; }"
    next
}
/private void HandleZoomReset\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleZoomReset(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Zoom Reset\"; }"
    next
}
/private void HandleAlignLeft\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleAlignLeft(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Align Left\"; }"
    next
}
/private void HandleAlignCenter\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleAlignCenter(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Align Center\"; }"
    next
}
/private void HandleAlignRight\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleAlignRight(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Align Right\"; }"
    next
}
/private void HandleAlignTop\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleAlignTop(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Align Top\"; }"
    next
}
/private void HandleAlignMiddle\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleAlignMiddle(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Align Middle\"; }"
    next
}
/private void HandleAlignBottom\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleAlignBottom(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Align Bottom\"; }"
    next
}
/private async void HandleOptions\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private async void HandleOptions(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Options NYI\"; }"
    next
}
/private void HandleDocumentation\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleDocumentation(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"Docs NYI\"; }"
    next
}
/private void HandleVMLReference\(object\? sender, RoutedEventArgs e\) \{ \}/ {
    print "    private void HandleVMLReference(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = \"VML Ref NYI\"; }"
    next
}
{ print }
' /tmp/current.cs > /tmp/patched.cs

# Update DB
CONTENT=$(cat /tmp/patched.cs | sed "s/'/''/g")
TIMESTAMP=$(date -u +"%Y-%m-%d %H:%M:%S")

sqlite3 $DB << SQL
UPDATE project_files 
SET content = '$CONTENT',
    updated_at = '$TIMESTAMP',
    version = version + 1
WHERE filename = 'MainWindow.axaml.cs';
SQL

# Extract to file
sqlite3 $DB "SELECT content FROM project_files WHERE filename='MainWindow.axaml.cs';" > MainWindow.axaml.cs

echo "✓ Handlers patched via SQL"
sqlite3 $DB "SELECT filename, version FROM project_files WHERE filename='MainWindow.axaml.cs';"
