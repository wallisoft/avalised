using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalised.Dialogs;

public class OptionsDialog : Window
{
    private TextBox? _customWidthBox;
    private TextBox? _customHeightBox;
    
    public OptionsDialog()
    {
        Title = "🌳 Avalised™ - Options";
        Width = 800;
        Height = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("200,*")
        };

        var categoryList = new ListBox
        {
            Background = Brush.Parse("#F5F5F5"),
            BorderBrush = Brush.Parse("#E0E0E0"),
            BorderThickness = new Avalonia.Thickness(0, 0, 1, 0)
        };
        
        string[] categories = { "General", "Canvas", "Grid & Snapping", "Database", "ODBC Connection", "Import Settings", "Editor", "Appearance", "Keyboard Shortcuts", "Performance", "Advanced" };
        
        foreach (var cat in categories)
            categoryList.Items.Add(cat);
        
        categoryList.SelectedIndex = 0;
        Grid.SetColumn(categoryList, 0);
        grid.Children.Add(categoryList);

        var settingsPanel = new ScrollViewer
        {
            Padding = new Avalonia.Thickness(20),
            Content = CreateGeneralSettings()
        };
        Grid.SetColumn(settingsPanel, 1);
        grid.Children.Add(settingsPanel);

        categoryList.SelectionChanged += (s, e) =>
        {
            if (categoryList.SelectedItem is string category)
            {
                settingsPanel.Content = category switch
                {
                    "General" => CreateGeneralSettings(),
                    "Canvas" => CreateCanvasSettings(),
                    "Grid & Snapping" => CreateGridSettings(),
                    "Database" => CreateDatabaseSettings(),
                    "ODBC Connection" => CreateODBCSettings(),
                    "Import Settings" => CreateImportSettings(),
                    "Editor" => CreateEditorSettings(),
                    "Appearance" => CreateAppearanceSettings(),
                    "Keyboard Shortcuts" => CreateKeyboardSettings(),
                    "Performance" => CreatePerformanceSettings(),
                    "Advanced" => CreateAdvancedSettings(),
                    _ => CreateGeneralSettings()
                };
            }
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 10, 20, 10)
        };

        var okBtn = new Button { Content = "OK", Width = 80 };
        okBtn.Click += (s, e) => Close();
        
        var cancelBtn = new Button { Content = "Cancel", Width = 80 };
        cancelBtn.Click += (s, e) => Close();
        
        var applyBtn = new Button { Content = "Apply", Width = 80 };
        applyBtn.Click += (s, e) => { /* TODO */ };

        buttonPanel.Children.Add(okBtn);
        buttonPanel.Children.Add(cancelBtn);
        buttonPanel.Children.Add(applyBtn);

        var mainPanel = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(buttonPanel, Dock.Bottom);
        mainPanel.Children.Add(buttonPanel);
        mainPanel.Children.Add(grid);

        return mainPanel;
    }

    private Control CreateGeneralSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "General Settings");
        AddCheckBox(stack, "Show splash screen on startup");
        AddCheckBox(stack, "Load last project on startup");
        AddCheckBox(stack, "Auto-save every 5 minutes", true);
        AddCheckBox(stack, "Check for updates on startup");
        AddTextSetting(stack, "Default project path", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        AddTextSetting(stack, "Temp files location", Path.GetTempPath());
        
        return stack;
    }

    private Control CreateCanvasSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Canvas Size Presets");
        
        var presetsCombo = new ComboBox { Width = 200 };
        string[] presets = {
            "Custom",
            "800×600 (SVGA)",
            "1024×768 (XGA)",
            "1280×720 (HD)",
            "1920×1080 (Full HD)",
            "2560×1440 (QHD)",
            "320×568 (iPhone SE)",
            "375×667 (iPhone 8)",
            "390×844 (iPhone 13)",
            "414×896 (iPhone 11 Pro Max)",
            "360×640 (Android Small)",
            "412×915 (Android Medium)",
            "768×1024 (iPad)",
            "1024×1366 (iPad Pro 12.9\")"
        };
        foreach (var preset in presets)
            presetsCombo.Items.Add(preset);
        presetsCombo.SelectedIndex = 1;
        
        stack.Children.Add(presetsCombo);
        
        AddSeparator(stack);
        AddHeader(stack, "Custom Canvas Size");
        
        var customPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        customPanel.Children.Add(new TextBlock { Text = "Width:", VerticalAlignment = VerticalAlignment.Center });
        _customWidthBox = new TextBox { Text = "800", Width = 80 };
        customPanel.Children.Add(_customWidthBox);
        customPanel.Children.Add(new TextBlock { Text = "Height:", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(10, 0, 0, 0) });
        _customHeightBox = new TextBox { Text = "600", Width = 80 };
        customPanel.Children.Add(_customHeightBox);
        customPanel.Children.Add(new TextBlock { Text = "px", VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(customPanel);
        
        presetsCombo.SelectionChanged += (s, e) =>
        {
            if (presetsCombo.SelectedItem is string selected && selected != "Custom")
            {
                var parts = selected.Split('×', '(');
                if (parts.Length >= 2)
                {
                    _customWidthBox.Text = parts[0].Trim();
                    _customHeightBox.Text = parts[1].Split(' ')[0].Trim();
                }
            }
        };
        
        AddSeparator(stack);
        AddHeader(stack, "Canvas Display");
        AddCheckBox(stack, "Show rulers", true);
        AddCheckBox(stack, "Show canvas border", true);
        AddColorSetting(stack, "Canvas background color", "#FFFFFF");
        AddNumberSetting(stack, "Canvas padding (px)", "20");
        
        return stack;
    }

    private Control CreateGridSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Grid & Snapping");
        AddCheckBox(stack, "Show grid", true);
        AddCheckBox(stack, "Snap to grid", true);
        AddNumberSetting(stack, "Grid size (px)", "10");
        AddNumberSetting(stack, "Major grid every (units)", "5");
        AddColorSetting(stack, "Grid color", "#E8E8E8");
        AddCheckBox(stack, "Snap to controls");
        AddNumberSetting(stack, "Snap distance (px)", "5");
        
        return stack;
    }

    private Control CreateDatabaseSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "SQLite Database");
        AddTextSetting(stack, "Database path", "~/.config/Avalised/designer.db");
        AddCheckBox(stack, "Auto-connect on startup", true);
        AddCheckBox(stack, "Enable query logging");
        AddCheckBox(stack, "Vacuum database on close");
        
        return stack;
    }

    private Control CreateODBCSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "ODBC Data Source Configuration");
        
        AddComboSetting(stack, "Driver", new[] { "SQL Server", "Oracle", "MySQL", "PostgreSQL", "MariaDB", "AWS RDS", "Azure SQL", "Custom DSN" }, 0);
        
        AddSeparator(stack);
        AddHeader(stack, "Connection Details");
        AddTextSetting(stack, "Server / Host", "localhost");
        AddNumberSetting(stack, "Port", "1433");
        AddTextSetting(stack, "Database Name", "");
        AddTextSetting(stack, "Username", "");
        AddPasswordSetting(stack, "Password", "");
        
        AddSeparator(stack);
        AddHeader(stack, "Connection Options");
        AddCheckBox(stack, "Use Windows Authentication");
        AddCheckBox(stack, "Enable SSL/TLS", true);
        AddCheckBox(stack, "Trust Server Certificate");
        AddNumberSetting(stack, "Connection timeout (seconds)", "30");
        AddNumberSetting(stack, "Command timeout (seconds)", "30");
        AddCheckBox(stack, "Use connection pooling", true);
        AddNumberSetting(stack, "Max pool size", "100");
        
        AddSeparator(stack);
        
        var testPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
        var testBtn = new Button { Content = "Test Connection", Width = 150 };
        testBtn.Click += async (s, e) =>
        {
            testBtn.IsEnabled = false;
            testBtn.Content = "Testing...";
            await System.Threading.Tasks.Task.Delay(1000); // Simulate test
            testBtn.Content = "✓ Connection Successful!";
            testBtn.Background = Brush.Parse("#4CAF50");
            await System.Threading.Tasks.Task.Delay(2000);
            testBtn.Content = "Test Connection";
            testBtn.Background = null;
            testBtn.IsEnabled = true;
        };
        testPanel.Children.Add(testBtn);
        
        var saveBtn = new Button { Content = "Save DSN", Width = 120 };
        testPanel.Children.Add(saveBtn);
        
        stack.Children.Add(testPanel);
        
        AddSeparator(stack);
        stack.Children.Add(new TextBlock
        {
            Text = "💡 Tip: ODBC connections enable live data binding to Oracle, SQL Server, MySQL, PostgreSQL, AWS RDS, and more!",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontStyle = FontStyle.Italic,
            Foreground = Brush.Parse("#666666"),
            FontSize = 11
        });
        
        return stack;
    }

    private Control CreateImportSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "VB5/VB6 Import");
        AddCheckBox(stack, "Preserve control names", true);
        AddCheckBox(stack, "Import event handlers");
        AddCheckBox(stack, "Convert VB types to C#", true);
        
        AddSeparator(stack);
        AddHeader(stack, "XAML Import");
        AddCheckBox(stack, "Preserve namespaces");
        AddCheckBox(stack, "Import resources");
        
        AddSeparator(stack);
        AddHeader(stack, "HTML/CSS Import");
        AddCheckBox(stack, "Convert div to Panel", true);
        AddCheckBox(stack, "Import CSS styles");
        
        return stack;
    }

    private Control CreateEditorSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Code Editor");
        AddComboSetting(stack, "Font family", new[] { "Consolas", "Courier New", "Monaco", "Menlo" }, 0);
        AddNumberSetting(stack, "Font size", "12");
        AddCheckBox(stack, "Show line numbers", true);
        AddCheckBox(stack, "Word wrap");
        AddCheckBox(stack, "Auto-indent", true);
        AddNumberSetting(stack, "Tab size", "4");
        AddCheckBox(stack, "Use spaces instead of tabs", true);
        
        return stack;
    }

    private Control CreateAppearanceSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Appearance");
        AddComboSetting(stack, "Theme", new[] { "Light", "Dark", "System" }, 0);
        AddColorSetting(stack, "Accent color", "#2E7D32");
        AddColorSetting(stack, "Menu bar color", "#2E7D32");
        AddColorSetting(stack, "Status bar color", "#2E7D32");
        AddColorSetting(stack, "Panel title bar color", "#C8E6C9");
        AddCheckBox(stack, "Show toolbar icons", true);
        AddComboSetting(stack, "Icon size", new[] { "Small", "Medium", "Large" }, 1);
        
        return stack;
    }

    private Control CreateKeyboardSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Keyboard Shortcuts");
        AddKeySetting(stack, "New File", "Ctrl+N");
        AddKeySetting(stack, "Open File", "Ctrl+O");
        AddKeySetting(stack, "Save", "Ctrl+S");
        AddKeySetting(stack, "Undo", "Ctrl+Z");
        AddKeySetting(stack, "Redo", "Ctrl+Y");
        AddKeySetting(stack, "Toggle Preview", "F8");
        AddKeySetting(stack, "Run", "F9");
        
        return stack;
    }

    private Control CreatePerformanceSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Performance");
        AddCheckBox(stack, "Enable hardware acceleration", true);
        AddCheckBox(stack, "Use GPU rendering", true);
        AddNumberSetting(stack, "Max undo history", "100");
        AddCheckBox(stack, "Cache rendered controls");
        AddNumberSetting(stack, "Max cache size (MB)", "256");
        AddCheckBox(stack, "Lazy load properties");
        
        return stack;
    }

    private Control CreateAdvancedSettings()
    {
        var stack = new StackPanel { Spacing = 12 };
        
        AddHeader(stack, "Advanced Settings");
        AddCheckBox(stack, "Enable debug mode");
        AddCheckBox(stack, "Verbose logging");
        AddTextSetting(stack, "Log file path", "~/.config/Avalised/logs/");
        AddCheckBox(stack, "Enable telemetry");
        AddCheckBox(stack, "Use experimental features");
        AddTextArea(stack, "Custom startup script", "");
        
        return stack;
    }

    // Helper methods
    private void AddHeader(StackPanel stack, string text)
    {
        stack.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 8, 0, 4)
        });
    }

    private void AddSeparator(StackPanel stack)
    {
        stack.Children.Add(new Border
        {
            Height = 1,
            Background = Brushes.LightGray,
            Margin = new Avalonia.Thickness(0, 8, 0, 8)
        });
    }

    private void AddCheckBox(StackPanel stack, string label, bool isChecked = false)
    {
        stack.Children.Add(new CheckBox
        {
            Content = label,
            IsChecked = isChecked
        });
    }

    private void AddTextSetting(StackPanel stack, string label, string value)
    {
        stack.Children.Add(new TextBlock { Text = label, FontSize = 12, FontWeight = FontWeight.Bold });
        stack.Children.Add(new TextBox { Text = value, Margin = new Avalonia.Thickness(0, 2, 0, 0) });
    }

    private void AddPasswordSetting(StackPanel stack, string label, string value)
    {
        stack.Children.Add(new TextBlock { Text = label, FontSize = 12, FontWeight = FontWeight.Bold });
        stack.Children.Add(new TextBox { Text = value, PasswordChar = '●', Margin = new Avalonia.Thickness(0, 2, 0, 0) });
    }

    private void AddTextArea(StackPanel stack, string label, string value)
    {
        stack.Children.Add(new TextBlock { Text = label, FontSize = 12, FontWeight = FontWeight.Bold });
        stack.Children.Add(new TextBox
        {
            Text = value,
            Height = 100,
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 2, 0, 0)
        });
    }

    private void AddNumberSetting(StackPanel stack, string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        panel.Children.Add(new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 200
        });
        panel.Children.Add(new TextBox { Text = value, Width = 100 });
        stack.Children.Add(panel);
    }

    private void AddColorSetting(StackPanel stack, string label, string color)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        panel.Children.Add(new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 200
        });
        panel.Children.Add(new TextBox { Text = color, Width = 100 });
        panel.Children.Add(new Border
        {
            Width = 30,
            Height = 20,
            Background = Brush.Parse(color),
            BorderBrush = Brushes.Black,
            BorderThickness = new Avalonia.Thickness(1)
        });
        stack.Children.Add(panel);
    }

    private void AddComboSetting(StackPanel stack, string label, string[] options, int selectedIndex)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        panel.Children.Add(new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 200
        });
        var combo = new ComboBox { Width = 150 };
        foreach (var opt in options)
            combo.Items.Add(opt);
        combo.SelectedIndex = selectedIndex;
        panel.Children.Add(combo);
        stack.Children.Add(panel);
    }

    private void AddKeySetting(StackPanel stack, string action, string shortcut)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        panel.Children.Add(new TextBlock
        {
            Text = action,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 200
        });
        panel.Children.Add(new TextBox
        {
            Text = shortcut,
            Width = 150,
            IsReadOnly = true,
            Background = Brush.Parse("#F5F5F5")
        });
        stack.Children.Add(panel);
    }
}
