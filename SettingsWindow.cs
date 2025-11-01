using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Avalised;

/// <summary>
/// Settings Window - Configure designer preferences
/// Snap to grid, grid size, visual settings, etc.
/// </summary>
public class SettingsWindow : Window
{
    private CheckBox? _snapToGridCheckBox;
    private NumericUpDown? _gridSizeInput;
    private CheckBox? _showGridCheckBox;
    private CheckBox? _showRulersCheckBox;
    private ComboBox? _themeComboBox;
    
    public bool SnapToGrid { get; private set; }
    public int GridSize { get; private set; } = 10;
    public bool ShowGrid { get; private set; } = true;
    public bool ShowRulers { get; private set; } = false;
    public string Theme { get; private set; } = "Light";
    
    public event EventHandler<EventArgs>? SettingsChanged;
    
    public SettingsWindow(bool snapToGrid = false, int gridSize = 10, bool showGrid = true)
    {
        SnapToGrid = snapToGrid;
        GridSize = gridSize;
        ShowGrid = showGrid;
        
        Title = "Settings - Avalised Designer";
        Width = 600;
        Height = 500;
        CanResize = false;
        
        BuildUI();
    }
    
    private void BuildUI()
    {
        var mainPanel = new DockPanel
        {
            LastChildFill = true
        };
        
        // Title bar with gradient
        var titleBar = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.Parse("#667eea"), 0),
                    new GradientStop(Color.Parse("#764ba2"), 1)
                }
            },
            Padding = new Thickness(20, 15),
            [DockPanel.DockProperty] = Dock.Top
        };
        
        var titleStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15
        };
        
        titleStack.Children.Add(new TextBlock
        {
            Text = "⚙️",
            FontSize = 32,
            VerticalAlignment = VerticalAlignment.Center
        });
        
        var titleTextStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2
        };
        
        titleTextStack.Children.Add(new TextBlock
        {
            Text = "Designer Settings",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });
        
        titleTextStack.Children.Add(new TextBlock
        {
            Text = "Configure your Avalised experience",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#E0E0E0"))
        });
        
        titleStack.Children.Add(titleTextStack);
        titleBar.Child = titleStack;
        mainPanel.Children.Add(titleBar);
        
        // Button bar
        var buttonBar = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F8F8F8")),
            Padding = new Thickness(20, 15),
            BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
            BorderThickness = new Thickness(0, 1, 0, 0),
            [DockPanel.DockProperty] = Dock.Bottom
        };
        
        var buttonStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        
        var saveButton = new Button
        {
            Content = "💾 Save & Apply",
            Width = 140,
            Height = 36,
            Background = new SolidColorBrush(Color.Parse("#667eea")),
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold
        };
        saveButton.Click += OnSaveClick;
        buttonStack.Children.Add(saveButton);
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100,
            Height = 36,
            FontSize = 14
        };
        cancelButton.Click += OnCancelClick;
        buttonStack.Children.Add(cancelButton);
        
        buttonBar.Child = buttonStack;
        mainPanel.Children.Add(buttonBar);
        
        // Main content
        var scrollViewer = new ScrollViewer
        {
            Padding = new Thickness(20)
        };
        
        var contentStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 20
        };
        
        // Grid section
        contentStack.Children.Add(CreateSection(
            "📐 Grid & Snapping",
            "Control how controls align to the canvas grid",
            CreateGridSettings()
        ));
        
        // Visual section
        contentStack.Children.Add(CreateSection(
            "🎨 Visual Preferences",
            "Customize the appearance of the designer",
            CreateVisualSettings()
        ));
        
        // Behavior section
        contentStack.Children.Add(CreateSection(
            "⚡ Behavior",
            "Configure designer behavior",
            CreateBehaviorSettings()
        ));
        
        scrollViewer.Content = contentStack;
        mainPanel.Children.Add(scrollViewer);
        
        Content = mainPanel;
    }
    
    private Border CreateSection(string title, string description, Control content)
    {
        var section = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20)
        };
        
        var sectionStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15
        };
        
        var headerStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5
        };
        
        headerStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeight.Bold
        });
        
        headerStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            TextWrapping = TextWrapping.Wrap
        });
        
        sectionStack.Children.Add(headerStack);
        sectionStack.Children.Add(new Border { Height = 1, Background = new SolidColorBrush(Color.Parse("#E0E0E0")) });
        sectionStack.Children.Add(content);
        
        section.Child = sectionStack;
        return section;
    }
    
    private StackPanel CreateGridSettings()
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 15 };
        
        // Snap to Grid
        var snapRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        _snapToGridCheckBox = new CheckBox { IsChecked = SnapToGrid };
        snapRow.Children.Add(_snapToGridCheckBox);
        
        var snapLabel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 2 };
        snapLabel.Children.Add(new TextBlock { Text = "Enable Snap to Grid", FontSize = 14, FontWeight = FontWeight.SemiBold });
        snapLabel.Children.Add(new TextBlock { Text = "Controls align to grid points when dragging", FontSize = 11, Foreground = new SolidColorBrush(Color.Parse("#666666")) });
        snapRow.Children.Add(snapLabel);
        stack.Children.Add(snapRow);
        
        // Grid Size
        var gridRow = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };
        gridRow.Children.Add(new TextBlock { Text = "Grid Size (pixels):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center, [Grid.ColumnProperty] = 0, Margin = new Thickness(0,0,15,0) });
        _gridSizeInput = new NumericUpDown { Value = GridSize, Minimum = 5, Maximum = 50, Increment = 5, Width = 100, [Grid.ColumnProperty] = 2 };
        gridRow.Children.Add(_gridSizeInput);
        stack.Children.Add(gridRow);
        
        // Show Grid
        var showRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        _showGridCheckBox = new CheckBox { IsChecked = ShowGrid };
        showRow.Children.Add(_showGridCheckBox);
        showRow.Children.Add(new TextBlock { Text = "Show grid lines on canvas", FontSize = 14 });
        stack.Children.Add(showRow);
        
        return stack;
    }
    
    private StackPanel CreateVisualSettings()
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 15 };
        
        // Rulers
        var rulersRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        _showRulersCheckBox = new CheckBox { IsChecked = ShowRulers };
        rulersRow.Children.Add(_showRulersCheckBox);
        rulersRow.Children.Add(new TextBlock { Text = "Show rulers (measurement guides)", FontSize = 14 });
        stack.Children.Add(rulersRow);
        
        // Theme
        var themeRow = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,200") };
        themeRow.Children.Add(new TextBlock { Text = "Theme:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center, [Grid.ColumnProperty] = 0, Margin = new Thickness(0,0,15,0) });
        _themeComboBox = new ComboBox { Width = 200, [Grid.ColumnProperty] = 2, SelectedIndex = 0 };
        _themeComboBox.Items.Add("Light (Default)");
        _themeComboBox.Items.Add("Dark");
        _themeComboBox.Items.Add("High Contrast");
        themeRow.Children.Add(_themeComboBox);
        stack.Children.Add(themeRow);
        
        return stack;
    }
    
    private StackPanel CreateBehaviorSettings()
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 15 };
        stack.Children.Add(new TextBlock 
        { 
            Text = "💡 More settings coming soon:\n• Auto-save\n• Undo/Redo limits\n• Double-click behavior",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            TextWrapping = TextWrapping.Wrap
        });
        return stack;
    }
    
    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        SnapToGrid = _snapToGridCheckBox?.IsChecked ?? false;
        GridSize = (int)(_gridSizeInput?.Value ?? 10);
        ShowGrid = _showGridCheckBox?.IsChecked ?? true;
        ShowRulers = _showRulersCheckBox?.IsChecked ?? false;
        Theme = _themeComboBox?.SelectedIndex switch { 1 => "Dark", 2 => "HighContrast", _ => "Light" };
        
        Console.WriteLine($"💾 Settings: Snap={SnapToGrid}, Grid={GridSize}px, ShowGrid={ShowGrid}");
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        Close();
    }
    
    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
