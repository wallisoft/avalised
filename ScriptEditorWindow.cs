using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Avalised;

/// <summary>
/// Script Editor Window - Edit event handlers for controls
/// Left panel: Event list (onClick, onHover, onChange, etc.)
/// Right panel: Code editor
/// </summary>
public class ScriptEditorWindow : Window
{
    private readonly Control _targetControl;
    private readonly Dictionary<string, string> _scripts;
    private ListBox? _eventList;
    private TextBox? _codeEditor;
    private TextBlock? _eventLabel;
    private string? _currentEvent;
    
    private static readonly string[] _availableEvents = new[]
    {
        "onClick",
        "onDoubleClick",
        "onRightClick",
        "onMouseEnter",
        "onMouseLeave",
        "onMouseMove",
        "onKeyDown",
        "onKeyUp",
        "onGotFocus",
        "onLostFocus",
        "onChange",
        "onLoad",
        "onResize"
    };
    
    public ScriptEditorWindow(Control targetControl, Dictionary<string, string> scripts)
    {
        _targetControl = targetControl;
        _scripts = scripts;
        
        Title = $"Script Editor - {targetControl.Name}";
        Width = 900;
        Height = 600;
        CanResize = true;
        
        BuildUI();
    }
    
    private void BuildUI()
    {
        // Main container
        var mainPanel = new DockPanel
        {
            LastChildFill = true
        };
        
        // Title bar
        var titleBar = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Padding = new Thickness(10),
            [DockPanel.DockProperty] = Dock.Top
        };
        
        var titleStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5
        };
        
        titleStack.Children.Add(new TextBlock
        {
            Text = $"Script Editor",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });
        
        titleStack.Children.Add(new TextBlock
        {
            Text = $"Control: {_targetControl.Name} ({_targetControl.GetType().Name})",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#AAAAAA"))
        });
        
        titleBar.Child = titleStack;
        mainPanel.Children.Add(titleBar);
        
        // Button bar at bottom
        var buttonBar = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F0F0F0")),
            Padding = new Thickness(10),
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
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
            Content = "Save & Close",
            Width = 120,
            Height = 32,
            Background = new SolidColorBrush(Color.Parse("#4CAF50")),
            Foreground = Brushes.White
        };
        saveButton.Click += OnSaveClick;
        buttonStack.Children.Add(saveButton);
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100,
            Height = 32
        };
        cancelButton.Click += OnCancelClick;
        buttonStack.Children.Add(cancelButton);
        
        buttonBar.Child = buttonStack;
        mainPanel.Children.Add(buttonBar);
        
        // Main content area (2 panels)
        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("250,*")
        };
        
        // LEFT PANEL: Event list
        var leftPanel = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F8F8F8")),
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Thickness(0, 0, 1, 0),
            [Grid.ColumnProperty] = 0
        };
        
        var leftStack = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        
        // Event list header
        var eventHeader = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E0E0E0")),
            Padding = new Thickness(10),
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
        
        eventHeader.Child = new TextBlock
        {
            Text = "EVENTS",
            FontWeight = FontWeight.Bold,
            FontSize = 12
        };
        leftStack.Children.Add(eventHeader);
        
        // Event list
        _eventList = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(5)
        };
        
        foreach (var eventName in _availableEvents)
        {
            var hasScript = _scripts.ContainsKey(eventName) && !string.IsNullOrWhiteSpace(_scripts[eventName]);
            var item = new ListBoxItem
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = hasScript ? "●" : "○",
                            Foreground = hasScript ? new SolidColorBrush(Color.Parse("#4CAF50")) : Brushes.Gray,
                            FontSize = 16,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = eventName,
                            FontSize = 13,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                },
                Tag = eventName
            };
            _eventList.Items.Add(item);
        }
        
        _eventList.SelectionChanged += OnEventSelectionChanged;
        leftStack.Children.Add(_eventList);
        
        leftPanel.Child = leftStack;
        contentGrid.Children.Add(leftPanel);
        
        // RIGHT PANEL: Code editor
        var rightPanel = new Border
        {
            Background = Brushes.White,
            [Grid.ColumnProperty] = 1,
            Padding = new Thickness(10)
        };
        
        var rightStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };
        
        // Code editor header
        var editorHeader = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E0E0E0")),
            Padding = new Thickness(10),
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Thickness(1)
        };
        
        _eventLabel = new TextBlock
        {
            Text = "Select an event to edit",
            FontWeight = FontWeight.Bold,
            FontSize = 12
        };
        editorHeader.Child = _eventLabel;
        rightStack.Children.Add(editorHeader);
        
        // Help text
        var helpText = new TextBlock
        {
            Text = "Write C# code for this event handler. Available context:\n• control - the control that fired this event\n• e - event arguments\n• canvas - the design canvas",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            TextWrapping = TextWrapping.Wrap
        };
        rightStack.Children.Add(helpText);
        
        // Code editor
        _codeEditor = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            TextWrapping = TextWrapping.NoWrap,
            FontFamily = new FontFamily("Courier New, Consolas, monospace"),
            FontSize = 13,
            Background = new SolidColorBrush(Color.Parse("#FAFAFA")),
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8),
            Watermark = "// Enter your code here...",
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 350
        };
        rightStack.Children.Add(_codeEditor);
        
        // Script info
        var infoText = new TextBlock
        {
            Text = "💡 Tip: Scripts are saved to the control and can be exported with your design",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            FontStyle = FontStyle.Italic
        };
        rightStack.Children.Add(infoText);
        
        rightPanel.Child = rightStack;
        contentGrid.Children.Add(rightPanel);
        
        mainPanel.Children.Add(contentGrid);
        
        Content = mainPanel;
        
        // Select first event by default
        if (_eventList.Items.Count > 0)
        {
            _eventList.SelectedIndex = 0;
        }
    }
    
    private void OnEventSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_eventList == null || _codeEditor == null || _eventLabel == null) return;
        
        // Save current script before switching
        if (_currentEvent != null && _codeEditor.Text != null)
        {
            _scripts[_currentEvent] = _codeEditor.Text;
        }
        
        // Load new event script
        if (_eventList.SelectedItem is ListBoxItem item && item.Tag is string eventName)
        {
            _currentEvent = eventName;
            _eventLabel.Text = $"Event: {eventName}";
            
            if (_scripts.TryGetValue(eventName, out var script))
            {
                _codeEditor.Text = script;
            }
            else
            {
                _codeEditor.Text = $"// {eventName} handler\n// Write your code here\n\n";
            }
            
            _codeEditor.Focus();
        }
    }
    
    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        // Save current script
        if (_currentEvent != null && _codeEditor?.Text != null)
        {
            _scripts[_currentEvent] = _codeEditor.Text;
        }
        
        Console.WriteLine($"💾 Saved scripts for {_targetControl.Name}:");
        foreach (var kvp in _scripts)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value))
            {
                Console.WriteLine($"   {kvp.Key}: {kvp.Value.Length} chars");
            }
        }
        
        Close();
    }
    
    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine($"❌ Script editing cancelled for {_targetControl.Name}");
        Close();
    }
}
