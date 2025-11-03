using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using System;
using System.Threading.Tasks;

namespace VB;

public partial class MainWindow : Window
{
    private Canvas? designCanvas;
    private Random random = new Random();
    private TextBlock? statusControl;
    private TextBlock? statusWindowSize;
    private TextBlock? statusCanvasSize;
    private Control? selectedControl;
    private Border? selectionBorder;
    
    public MainWindow()
    {
        InitializeComponent();
        SizeChanged += MainWindow_SizeChanged;
        
        // Wire up main window pointer events for toolbox drag
        this.PointerMoved += MainWindow_PointerMoved;
        this.PointerReleased += MainWindow_PointerReleased;
    }
    
    private bool isToolboxDragging = false;
    private Border? dragGhost;
    private string? dragControlType;
    
    public void InitializeDesigner(
        StackPanel? commonStack, 
        StackPanel? inputStack,
        StackPanel? layoutStack,
        StackPanel? displayStack,
        StackPanel? containerStack,
        Canvas? canvas,
        TextBlock? statusCtrl,
        TextBlock? statusWinSize,
        TextBlock? statusCanSize)
    {
        designCanvas = canvas;
        statusControl = statusCtrl;
        statusWindowSize = statusWinSize;
        statusCanvasSize = statusCanSize;
        
        if (designCanvas != null)
        {
            designCanvas.PointerPressed += Canvas_PointerPressed;
        }
        
        UpdateStatus();
        GenerateToolbox(commonStack, inputStack, layoutStack, displayStack, containerStack);
        
        Console.WriteLine("[INIT] Designer initialized - drag-drop ready!");
    }
    
    private void GenerateToolbox(
        StackPanel? commonStack,
        StackPanel? inputStack, 
        StackPanel? layoutStack,
        StackPanel? displayStack,
        StackPanel? containerStack)
    {
        if (commonStack == null) return;
        
        var common = new[] { "Button", "TextBlock", "Label", "Image" };
        var input = new[] { "TextBox", "CheckBox", "RadioButton", "ComboBox", "ListBox", "Slider" };
        var layout = new[] { "StackPanel", "Grid", "DockPanel", "Canvas" };
        var display = new[] { "ProgressBar", "DataGrid", "TreeView", "ListView" };
        var container = new[] { "Border", "ScrollViewer", "Viewbox" };
        
        AddToolboxButtons(commonStack, common);
        if (inputStack != null) AddToolboxButtons(inputStack, input);
        if (layoutStack != null) AddToolboxButtons(layoutStack, layout);
        if (displayStack != null) AddToolboxButtons(displayStack, display);
        if (containerStack != null) AddToolboxButtons(containerStack, container);
    }
    
    private void AddToolboxButtons(StackPanel stack, string[] controls)
    {
        foreach (var controlType in controls)
        {
            var dummy = CreateToolboxButton(controlType);
            stack.Children.Add(dummy);
        }
    }
    
    private Border CreateToolboxButton(string controlType)
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
    }
    
    private void StartToolboxDrag(string controlType, Border sourceDummy)
    {
        Console.WriteLine($"[DRAG] Starting drag: {controlType}");
        isToolboxDragging = true;
        dragControlType = controlType;
        
        // Create blue ghost
        dragGhost = new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#2196F3")),
            BorderThickness = new Avalonia.Thickness(2),
            Background = new SolidColorBrush(Color.FromArgb(30, 33, 150, 243)),
            Width = 100,
            Height = 30,
            IsHitTestVisible = false,
            Child = new TextBlock 
            { 
                Text = controlType, 
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#2196F3"))
            }
        };
        
        if (statusControl != null)
        {
            statusControl.Text = $"Dragging: {controlType}";
        }
    }
    
    private void MainWindow_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isToolboxDragging || dragGhost == null || designCanvas == null) return;
        
        // Add ghost to canvas if not there
        if (!designCanvas.Children.Contains(dragGhost))
        {
            designCanvas.Children.Add(dragGhost);
            Console.WriteLine("[DRAG] Ghost added to canvas");
        }
        
        // Position ghost at cursor
        var canvasPoint = e.GetPosition(designCanvas);
        Canvas.SetLeft(dragGhost, canvasPoint.X - dragGhost.Width / 2);
        Canvas.SetTop(dragGhost, canvasPoint.Y - dragGhost.Height / 2);
    }
    
    private void MainWindow_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!isToolboxDragging) return;
        
        Console.WriteLine("[DRAG] Pointer released");
        
        if (designCanvas != null && dragGhost != null)
        {
            var canvasPoint = e.GetPosition(designCanvas);
            var canvasBounds = new Rect(0, 0, designCanvas.Bounds.Width, designCanvas.Bounds.Height);
            
            if (canvasBounds.Contains(canvasPoint))
            {
                Console.WriteLine($"[DRAG] Drop on canvas at {canvasPoint}");
                var control = CreateControl(dragControlType);
                
                if (control != null)
                {
                    var x = Math.Max(0, canvasPoint.X - 50);
                    var y = Math.Max(0, canvasPoint.Y - 15);
                    
                    Canvas.SetLeft(control, x);
                    Canvas.SetTop(control, y);
                    designCanvas.Children.Add(control);
                    
                    SelectControl(control);
                    
                    if (statusControl != null)
                    {
                        statusControl.Text = $"Added: {dragControlType}";
                    }
                }
            }
            else
            {
                Console.WriteLine("[DRAG] Drop outside canvas - cancelled");
                if (statusControl != null)
                {
                    statusControl.Text = "Drag cancelled";
                }
            }
            
            designCanvas.Children.Remove(dragGhost);
        }
        
        dragGhost = null;
        isToolboxDragging = false;
        dragControlType = null;
    }
    
    private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateStatus();
    }
    
    private void UpdateStatus()
    {
        if (statusWindowSize != null)
            statusWindowSize.Text = $"Window: {(int)Bounds.Width}x{(int)Bounds.Height}";
        
        if (statusCanvasSize != null && designCanvas != null)
            statusCanvasSize.Text = $"Canvas: {(int)designCanvas.Width}x{(int)designCanvas.Height}";
        
        if (statusControl != null && selectedControl != null)
        {
            var x = Canvas.GetLeft(selectedControl);
            var y = Canvas.GetTop(selectedControl);
            statusControl.Text = $"Selected: {selectedControl.GetType().Name} ({(int)x},{(int)y})";
        }
        else if (statusControl != null && !isToolboxDragging)
        {
            statusControl.Text = "Ready";
        }
    }
    
    // CANVAS SELECTION
    private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (designCanvas == null) return;
        
        var point = e.GetPosition(designCanvas);
        Control? hitControl = null;
        
        foreach (var child in designCanvas.Children)
        {
            if (child is Control control && control != selectionBorder && control != dragGhost)
            {
                var left = Canvas.GetLeft(control);
                var top = Canvas.GetTop(control);
                var bounds = new Rect(left, top, control.Bounds.Width, control.Bounds.Height);
                
                if (bounds.Contains(point))
                {
                    hitControl = control;
                }
            }
        }
        
        if (hitControl != null)
        {
            Console.WriteLine($"[SELECT] Control clicked: {hitControl.GetType().Name}");
            SelectControl(hitControl);
        }
        else
        {
            Console.WriteLine("[SELECT] Canvas clicked - deselect");
            DeselectControl();
        }
    }
    
    private void SelectControl(Control control)
    {
        selectedControl = control;
        
        if (selectionBorder != null && designCanvas != null)
        {
            designCanvas.Children.Remove(selectionBorder);
        }
        
        if (designCanvas != null)
        {
            var left = Canvas.GetLeft(control);
            var top = Canvas.GetTop(control);
            
            selectionBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.Parse("#2196F3")),
                BorderThickness = new Avalonia.Thickness(2),
                Background = Brushes.Transparent,
                Width = control.Bounds.Width,
                Height = control.Bounds.Height,
                IsHitTestVisible = false
            };
            
            Canvas.SetLeft(selectionBorder, left);
            Canvas.SetTop(selectionBorder, top);
            designCanvas.Children.Add(selectionBorder);
        }
        
        UpdateStatus();
    }
    
    private void DeselectControl()
    {
        selectedControl = null;
        
        if (selectionBorder != null && designCanvas != null)
        {
            designCanvas.Children.Remove(selectionBorder);
            selectionBorder = null;
        }
        
        UpdateStatus();
    }
    
    // MENU HANDLERS (simplified)
    private void HandleNew(object? sender, RoutedEventArgs e)
    {
        if (designCanvas != null)
        {
            designCanvas.Children.Clear();
            DeselectControl();
            if (statusControl != null) statusControl.Text = "New project";
        }
    }
    
    private async void HandleOpen(object? sender, RoutedEventArgs e)
    {
        var result = await new OpenFileDialog { Title = "Open 🌳Visualised Project" }.ShowAsync(this);
        if (result?.Length > 0 && statusControl != null)
            statusControl.Text = $"Opened: {System.IO.Path.GetFileName(result[0])}";
    }
    
    private async void HandleSave(object? sender, RoutedEventArgs e)
    {
        if (statusControl != null) statusControl.Text = "Saved";
    }
    
    private async void HandleSaveAs(object? sender, RoutedEventArgs e)
    {
        var result = await new SaveFileDialog { Title = "Save As" }.ShowAsync(this);
        if (result != null && statusControl != null)
            statusControl.Text = $"Saved: {System.IO.Path.GetFileName(result)}";
    }
    
    private void HandleExit(object? sender, RoutedEventArgs e) => Close();
    private void HandleUndo(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Undo NYI"; }
    private void HandleRedo(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Redo NYI"; }
    private void HandleCut(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Cut"; }
    private void HandleCopy(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Copy"; }
    private void HandlePaste(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Paste"; }
    
    private void HandleDelete(object? sender, RoutedEventArgs e)
    {
        if (selectedControl != null && designCanvas != null)
        {
            designCanvas.Children.Remove(selectedControl);
            DeselectControl();
            if (statusControl != null) statusControl.Text = "Deleted";
        }
    }
    
    private void HandleSelectAll(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Select All NYI"; }
    private void HandlePropertiesWindow(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Toggle Props"; }
    private void HandleToolboxWindow(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Toggle Toolbox"; }
    private void HandleZoomIn(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Zoom In"; }
    private void HandleZoomOut(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Zoom Out"; }
    private void HandleZoomReset(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Zoom Reset"; }
    private void HandleAlignLeft(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Align Left"; }
    private void HandleAlignCenter(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Align Center"; }
    private void HandleAlignRight(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Align Right"; }
    private void HandleAlignTop(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Align Top"; }
    private void HandleAlignMiddle(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Align Middle"; }
    private void HandleAlignBottom(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Align Bottom"; }
    private async void HandleOptions(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Options NYI"; }
    private void HandleDocumentation(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "Docs NYI"; }
    private void HandleVMLReference(object? sender, RoutedEventArgs e) { if (statusControl != null) statusControl.Text = "VML Ref NYI"; }
    
    private async void HandleAbout(object? sender, RoutedEventArgs e)
    {
        var w = new Window { Title = "About", Width = 400, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var s = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        s.Children.Add(new TextBlock { Text = "🌳Visualised V1.0", FontSize = 20, FontWeight = FontWeight.Bold });
        s.Children.Add(new TextBlock { Text = "Steve & Claude", FontSize = 12 });
        var btn = new Button { Content = "Close", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        btn.Click += (ss, ee) => w.Close();
        s.Children.Add(btn);
        w.Content = s;
        await w.ShowDialog(this);
    }
    
    private Border CreateControl(string? typeName)
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
}
