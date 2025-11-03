#!/bin/bash
DB="visualised.db"

# SQL UPDATE: Replace real Button with dummy Border for toolbox
sqlite3 $DB << 'PATCH1'
UPDATE project_files 
SET content = REPLACE(content,
    '    private Button CreateToolboxButton(string controlType)
    {
        var btn = new Button
        {
            Content = controlType,
            Tag = controlType,
            Margin = new Avalonia.Thickness(0, 2),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Avalonia.Thickness(8, 4),
            Background = Brushes.White,
            Foreground = Brushes.Black,
            BorderBrush = new SolidColorBrush(Color.Parse("#66bb6a")),
            BorderThickness = new Avalonia.Thickness(3),
            CornerRadius = new Avalonia.CornerRadius(2)
        };
        
        btn.PointerPressed += (s, e) => {
            Console.WriteLine($"[TOOLBOX] Button pressed: {controlType}");
            StartToolboxDrag(controlType, btn);
            e.Handled = true;
        };
        
        return btn;
    }',
    '    private Border CreateToolboxButton(string controlType)
    {
        var dummy = new Border
        {
            Tag = controlType,
            Margin = new Avalonia.Thickness(0, 2),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Padding = new Avalonia.Thickness(8, 4),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#66bb6a")),
            BorderThickness = new Avalonia.Thickness(3),
            CornerRadius = new Avalonia.CornerRadius(2),
            Cursor = new Cursor(StandardCursorType.Hand),
            Child = new TextBlock 
            { 
                Text = controlType,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brushes.Black
            }
        };
        
        dummy.PointerPressed += (s, e) => {
            Console.WriteLine($"[TOOLBOX] Dummy pressed: {controlType}");
            StartToolboxDrag(controlType, dummy);
            e.Handled = true;
        };
        
        return dummy;
    }'),
    version = version + 1,
    updated_at = datetime('now')
WHERE filename = 'MainWindow.axaml.cs';
PATCH1

# SQL UPDATE: Change CreateControl to return dummy ghost instead of real control
sqlite3 $DB << 'PATCH2'
UPDATE project_files 
SET content = REPLACE(content,
    '    private Control? CreateControl(string? typeName)
    {
        if (typeName == null) return null;
        
        var fullTypeName = $"Avalonia.Controls.{typeName}, Avalonia.Controls";
        var type = Type.GetType(fullTypeName);
        
        if (type == null || !typeof(Control).IsAssignableFrom(type)) return null;
        
        var control = Activator.CreateInstance(type) as Control;
        if (control == null) return null;
        
        if (control is Button btn) { btn.Content = "Button"; btn.Width = 100; btn.Height = 30; }
        else if (control is TextBlock tb) { tb.Text = "TextBlock"; }
        else if (control is TextBox txtBox) { txtBox.Width = 150; txtBox.Text = "TextBox"; }
        else if (control is CheckBox cb) { cb.Content = "CheckBox"; }
        else if (control is Label lbl) { lbl.Content = "Label"; }
        
        // Wire up events for selection and drag
        control.PointerPressed += PlacedControl_PointerPressed;
        control.PointerMoved += PlacedControl_PointerMoved;
        control.PointerReleased += PlacedControl_PointerReleased;
        
        return control;
    }',
    '    private Border CreateControl(string? typeName)
    {
        if (typeName == null) return null;
        
        // Create dummy ghost representation - not real control!
        var (width, height, text) = typeName switch
        {
            "Button" => (100.0, 30.0, "Button"),
            "TextBox" => (150.0, 25.0, "TextBox"),
            "TextBlock" => (80.0, 20.0, "TextBlock"),
            "CheckBox" => (100.0, 20.0, "☐ CheckBox"),
            "Label" => (80.0, 20.0, "Label"),
            "ComboBox" => (150.0, 25.0, "ComboBox ▼"),
            "ListBox" => (150.0, 100.0, "ListBox"),
            "Slider" => (150.0, 20.0, "━━━━━○━━"),
            _ => (100.0, 30.0, typeName)
        };
        
        var dummy = new Border
        {
            Tag = typeName,
            Width = width,
            Height = height,
            Background = Brushes.White,
            BorderBrush = Brushes.Black,
            BorderThickness = new Avalonia.Thickness(1),
            CornerRadius = new Avalonia.CornerRadius(2),
            Cursor = new Cursor(StandardCursorType.SizeAll),
            Child = new TextBlock
            {
                Text = text,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = 12
            }
        };
        
        // Wire up events for selection and drag
        dummy.PointerPressed += PlacedControl_PointerPressed;
        dummy.PointerMoved += PlacedControl_PointerMoved;
        dummy.PointerReleased += PlacedControl_PointerReleased;
        
        return dummy;
    }'),
    version = version + 1,
    updated_at = datetime('now')
WHERE filename = 'MainWindow.axaml.cs';
PATCH2

# SQL UPDATE: Fix AddToolboxButtons to expect Border not Button
sqlite3 $DB << 'PATCH3'
UPDATE project_files 
SET content = REPLACE(content,
    '    private void AddToolboxButtons(StackPanel stack, string[] controls)
    {
        foreach (var controlType in controls)
        {
            var btn = CreateToolboxButton(controlType);
            stack.Children.Add(btn);
        }
    }',
    '    private void AddToolboxButtons(StackPanel stack, string[] controls)
    {
        foreach (var controlType in controls)
        {
            var dummy = CreateToolboxButton(controlType);
            stack.Children.Add(dummy);
        }
    }'),
    version = version + 1,
    updated_at = datetime('now')
WHERE filename = 'MainWindow.axaml.cs';
PATCH3

# SQL UPDATE: Fix StartToolboxDrag to accept Border
sqlite3 $DB << 'PATCH4'
UPDATE project_files 
SET content = REPLACE(content,
    '    private void StartToolboxDrag(string controlType, Button sourceButton)',
    '    private void StartToolboxDrag(string controlType, Border sourceDummy)'),
    version = version + 1,
    updated_at = datetime('now')
WHERE filename = 'MainWindow.axaml.cs';
PATCH4

echo "✓ SQL patched: Dummies everywhere - no real controls!"

# Extract
sqlite3 $DB "SELECT content FROM project_files WHERE filename='MainWindow.axaml.cs';" > MainWindow.axaml.cs

sqlite3 $DB "SELECT filename, version FROM project_files WHERE filename='MainWindow.axaml.cs';"
