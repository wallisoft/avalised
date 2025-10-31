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
    
    // Drag & Drop state (toolbox)
    private string? _dragControlType;
    private bool _isDragging;
    
    // Control movement drag state - OPTIMIZED with RenderTransform! 🚀
    private bool _isDraggingControl;
    private Control? _draggedControl;
    private Avalonia.Point _dragStartPoint;
    private double _dragStartLeft;
    private double _dragStartTop;
    private TranslateTransform? _dragTransform; // GPU-accelerated smooth dragging!
    private ITransform? _originalTransform; // Store original transform to restore
    
    // Preview mode state
    private bool _isPreviewMode = false;
    
    private Canvas? _designCanvas;
    private Control? _selectedControl;
    // Selection border removed - cursor shows resize zones!
    private ResizeBehavior? _resizeBehavior;
    
    public List<ControlAction>? Actions => _builder?.Actions;

    public DesignerLayout(string dbPath)
    {
        _dbPath = dbPath;
        LoadDesignerFromDatabase();
        
        // Defer wiring until visual tree is fully constructed
        Dispatcher.UIThread.Post(() =>
        {
            WireCanvasEvents();
            // WireToolboxButtons(); // No longer needed - toolbox is soft-coded!
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
                var capturedType = controlType; // Fix closure bug!
                button.PointerPressed += (s, e) => OnToolboxButtonPressed(capturedType, e);
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
        
        // Route to resize behavior if active
        if (_resizeBehavior != null && _selectedControl != null)
        {
            _resizeBehavior.HandlePointerMoved(sender, e);
        }
        
        // Handle control dragging (moving existing controls) - OPTIMIZED!
        if (_isDraggingControl && _draggedControl != null && _dragTransform != null)
        {
            var deltaX = pos.X - _dragStartPoint.X;
            var deltaY = pos.Y - _dragStartPoint.Y;
            
            // 🚀 GPU-ACCELERATED: Just update the transform, NO layout recalculation!
            _dragTransform.X = deltaX;
            _dragTransform.Y = deltaY;
            

            var newLeft = _dragStartLeft + deltaX;
            var newTop = _dragStartTop + deltaY;
            UpdateStatus($"Moving: {_draggedControl.Name} to ({(int)newLeft}, {(int)newTop})", false);
            return;
        }
        
        // Visual feedback during toolbox drag
        if (_isDragging && _dragControlType != null)
        {
            _designCanvas.Cursor = new Cursor(StandardCursorType.Cross);
        }
    }
    
    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // Route to resize behavior if active
        if (_resizeBehavior != null && _selectedControl != null)
        {
            _resizeBehavior.HandlePointerReleased(sender, e);
        }
        
        // Handle control drag end - COMMIT the transform to actual position
        if (_isDraggingControl && _draggedControl != null && _dragTransform != null)
        {
            // Calculate final position
            var finalLeft = _dragStartLeft + _dragTransform.X;
            var finalTop = _dragStartTop + _dragTransform.Y;
            
            // COMMIT: Apply to Canvas.Left/Top only ONCE at the end
            Canvas.SetLeft(_draggedControl, finalLeft);
            Canvas.SetTop(_draggedControl, finalTop);
            
            // Restore original transform (or null)
            _draggedControl.RenderTransform = _originalTransform;
            

            Console.WriteLine($"✅ Moved {_draggedControl.Name} to ({(int)finalLeft}, {(int)finalTop})");
            UpdateStatus($"Moved {_draggedControl.Name}", false);
            
            // Clean up
            _isDraggingControl = false;
            _draggedControl = null;
            _dragTransform = null;
            _originalTransform = null;
            return;
        }
        
        // Handle toolbox drag (creating new control)
        if (!_isDragging || _dragControlType == null || _designCanvas == null)
            return;
        
        var pos = e.GetPosition(_designCanvas);
        
        // Create dummy control properties
        var properties = new Dictionary<string, string>();
        
        switch (_dragControlType)
        {
            case "Button":
                properties["content"] = "Button";
                properties["width"] = "100";
                properties["height"] = "32";
                break;
            case "TextBox":
                properties["text"] = "TextBox";
                properties["width"] = "150";
                properties["height"] = "28";
                break;
            case "TextBlock":
                properties["text"] = "Label";
                properties["width"] = "80";
                properties["height"] = "24";
                break;
            case "CheckBox":
                properties["content"] = "CheckBox";
                properties["width"] = "100";
                properties["height"] = "24";
                break;
            case "Panel":
                properties["width"] = "200";
                properties["height"] = "150";
                properties["background"] = "#EEEEEE";
                break;
            case "StackPanel":
                properties["width"] = "200";
                properties["height"] = "150";
                properties["background"] = "#E3F2FD";
                break;
            case "Canvas":
                properties["width"] = "200";
                properties["height"] = "150";
                properties["background"] = "#E8F5E9";
                break;
        }
        
        // Create dummy control
        var newControl = CreateDummyControl(_dragControlType, properties);
        
        // Position it
        Canvas.SetLeft(newControl, pos.X);
        Canvas.SetTop(newControl, pos.Y);
        
        // Give it a unique name
        newControl.Name = $"{_dragControlType}_{DateTime.Now.Ticks % 10000}";
        
        // Add to canvas
        _designCanvas.Children.Add(newControl);
        newControl.SetValue(Canvas.ZIndexProperty, 1); // Normal layer
        
        // Make it selectable - dummy controls don't consume events
        newControl.PointerPressed += OnControlPointerPressed;
        
        UpdateStatus($"Added {_dragControlType} at ({(int)pos.X}, {(int)pos.Y})", false);
        Console.WriteLine($"✅ Dummy control: Created {newControl.Name} at {(int)pos.X},{(int)pos.Y}");
        
        // Auto-select the new control
        SelectControl(newControl);
        
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
        if (sender is Control control && _designCanvas != null)
        {
            // If already selected, try ResizeBehavior first
            if (control == _selectedControl && _resizeBehavior != null)
            {
                // Let ResizeBehavior try to handle it
                if (_resizeBehavior.HandlePointerPressed(sender, e))
                {
                    // ResizeBehavior is handling it (resize started)
                    return;
                }
            }
            
            // Select the control (only if not already selected, or if not in resize zone)
            if (control != _selectedControl)
            {
                SelectControl(control);
            }
            
            // If in resize zone, stop here - ResizeBehavior will handle it
            {
                return;
            }
            
            // Start dragging this control - OPTIMIZED with RenderTransform! 🚀
            _isDraggingControl = true;
            _draggedControl = control;
            _dragStartPoint = e.GetPosition(_designCanvas);
            _dragStartLeft = Canvas.GetLeft(control);
            _dragStartTop = Canvas.GetTop(control);
            
            // Store original transform and create new one for dragging
            _originalTransform = control.RenderTransform;
            _dragTransform = new TranslateTransform(0, 0);
            control.RenderTransform = _dragTransform;
            
            e.Handled = true;
        }
    }
    
    // Helper: Check if position is in resize zone (8px from edges)
    
    private void SelectControl(Control control)
    {
        // Deselect previous
        DeselectControl();
        
        _selectedControl = control;
        
        // 🚀 ATTACH RESIZE BEHAVIOR!
        if (_designCanvas != null)
        {
            _resizeBehavior = new ResizeBehavior(control, _designCanvas, minWidth: 20, minHeight: 20);
        }
        
        // Update properties panel
        UpdatePropertiesPanel(control);
        
        UpdateStatus($"Selected: {control.Name} ({control.GetType().Name}) - Drag to move, resize from edges", false);
        Console.WriteLine($"🎯 Selected: {control.Name}");
    }
    
    private void DeselectControl()
    {
        if (_selectedControl == null) return;
        
        // Clear resize behavior
        _resizeBehavior = null;
        _selectedControl = null;
        
        UpdateStatus("Ready", false);
    }
    
    
    
    // Update selection border size AND position during resize
    
    // Finalize selection border after resize completes
    
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
    
    // Create dummy control for design mode (not functional, just visual)
    private Control CreateDummyControl(string controlType, Dictionary<string,string> properties)
    {
        // Get dimensions from properties
        var width = properties.TryGetValue("width", out var w) ? double.Parse(w) : 100;
        var height = properties.TryGetValue("height", out var h) ? double.Parse(h) : 32;
        
        // Get content/text for label
        string displayText = controlType;
        if (properties.TryGetValue("content", out var content))
            displayText = content;
        else if (properties.TryGetValue("text", out var text))
            displayText = text;
        
        // Get background color if specified
        IBrush background = Brushes.White;
        if (properties.TryGetValue("background", out var bgColor))
            background = Brush.Parse(bgColor);
        
        // Create dummy representation - a Border with label
        var dummy = new Border
        {
            Width = width,
            Height = height,
            Background = background,
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Child = new TextBlock
            {
                Text = displayText,
                FontSize = 10,
                Foreground = Brushes.DarkGray,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
        
        // Store control type in Tag for later reference
        dummy.Tag = controlType;
        
        return dummy;
    }
    
    
    // PUBLIC API - Called by ActionExecutor for soft-coded control creation
    public void AddControlToCanvas(string controlType, double x, double y, Dictionary<string,string> properties)
    {
        if (_designCanvas == null)
        {
            Console.WriteLine("❌ Canvas not available");
            return;
        }
        
        // Create dummy control for design mode
        Control newControl = CreateDummyControl(controlType, properties);
        
        // Position it
        Canvas.SetLeft(newControl, x);
        Canvas.SetTop(newControl, y);
        newControl.SetValue(Canvas.ZIndexProperty, 1); // Normal layer
        
        // Name it
        newControl.Name = $"{controlType}_{DateTime.Now.Ticks % 10000}";
        
        // Add to canvas
        _designCanvas.Children.Add(newControl);
        newControl.SetValue(Canvas.ZIndexProperty, 1); // Normal layer
        
        // Make it selectable - dummy controls don't consume events
        newControl.PointerPressed += OnControlPointerPressed;
        
        UpdateStatus($"Added {controlType} at ({(int)x}, {(int)y})", false);
        Console.WriteLine($"✅ Dummy control: Created {newControl.Name} at {(int)x},{(int)y}");
        
        // Auto-select
        SelectControl(newControl);
    }
    
    // TEST METHOD - Direct control creation
    public void TestCreateControl()
    {
        if (_designCanvas == null)
        {
            Console.WriteLine("❌ Canvas not found for test");
            return;
        }
        
        // Create test button directly
        var testButton = new Button 
        { 
            Content = "TEST BUTTON", 
            Width = 120, 
            Height = 40 
        };
        
        Canvas.SetLeft(testButton, 100);
        Canvas.SetTop(testButton, 100);
        testButton.Name = "TestButton_" + DateTime.Now.Ticks;
        
        _designCanvas.Children.Add(testButton);
        
        // Use AddHandler for button to receive handled events
        testButton.AddHandler(PointerPressedEvent, OnControlPointerPressed, handledEventsToo: true);
        
        Console.WriteLine($"✅ TEST: Created {testButton.Name} at 100,100");
        Console.WriteLine($"✅ TEST: Canvas now has {_designCanvas.Children.Count} children");
        
        UpdateStatus($"TEST: Added {testButton.Name}", false);
        SelectControl(testButton);
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







