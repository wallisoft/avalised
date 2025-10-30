using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalised.Dialogs;

public class ScriptEditorDialog : Window
{
    private readonly Control _control;
    private ListBox? _eventsList;
    private TextBox? _scriptBox;
    
    public ScriptEditorDialog(Control control)
    {
        _control = control;
        
        Title = $"🌳 Avalised™ - Script Editor - {control.GetType().Name}";
        Width = 900;
        Height = 650;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("250,*")
        };

        // Left: Events list
        var eventsPanel = new DockPanel { LastChildFill = true };
        
        var eventsHeader = new Border
        {
            Background = Brush.Parse("#C8E6C9"),
            Height = 30,
            Child = new TextBlock
            {
                Text = "Events",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(8, 0)
            }
        };
        DockPanel.SetDock(eventsHeader, Dock.Top);
        eventsPanel.Children.Add(eventsHeader);

        _eventsList = new ListBox
        {
            FontSize = 11,
            FontFamily = new FontFamily("Segoe UI"),
            Padding = new Avalonia.Thickness(5)
        };
        _eventsList.SelectionChanged += OnEventSelected;
        
        PopulateEvents();
        
        eventsPanel.Children.Add(_eventsList);
        
        Grid.SetColumn(eventsPanel, 0);
        grid.Children.Add(eventsPanel);

        // Right: Script editor
        var editorPanel = new DockPanel { LastChildFill = true };
        
        var editorHeader = new Border
        {
            Background = Brush.Parse("#C8E6C9"),
            Height = 30,
            Child = new TextBlock
            {
                Text = "Script",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(8, 0)
            }
        };
        DockPanel.SetDock(editorHeader, Dock.Top);
        editorPanel.Children.Add(editorHeader);

        _scriptBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
            FontFamily = new FontFamily("Consolas, Courier New"),
            FontSize = 11,
            Padding = new Avalonia.Thickness(10),
            Margin = new Avalonia.Thickness(5),
            Text = "// Write your event handler code here\n\n"
        };

        var scroll = new ScrollViewer
        {
            Content = _scriptBox,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        
        editorPanel.Children.Add(scroll);
        
        Grid.SetColumn(editorPanel, 1);
        grid.Children.Add(editorPanel);

        // Bottom buttons
        var mainPanel = new DockPanel { LastChildFill = true };
        
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(10)
        };

        var saveBtn = new Button { Content = "Save", Width = 80 };
        saveBtn.Click += (s, e) => Close();
        
        var cancelBtn = new Button { Content = "Cancel", Width = 80 };
        cancelBtn.Click += (s, e) => Close();

        buttonPanel.Children.Add(saveBtn);
        buttonPanel.Children.Add(cancelBtn);

        DockPanel.SetDock(buttonPanel, Dock.Bottom);
        mainPanel.Children.Add(buttonPanel);
        mainPanel.Children.Add(grid);

        return mainPanel;
    }

    private void PopulateEvents()
    {
        var controlType = _control.GetType();
        
        // Common events for all controls
        string[] commonEvents = {
            "Loaded",
            "Unloaded",
            "PointerPressed",
            "PointerReleased",
            "PointerMoved",
            "PointerEntered",
            "PointerExited",
            "KeyDown",
            "KeyUp",
            "GotFocus",
            "LostFocus",
            "SizeChanged",
            "PropertyChanged"
        };

        // Control-specific events
        var specificEvents = controlType.Name switch
        {
            "Button" => new[] { "Click" },
            "TextBox" => new[] { "TextChanged", "TextChanging", "SelectionChanged" },
            "CheckBox" => new[] { "IsCheckedChanged", "Checked", "Unchecked" },
            "RadioButton" => new[] { "IsCheckedChanged", "Checked", "Unchecked" },
            "ComboBox" => new[] { "SelectionChanged", "DropDownOpened", "DropDownClosed" },
            "ListBox" => new[] { "SelectionChanged", "DoubleTapped" },
            "Slider" => new[] { "ValueChanged" },
            "Calendar" => new[] { "SelectedDatesChanged" },
            "DatePicker" => new[] { "SelectedDateChanged" },
            "Expander" => new[] { "Expanded", "Collapsed" },
            "TabControl" => new[] { "SelectionChanged" },
            "ScrollViewer" => new[] { "ScrollChanged" },
            "MenuItem" => new[] { "Click", "SubmenuOpened" },
            _ => Array.Empty<string>()
        };

        var allEvents = commonEvents.Concat(specificEvents).Distinct().OrderBy(e => e);

        foreach (var evt in allEvents)
        {
            _eventsList?.Items.Add(new ListBoxItem
            {
                Content = evt,
                FontFamily = new FontFamily("Segoe UI"),
                Padding = new Avalonia.Thickness(5, 3)
            });
        }
    }

    private void OnEventSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_eventsList?.SelectedItem is ListBoxItem item && item.Content is string eventName)
        {
            var template = eventName switch
            {
                "Click" => $@"private void On{_control.Name ?? "Control"}_Click(object? sender, RoutedEventArgs e)
{{
    // Handle click event
    
}}",
                "TextChanged" => $@"private void On{_control.Name ?? "Control"}_TextChanged(object? sender, TextChangedEventArgs e)
{{
    var text = ((TextBox)sender).Text;
    // Handle text changed
    
}}",
                "SelectionChanged" => $@"private void On{_control.Name ?? "Control"}_SelectionChanged(object? sender, SelectionChangedEventArgs e)
{{
    // Handle selection changed
    
}}",
                "ValueChanged" => $@"private void On{_control.Name ?? "Control"}_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
{{
    // Handle value changed
    
}}",
                "PointerPressed" => $@"private void On{_control.Name ?? "Control"}_PointerPressed(object? sender, PointerPressedEventArgs e)
{{
    // Handle pointer pressed
    
}}",
                "KeyDown" => $@"private void On{_control.Name ?? "Control"}_KeyDown(object? sender, KeyEventArgs e)
{{
    if (e.Key == Key.Enter)
    {{
        // Handle Enter key
    }}
}}",
                _ => $@"private void On{_control.Name ?? "Control"}_{eventName}(object? sender, EventArgs e)
{{
    // Handle {eventName} event
    
}}"
            };

            if (_scriptBox != null)
                _scriptBox.Text = template;
        }
    }
}
