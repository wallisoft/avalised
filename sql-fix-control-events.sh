#!/bin/bash
DB="visualised.db"

# SQL UPDATE to patch CreateControl and add event wiring
sqlite3 $DB << 'SQLPATCH'
UPDATE project_files 
SET content = REPLACE(content,
    '        return control;
    }
}',
    '        // Wire up events for selection and drag
        control.PointerPressed += PlacedControl_PointerPressed;
        control.PointerMoved += PlacedControl_PointerMoved;
        control.PointerReleased += PlacedControl_PointerReleased;
        
        return control;
    }
    
    // PLACED CONTROL EVENTS
    private bool isDraggingControl = false;
    private Point controlDragStart;
    private double controlOrigX;
    private double controlOrigY;
    
    private void PlacedControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control || designCanvas == null) return;
        
        Console.WriteLine($"[CONTROL] Pressed: {control.GetType().Name}");
        SelectControl(control);
        
        isDraggingControl = true;
        controlDragStart = e.GetPosition(designCanvas);
        controlOrigX = Canvas.GetLeft(control);
        controlOrigY = Canvas.GetTop(control);
        
        e.Handled = true;
    }
    
    private void PlacedControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isDraggingControl || sender is not Control control || designCanvas == null) return;
        
        var currentPos = e.GetPosition(designCanvas);
        var deltaX = currentPos.X - controlDragStart.X;
        var deltaY = currentPos.Y - controlDragStart.Y;
        
        var newX = Math.Max(0, Math.Min(controlOrigX + deltaX, designCanvas.Bounds.Width - control.Bounds.Width));
        var newY = Math.Max(0, Math.Min(controlOrigY + deltaY, designCanvas.Bounds.Height - control.Bounds.Height));
        
        Canvas.SetLeft(control, newX);
        Canvas.SetTop(control, newY);
        
        if (selectionBorder != null)
        {
            Canvas.SetLeft(selectionBorder, newX);
            Canvas.SetTop(selectionBorder, newY);
        }
        
        UpdateStatus();
        e.Handled = true;
    }
    
    private void PlacedControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        isDraggingControl = false;
        Console.WriteLine("[CONTROL] Drag complete");
        e.Handled = true;
    }
}'),
    version = version + 1,
    updated_at = datetime('now')
WHERE filename = 'MainWindow.axaml.cs'
AND content LIKE '%return control;%';
SQLPATCH

echo "✓ SQL patched: Control events wired up"

# Extract updated file
sqlite3 $DB "SELECT content FROM project_files WHERE filename='MainWindow.axaml.cs';" > MainWindow.axaml.cs

# Show version
sqlite3 $DB "SELECT filename, version, updated_at FROM project_files WHERE filename='MainWindow.axaml.cs';"
