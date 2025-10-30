using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalised.Dialogs;

public class MarkupEditorDialog : Window
{
    private ComboBox? _settingsCombo;
    private TextBox? _editorBox;
    
    public MarkupEditorDialog()
    {
        Title = "🌳 Avalised™ - Markup Editor";
        Width = 900;
        Height = 650;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var mainPanel = new DockPanel { LastChildFill = true };

        // Top toolbar
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Avalonia.Thickness(10),
            Height = 35
        };

        toolbar.Children.Add(new TextBlock
        {
            Text = "Settings:",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold
        });

        _settingsCombo = new ComboBox
        {
            Width = 200,
            FontSize = 11
        };

        string[] settings = {
            "General Settings",
            "Canvas Settings",
            "Grid & Snapping",
            "Database Connection",
            "ODBC Configuration",
            "Import Settings",
            "Editor Preferences",
            "Appearance",
            "Keyboard Shortcuts",
            "Performance",
            "Advanced Options"
        };

        foreach (var setting in settings)
            _settingsCombo.Items.Add(setting);

        _settingsCombo.SelectedIndex = 0;
        _settingsCombo.SelectionChanged += OnSettingChanged;
        
        toolbar.Children.Add(_settingsCombo);

        var loadBtn = new Button
        {
            Content = "Load",
            Width = 70,
            Margin = new Avalonia.Thickness(10, 0, 0, 0)
        };
        loadBtn.Click += (s, e) => LoadSetting();
        toolbar.Children.Add(loadBtn);

        var saveBtn = new Button
        {
            Content = "Save",
            Width = 70
        };
        saveBtn.Click += (s, e) => SaveSetting();
        toolbar.Children.Add(saveBtn);

        DockPanel.SetDock(toolbar, Dock.Top);
        mainPanel.Children.Add(toolbar);

        // Editor
        _editorBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
            FontFamily = new FontFamily("Consolas, Courier New"),
            FontSize = 11,
            Padding = new Avalonia.Thickness(10),
            Text = GetDefaultMarkup()
        };

        var scroll = new ScrollViewer
        {
            Content = _editorBox,
            Margin = new Avalonia.Thickness(10, 0, 10, 10),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        mainPanel.Children.Add(scroll);

        // Bottom buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 0, 10, 10)
        };

        var applyBtn = new Button { Content = "Apply", Width = 80 };
        applyBtn.Click += (s, e) => ApplyChanges();
        
        var closeBtn = new Button { Content = "Close", Width = 80 };
        closeBtn.Click += (s, e) => Close();

        buttonPanel.Children.Add(applyBtn);
        buttonPanel.Children.Add(closeBtn);

        DockPanel.SetDock(buttonPanel, Dock.Bottom);
        mainPanel.Children.Add(buttonPanel);

        return mainPanel;
    }

    private string GetDefaultMarkup()
    {
        return @"# Avalised Markup Language (AVML)
# Settings Definition

Window: MainWindow
  Title: My Application
  Width: 1280
  Height: 720
  
  StackPanel: RootPanel
    Orientation: Vertical
    Margin: 10
    
    TextBlock: HeaderText
      Text: Welcome to Avalised
      FontSize: 24
      FontWeight: Bold
      Margin: 0,0,0,20
    
    Button: SubmitButton
      Content: Submit
      Width: 120
      Height: 35
      HorizontalAlignment: Left
";
    }

    private void OnSettingChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_settingsCombo?.SelectedItem is string setting && _editorBox != null)
        {
            _editorBox.Text = setting switch
            {
                "General Settings" => GetGeneralSettingsMarkup(),
                "Canvas Settings" => GetCanvasSettingsMarkup(),
                "Grid & Snapping" => GetGridSettingsMarkup(),
                "Database Connection" => GetDatabaseSettingsMarkup(),
                "ODBC Configuration" => GetODBCSettingsMarkup(),
                _ => GetDefaultMarkup()
            };
        }
    }

    private string GetGeneralSettingsMarkup()
    {
        return @"# General Settings

Setting: ShowSplashScreen
  Type: Boolean
  Value: true
  
Setting: LoadLastProject
  Type: Boolean
  Value: true
  
Setting: AutoSaveInterval
  Type: Integer
  Value: 5
  Unit: minutes
  
Setting: CheckForUpdates
  Type: Boolean
  Value: true
  
Setting: DefaultProjectPath
  Type: String
  Value: ~/Documents/Avalised/Projects
";
    }

    private string GetCanvasSettingsMarkup()
    {
        return @"# Canvas Settings

Canvas: DefaultSize
  Width: 800
  Height: 600
  
Canvas: ShowRulers
  Value: true
  
Canvas: ShowBorder
  Value: true
  
Canvas: BackgroundColor
  Value: #FFFFFF
  
Canvas: Padding
  Value: 20
";
    }

    private string GetGridSettingsMarkup()
    {
        return @"# Grid & Snapping Settings

Grid: ShowGrid
  Value: true
  
Grid: SnapToGrid
  Value: true
  
Grid: GridSize
  Value: 10
  Unit: pixels
  
Grid: MajorGridEvery
  Value: 5
  Unit: units
  
Grid: GridColor
  Value: #E8E8E8
  
Grid: SnapToControls
  Value: true
  
Grid: SnapDistance
  Value: 5
  Unit: pixels
";
    }

    private string GetDatabaseSettingsMarkup()
    {
        return @"# Database Settings

Database: Path
  Value: ~/.config/Avalised/designer.db
  
Database: AutoConnect
  Value: true
  
Database: EnableLogging
  Value: false
  
Database: ConnectionTimeout
  Value: 30
  Unit: seconds
";
    }

    private string GetODBCSettingsMarkup()
    {
        return @"# ODBC Configuration

ODBC: Driver
  Value: SQL Server
  Options: [SQL Server, Oracle, MySQL, PostgreSQL, MariaDB]
  
ODBC: Server
  Value: localhost
  
ODBC: Port
  Value: 1433
  
ODBC: Database
  Value: MyDatabase
  
ODBC: Username
  Value: sa
  
ODBC: UseWindowsAuth
  Value: false
  
ODBC: EnableSSL
  Value: true
  
ODBC: ConnectionTimeout
  Value: 30
  Unit: seconds
";
    }

    private void LoadSetting()
    {
        // TODO: Load from file
        if (_settingsCombo?.SelectedItem is string setting)
        {
            OnSettingChanged(_settingsCombo, new SelectionChangedEventArgs(null!, null!, null!));
        }
    }

    private void SaveSetting()
    {
        // TODO: Save to file
        var content = _editorBox?.Text ?? "";
        // Save logic here
    }

    private void ApplyChanges()
    {
        // TODO: Parse and apply settings
        var content = _editorBox?.Text ?? "";
        // Parse AVML and apply
    }
}
