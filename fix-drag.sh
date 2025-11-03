#!/bin/bash
DB="visualised.db"

# Create fixed MainWindow with simpler drag logic
cat > /tmp/fixed-mainwindow.cs << 'ENDCS'
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
            var btn = CreateToolboxButton(controlType);
            stack.Children.Add(btn);
        }
    }
    
    private Button CreateToolboxButton(string controlType)
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
    }
    
    private void StartToolboxDrag(string controlType, Button sourceButton)
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
    
    private void HandleSelectAll(object? sender, RoutedEventArgs e) { }
    private void HandlePropertiesWindow(object? sender, RoutedEventArgs e) { }
    private void HandleToolboxWindow(object? sender, RoutedEventArgs e) { }
    private void HandleZoomIn(object? sender, RoutedEventArgs e) { }
    private void HandleZoomOut(object? sender, RoutedEventArgs e) { }
    private void HandleZoomReset(object? sender, RoutedEventArgs e) { }
    private void HandleAlignLeft(object? sender, RoutedEventArgs e) { }
    private void HandleAlignCenter(object? sender, RoutedEventArgs e) { }
    private void HandleAlignRight(object? sender, RoutedEventArgs e) { }
    private void HandleAlignTop(object? sender, RoutedEventArgs e) { }
    private void HandleAlignMiddle(object? sender, RoutedEventArgs e) { }
    private void HandleAlignBottom(object? sender, RoutedEventArgs e) { }
    private async void HandleOptions(object? sender, RoutedEventArgs e) { }
    private void HandleDocumentation(object? sender, RoutedEventArgs e) { }
    private void HandleVMLReference(object? sender, RoutedEventArgs e) { }
    
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
    
    private Control? CreateControl(string? typeName)
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
        
        return control;
    }
}
ENDCS

# Update in DB with proper escaping
CONTENT=$(cat /tmp/fixed-mainwindow.cs | sed "s/'/''/g")
TIMESTAMP=$(date -u +"%Y-%m-%d %H:%M:%S")

sqlite3 $DB << SQL
UPDATE project_files 
SET content = '$CONTENT',
    updated_at = '$TIMESTAMP',
    version = version + 1
WHERE filename = 'MainWindow.axaml.cs';
SQL

# Extract from DB to file
sqlite3 $DB "SELECT content FROM project_files WHERE filename='MainWindow.axaml.cs';" > MainWindow.axaml.cs

echo "✓ Fixed MainWindow via SQL"
