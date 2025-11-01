using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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
    private Dictionary<Control, Dictionary<string, string>> _controlScripts = new();
    
    // Settings
    private bool _snapToGrid = false;
    private int _gridSize = 10;
    private bool _showGrid = true;
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
            WireActions();
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
        
        // Deselection (click on empty canvas)
        _designCanvas.PointerPressed += OnCanvasPointerPressed;
        
        Console.WriteLine("✅ Canvas events wired (GotFocus/LostFocus + Boundaries + ContextMenu)");
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
        
        // Handle control dragging (moving existing controls) - OPTIMIZED + BOUNDED!
        if (_isDraggingControl && _draggedControl != null && _dragTransform != null)
        {
            var deltaX = pos.X - _dragStartPoint.X;
            var deltaY = pos.Y - _dragStartPoint.Y;
            
            // 🎯 BOUNDARY CHECK: Constrain to canvas bounds
            var newLeft = _dragStartLeft + deltaX;
            var newTop = _dragStartTop + deltaY;
            var controlWidth = _draggedControl.Bounds.Width;
            var controlHeight = _draggedControl.Bounds.Height;
            var canvasWidth = _designCanvas.Bounds.Width;
            var canvasHeight = _designCanvas.Bounds.Height;
            
            // Clamp position to keep control within canvas
            newLeft = Math.Max(0, Math.Min(newLeft, canvasWidth - controlWidth));
            newTop = Math.Max(0, Math.Min(newTop, canvasHeight - controlHeight));
            
            // Recalculate constrained delta
            deltaX = newLeft - _dragStartLeft;
            deltaY = newTop - _dragStartTop;
            
            // 🚀 GPU-ACCELERATED: Just update the transform, NO layout recalculation!
            _dragTransform.X = deltaX;
            _dragTransform.Y = deltaY;

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
            // Calculate final position (already bounded during drag)
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
        
        // Position it (with boundary check)
        var width = double.Parse(properties["width"]);
        var height = double.Parse(properties["height"]);
        var left = Math.Max(0, Math.Min(pos.X, _designCanvas.Bounds.Width - width));
        var top = Math.Max(0, Math.Min(pos.Y, _designCanvas.Bounds.Height - height));
        
        Canvas.SetLeft(newControl, left);
        Canvas.SetTop(newControl, top);
        
        // Give it a unique name
        newControl.Name = $"{_dragControlType}_{DateTime.Now.Ticks % 10000}";
        
        // Add to canvas
        _designCanvas.Children.Add(newControl);
        newControl.SetValue(Canvas.ZIndexProperty, 1); // Normal layer
        
        // 🎯 FOCUS PATTERN: Wire focus events and make focusable
        WireControlEvents(newControl);
        
        UpdateStatus($"Added {_dragControlType} at ({(int)left}, {(int)top})", false);
        Console.WriteLine($"✅ Dummy control: Created {newControl.Name} at {(int)left},{(int)top}");
        
        // Auto-select the new control by focusing it
        newControl.Focus();
        
        // Reset drag state
        _isDragging = false;
        _dragControlType = null;
        if (_designCanvas != null)
            _designCanvas.Cursor = Cursor.Default;
    }
    
    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Click on empty canvas = deselect (by removing focus)
        if (e.Source == _designCanvas)
        {
            // Clear focus from any control - LostFocus will handle deselection
            _designCanvas.Focus();
        }
    }
    
    // 🎯 SIMPLIFIED: Click just starts drag, focus handles selection!
    private void OnControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && _designCanvas != null)
        {
            // Focus the control - GotFocus will handle selection
            control.Focus();
            
            // If already selected and in resize zone, ResizeBehavior handles it via routing
            if (control == _selectedControl && _resizeBehavior != null)
            {
                if (_resizeBehavior.HandlePointerPressed(sender, e))
                {
                    // ResizeBehavior is handling resize
                    return;
                }
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
    
    // 🎯 GOT FOCUS: Automatically attach resize behavior and show selection
    private void OnControlGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is Control control)
        {
            SelectControl(control);
            Console.WriteLine($"🎯 GotFocus: {control.Name} - selection attached");
        }
    }
    
    // 🎯 LOST FOCUS: Automatically detach resize behavior and clear selection
    private void OnControlLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender == _selectedControl)
        {
            DeselectControl();
            Console.WriteLine($"🎯 LostFocus: cleared selection");
        }
    }
    
    private void SelectControl(Control control)
    {
        // Deselect previous (if any)
        if (_selectedControl != null && _selectedControl != control)
        {
            DeselectControl();
        }
        
        _selectedControl = control;
        
        // 🚀 ATTACH RESIZE BEHAVIOR (with canvas bounds)!
        if (_designCanvas != null)
        {
            _resizeBehavior = new ResizeBehavior(
                control, 
                _designCanvas, 
                minWidth: 20, 
                minHeight: 20,
                maxWidth: _designCanvas.Bounds.Width,
                maxHeight: _designCanvas.Bounds.Height
            );
        }
        
        // Update properties panel
        UpdatePropertiesPanel(control);
        
        UpdateStatus($"Selected: {control.Name} ({control.GetType().Name}) - Drag to move, resize from edges, right-click for menu", false);
    }
    
    private void DeselectControl()
    {
        if (_selectedControl == null) return;
        
        // Clear resize behavior
        _resizeBehavior = null;
        _selectedControl = null;
        
        UpdateStatus("Ready", false);
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
            Focusable = true,  // 🎯 CRITICAL: Must be focusable!
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
    
    // 🎯 CREATE CONTEXT MENU for control
    private ContextMenu CreateControlContextMenu(Control control)
    {
        var menu = new ContextMenu();
        
        // Edit Script... 🎯
        var scriptItem = new MenuItem { Header = "Edit Script..." };
        scriptItem.Click += (s, e) => EditControlScript(control);
        menu.Items.Add(scriptItem);
        
        menu.Items.Add(new Separator());
        
        // Delete
        var deleteItem = new MenuItem { Header = "Delete" };
        deleteItem.Click += (s, e) => DeleteControl(control);
        menu.Items.Add(deleteItem);
        
        menu.Items.Add(new Separator());
        
        // Copy
        var copyItem = new MenuItem { Header = "Copy", IsEnabled = false };
        menu.Items.Add(copyItem);
        
        // Paste
        var pasteItem = new MenuItem { Header = "Paste", IsEnabled = false };
        menu.Items.Add(pasteItem);
        
        // Duplicate
        var duplicateItem = new MenuItem { Header = "Duplicate" };
        duplicateItem.Click += (s, e) => DuplicateControl(control);
        menu.Items.Add(duplicateItem);
        
        menu.Items.Add(new Separator());
        
        // Bring to Front
        var bringToFrontItem = new MenuItem { Header = "Bring to Front" };
        bringToFrontItem.Click += (s, e) => BringToFront(control);
        menu.Items.Add(bringToFrontItem);
        
        // Send to Back
        var sendToBackItem = new MenuItem { Header = "Send to Back" };
        sendToBackItem.Click += (s, e) => SendToBack(control);
        menu.Items.Add(sendToBackItem);
        
        menu.Items.Add(new Separator());
        
        // Properties
        var propertiesItem = new MenuItem { Header = "Properties" };
        propertiesItem.Click += (s, e) => ShowProperties(control);
        menu.Items.Add(propertiesItem);
        
        return menu;
    }
    
    // Context menu actions
    private void DeleteControl(Control control)
    {
        if (_designCanvas == null) return;
        
        _designCanvas.Children.Remove(control);
        if (_selectedControl == control)
        {
            DeselectControl();
        }
        
        Console.WriteLine($"🗑️ Deleted: {control.Name}");
        UpdateStatus($"Deleted {control.Name}", false);
    }
    
    private void DuplicateControl(Control control)
    {
        if (_designCanvas == null) return;
        
        // Get current properties
        var properties = new Dictionary<string, string>();
        properties["width"] = control.Width.ToString();
        properties["height"] = control.Height.ToString();
        
        if (control.Tag is string controlType)
        {
            // Offset the duplicate slightly
            var left = Canvas.GetLeft(control) + 20;
            var top = Canvas.GetTop(control) + 20;
            
            var newControl = CreateDummyControl(controlType, properties);
            Canvas.SetLeft(newControl, left);
            Canvas.SetTop(newControl, top);
            newControl.Name = $"{controlType}_{DateTime.Now.Ticks % 10000}";
            
            _designCanvas.Children.Add(newControl);
            WireControlEvents(newControl);
            
            newControl.Focus();
            Console.WriteLine($"📋 Duplicated: {control.Name} → {newControl.Name}");
        }
    }
    
    private void BringToFront(Control control)
    {
        if (_designCanvas == null) return;
        
        var maxZ = 0;
        foreach (var child in _designCanvas.Children)
        {
            var z = child.GetValue(Canvas.ZIndexProperty);
            if (z > maxZ) maxZ = z;
        }
        
        control.SetValue(Canvas.ZIndexProperty, maxZ + 1);
        Console.WriteLine($"⬆️ Brought to front: {control.Name} (Z={maxZ + 1})");
    }
    
    private void SendToBack(Control control)
    {
        control.SetValue(Canvas.ZIndexProperty, 0);
        Console.WriteLine($"⬇️ Sent to back: {control.Name} (Z=0)");
    }
    
    private void ShowProperties(Control control)
    {
        control.Focus();
        UpdateStatus($"Properties: {control.Name}", false);
    }
    
    // 🎯 WIRE FOCUS EVENTS: Called for every selectable control
    private void WireControlEvents(Control control)
    {
        // Make control focusable
        control.Focusable = true;
        
        // Wire pointer events for drag/resize
        control.PointerPressed += OnControlPointerPressed;
        
        // 🎯 Wire focus events for selection management
        control.GotFocus += OnControlGotFocus;
        control.LostFocus += OnControlLostFocus;
        
        // 🎯 CONTEXT MENU!
        control.ContextMenu = CreateControlContextMenu(control);
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
        
        // Position it (with boundary check)
        var width = properties.TryGetValue("width", out var w) ? double.Parse(w) : 100;
        var height = properties.TryGetValue("height", out var h) ? double.Parse(h) : 32;
        var left = Math.Max(0, Math.Min(x, _designCanvas.Bounds.Width - width));
        var top = Math.Max(0, Math.Min(y, _designCanvas.Bounds.Height - height));
        
        Canvas.SetLeft(newControl, left);
        Canvas.SetTop(newControl, top);
        newControl.SetValue(Canvas.ZIndexProperty, 1); // Normal layer
        
        // Name it
        newControl.Name = $"{controlType}_{DateTime.Now.Ticks % 10000}";
        
        // Add to canvas
        _designCanvas.Children.Add(newControl);
        
        // 🎯 Wire focus events + context menu
        WireControlEvents(newControl);
        
        UpdateStatus($"Added {controlType} at ({(int)left}, {(int)top})", false);
        Console.WriteLine($"✅ Dummy control: Created {newControl.Name} at {(int)left},{(int)top}");
        
        // Auto-select by focusing
        newControl.Focus();
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
            Height = 40,
            Focusable = true  // 🎯 Make focusable
        };
        
        Canvas.SetLeft(testButton, 100);
        Canvas.SetTop(testButton, 100);
        testButton.Name = "TestButton_" + DateTime.Now.Ticks;
        
        _designCanvas.Children.Add(testButton);
        
        // 🎯 Wire focus events + context menu
        WireControlEvents(testButton);
        
        Console.WriteLine($"✅ TEST: Created {testButton.Name} at 100,100");
        Console.WriteLine($"✅ TEST: Canvas now has {_designCanvas.Children.Count} children");
        
        UpdateStatus($"TEST: Added {testButton.Name}", false);
        testButton.Focus();
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
    
    private void EditControlScript(Control control)
    {
        // Get or create script dictionary for this control
        if (!_controlScripts.ContainsKey(control))
        {
            _controlScripts[control] = new Dictionary<string, string>();
        }
        
        // Open script editor window
        var scriptEditor = new ScriptEditorWindow(control, _controlScripts[control]);
        scriptEditor.Show();
        
        Console.WriteLine($"📝 Opening script editor for {control.Name}");
    }
    
    private double SnapToGrid(double value)
    {
        if (!_snapToGrid) return value;
        return Math.Round(value / _gridSize) * _gridSize;
    }
    
    public void ShowSettings()
    {
        var settingsWindow = new SettingsWindow(_snapToGrid, _gridSize, _showGrid);
        settingsWindow.SettingsChanged += (s, e) =>
        {
            if (s is SettingsWindow sw)
            {
                _snapToGrid = sw.SnapToGrid;
                _gridSize = sw.GridSize;
                _showGrid = sw.ShowGrid;
                UpdateStatus($"Settings: Snap={_snapToGrid}, Grid={_gridSize}px", false);
            }
        };
        settingsWindow.Show();
    }

    private void WireActions()
    {
        if (_builder?.Actions == null) return;
        
        foreach (var action in _builder.Actions)
        {
            if (action.Control is Button button)
            {
                button.Click += (sender, e) => HandleAction(action.ActionName, action.Parameters);
            }
            else if (action.Control is MenuItem menuItem)
            {
                menuItem.Click += (sender, e) => HandleAction(action.ActionName, action.Parameters);
            }
        }
    }

    private void HandleAction(string actionName, Dictionary<string, string> parameters)
    {
        switch (actionName)
        {
            case "panel.toggle":
                if (parameters.TryGetValue("panel", out var panelName))
                {
                    TogglePanel(panelName);
                }
                break;
        }
    }

    private void TogglePanel(string panelName)
    {
        var container = FindControlByName(Content as Control, panelName + "Container");
        if (container != null)
        {
            container.IsVisible = !container.IsVisible;
        }
    }

    private Control? FindControlByName(Control? root, string name)
    {
        if (root == null) return null;
        if (root.Name == name) return root;
        
        if (root is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control ctrl)
                {
                    var found = FindControlByName(ctrl, name);
                    if (found != null) return found;
                }
            }
        }
        else if (root is ContentControl contentControl && contentControl.Content is Control content)
        {
            return FindControlByName(content, name);
        }
        else if (root is Decorator decorator && decorator.Child is Control child)
        {
            return FindControlByName(child, name);
        }
        
        return null;
    }
}
