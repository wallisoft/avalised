using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;

namespace Avalised;

public class DesignerLayout : ContentControl
{
    private readonly string _dbPath;
    private UITreeBuilder? _builder;
    
    // Drag & Drop state
    private string? _dragControlType;
    private bool _isDragging;
    private Canvas? _designCanvas;
    private Control? _selectedControl;
    private Border? _selectionBorder;
    
    public List<ControlAction>? Actions => _builder?.Actions;

    public DesignerLayout(string dbPath)
    {
        _dbPath = dbPath;
        LoadDesignerFromDatabase();
        
        // Defer wiring until visual tree is fully constructed
        Dispatcher.UIThread.Post(() =>
        {
            WireCanvasEvents();
            WireToolboxButtons();
        }, DispatcherPriority.Loaded);
    }
    
    private void LoadDesignerFromDatabase()
    {
        try
        {
            _builder = new UITreeBuilder(_dbPath);
            var designerUI = _builder.BuildUI();
            
            if (designerUI == null)
            {
                Content = new TextBlock 
                { 
                    Text = "ERROR: UITreeBuilder returned null",
                    Foreground = Brushes.Red,
                    FontSize = 16,
                    Margin = new Avalonia.Thickness(20)
                };
                return;
            }
            
            if (designerUI is Window window)
            {
                if (window.Content == null)
                {
                    Content = new TextBlock 
                    { 
                        Text = "ERROR: Window.Content is null",
                        Foreground = Brushes.Red,
                        FontSize = 16,
                        Margin = new Avalonia.Thickness(20)
                    };
                    return;
                }
                Content = window.Content;
            }
            else
            {
                Content = designerUI;
            }
            
            Console.WriteLine($"✅ DesignerLayout loaded: {designerUI.GetType().Name}");
        }
        catch (Exception ex)
        {
            Content = new TextBlock 
            { 
                Text = $"ERROR loading designer:\n{ex.Message}\n\n{ex.StackTrace}",
                Foreground = Brushes.Red,
                FontSize = 12,
                Margin = new Avalonia.Thickness(20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
        }
    }
    
    private void WireCanvasEvents()
    {
        _designCanvas = FindControl<Canvas>(this, "DesignCanvas");
        if (_designCanvas == null)
        {
            Console.WriteLine("⚠️  DesignCanvas not found");
            return;
        }
        
        // Mouse tracking
        _designCanvas.PointerMoved += OnCanvasPointerMoved;
        
        // Drop zone for new controls
        _designCanvas.PointerReleased += OnCanvasPointerReleased;
        
        // Selection
        _designCanvas.PointerPressed += OnCanvasPointerPressed;
        
        Console.WriteLine("✅ Canvas events wired");
    }
    
    private void WireToolboxButtons()
    {
        // Find all toolbox buttons and wire them for drag
        var toolboxButtons = new[]
        {
            ("AddButton", "Button"),
            ("AddTextBox", "TextBox"),
            ("AddLabel", "TextBlock"),
            ("AddCheckBox", "CheckBox"),
            ("AddPanel", "Panel"),
            ("AddStackPanel", "StackPanel"),
            ("AddCanvas", "Canvas")
        };
        
        int wiredCount = 0;
        foreach (var (buttonName, controlType) in toolboxButtons)
        {
            var button = FindControl<Button>(this, buttonName);
            if (button != null)
            {
                button.PointerPressed += (s, e) => OnToolboxButtonPressed(controlType, e);
                wiredCount++;
            }
        }
        
        Console.WriteLine($"✅ Wired {wiredCount} toolbox buttons");
    }
    
    private void OnToolboxButtonPressed(string controlType, PointerPressedEventArgs e)
    {
        _dragControlType = controlType;
        _isDragging = true;
        UpdateStatus($"Dragging: {controlType}", false);
        Console.WriteLine($"🎯 Started drag: {controlType}");
    }
    
    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_designCanvas == null) return;
        
        var pos = e.GetPosition(_designCanvas);
        UpdateMousePosition((int)pos.X, (int)pos.Y);
        
        // Visual feedback during drag
        if (_isDragging && _dragControlType != null)
        {
            _designCanvas.Cursor = new Cursor(StandardCursorType.Cross);
        }
    }
    
    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _dragControlType == null || _designCanvas == null)
            return;
        
        var pos = e.GetPosition(_designCanvas);
        
        // Create the new control
        Control? newControl = _dragControlType switch
        {
            "Button" => new Button { Content = "Button", Width = 100, Height = 32 },
            "TextBox" => new TextBox { Text = "TextBox", Width = 150, Height = 28 },
            "TextBlock" => new TextBlock { Text = "Label", FontSize = 14 },
            "CheckBox" => new CheckBox { Content = "CheckBox" },
            "Panel" => new Panel { Width = 200, Height = 150, Background = Brushes.LightGray },
            "StackPanel" => new StackPanel { Width = 200, Height = 150, Background = Brushes.LightBlue },
            "Canvas" => new Canvas { Width = 200, Height = 150, Background = Brushes.LightGreen },
            _ => null
        };
        
        if (newControl != null)
        {
            // Position it
            Canvas.SetLeft(newControl, pos.X);
            Canvas.SetTop(newControl, pos.Y);
            
            // Give it a unique name
            newControl.Name = $"{_dragControlType}_{DateTime.Now.Ticks % 10000}";
            
            // Add to canvas
            _designCanvas.Children.Add(newControl);
            
            // Make it selectable
            newControl.PointerPressed += OnControlPointerPressed;
            
            UpdateStatus($"Added {_dragControlType} at ({(int)pos.X}, {(int)pos.Y})", false);
            Console.WriteLine($"✅ Created {newControl.Name} at {(int)pos.X},{(int)pos.Y}");
            
            // Auto-select the new control
            SelectControl(newControl);
        }
        
        // Reset drag state
        _isDragging = false;
        _dragControlType = null;
        if (_designCanvas != null)
            _designCanvas.Cursor = Cursor.Default;
    }
    
    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Click on empty canvas = deselect
        if (e.Source == _designCanvas)
        {
            DeselectControl();
        }
    }
    
    private void OnControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            SelectControl(control);
            e.Handled = true; // Don't propagate to canvas
        }
    }
    
    private void SelectControl(Control control)
    {
        // Deselect previous
        DeselectControl();
        
        _selectedControl = control;
        
        // Show selection border
        ShowSelectionBorder(control);
        
        // Update properties panel
        UpdatePropertiesPanel(control);
        
        UpdateStatus($"Selected: {control.Name} ({control.GetType().Name})", false);
        Console.WriteLine($"🎯 Selected: {control.Name}");
    }
    
    private void DeselectControl()
    {
        if (_selectedControl == null) return;
        
        // Hide selection border
        HideSelectionBorder();
        
        _selectedControl = null;
        
        UpdateStatus("Ready", false);
    }
    
    private void ShowSelectionBorder(Control control)
    {
        if (_designCanvas == null) return;
        
        // Create selection border
        _selectionBorder = new Border
        {
            BorderBrush = Brushes.Blue,
            BorderThickness = new Avalonia.Thickness(2),
            IsHitTestVisible = false // Don't interfere with mouse events
        };
        
        // Position and size to match control
        Canvas.SetLeft(_selectionBorder, Canvas.GetLeft(control));
        Canvas.SetTop(_selectionBorder, Canvas.GetTop(control));
        _selectionBorder.Width = control.Bounds.Width;
        _selectionBorder.Height = control.Bounds.Height;
        
        _designCanvas.Children.Add(_selectionBorder);
    }
    
    private void HideSelectionBorder()
    {
        if (_selectionBorder != null && _designCanvas != null)
        {
            _designCanvas.Children.Remove(_selectionBorder);
            _selectionBorder = null;
        }
    }
    
    private void UpdatePropertiesPanel(Control control)
    {
        var propertiesContent = FindControl<StackPanel>(this, "PropertiesContent");
        if (propertiesContent == null)
        {
            Console.WriteLine("⚠️  PropertiesContent panel not found");
            return;
        }
        
        // Clear existing properties
        propertiesContent.Children.Clear();
        
        // Add title
        propertiesContent.Children.Add(new TextBlock
        {
            Text = "PROPERTIES",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        });
        
        // Add control type
        propertiesContent.Children.Add(new TextBlock
        {
            Text = $"{control.GetType().Name}",
            FontSize = 12,
            Foreground = Brushes.Gray,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        });
        
        // Add properties
        AddProperty(propertiesContent, "Name", control.Name ?? "");
        AddProperty(propertiesContent, "Width", control.Width.ToString());
        AddProperty(propertiesContent, "Height", control.Height.ToString());
        AddProperty(propertiesContent, "X", Canvas.GetLeft(control).ToString("F0"));
        AddProperty(propertiesContent, "Y", Canvas.GetTop(control).ToString("F0"));
        
        // Type-specific properties
        if (control is Button button)
        {
            AddProperty(propertiesContent, "Content", button.Content?.ToString() ?? "");
        }
        else if (control is TextBox textBox)
        {
            AddProperty(propertiesContent, "Text", textBox.Text ?? "");
        }
        else if (control is TextBlock textBlock)
        {
            AddProperty(propertiesContent, "Text", textBlock.Text ?? "");
        }
        else if (control is CheckBox checkBox)
        {
            AddProperty(propertiesContent, "Content", checkBox.Content?.ToString() ?? "");
        }
        
        Console.WriteLine($"✅ Properties panel updated for {control.Name}");
    }
    
    private void AddProperty(StackPanel parent, string propertyName, string propertyValue)
    {
        var propertyStack = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Spacing = 2,
            Margin = new Avalonia.Thickness(0, 0, 0, 8)
        };
        
        // Label
        propertyStack.Children.Add(new TextBlock
        {
            Text = propertyName,
            FontSize = 11,
            Foreground = Brushes.Gray
        });
        
        // TextBox for editing (for now, read-only)
        propertyStack.Children.Add(new TextBox
        {
            Text = propertyValue,
            FontSize = 11,
            IsReadOnly = true // TODO: Make editable
        });
        
        parent.Children.Add(propertyStack);
    }
    
    private T? FindControl<T>(Control? parent, string name) where T : Control
    {
        if (parent == null) return null;
        
        if (parent is T control && parent.Name == name)
            return control;
        
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is Control childControl)
            {
                var found = FindControl<T>(childControl, name);
                if (found != null) return found;
            }
        }
        return null;
    }
    
    public void UpdateStatus(string status, bool connected)
    {
        var statusText = FindControl<TextBlock>(this, "StatusText");
        if (statusText != null)
        {
            statusText.Text = status;
        }
    }
    
    public void UpdateWindowInfo(double width, double height)
    {
        var windowSize = FindControl<TextBlock>(this, "WindowSize");
        if (windowSize != null)
        {
            windowSize.Text = $"Window: {(int)width}×{(int)height}";
        }
    }
    
    private void UpdateMousePosition(int x, int y)
    {
        var mousePos = FindControl<TextBlock>(this, "MousePosition");
        if (mousePos != null)
        {
            mousePos.Text = $"Mouse: {x}, {y}";
        }
    }
    
    public void TogglePreviewMode()
    {
    }
}
