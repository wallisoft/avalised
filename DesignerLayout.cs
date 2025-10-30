using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Input;
using System;
using System.Linq;

namespace Avalised;

public class DesignerLayout : DockPanel
{
    private readonly TextBlock _statusText;
    private readonly Ellipse _statusDot;
    private readonly TextBlock _windowText;
    private readonly TextBlock _mouseText;
    private readonly DetachablePanel _toolboxPanel;
    private readonly DetachablePanel _propertiesPanel;
    private readonly Canvas _designCanvas;
    private readonly Canvas _gridCanvas;
    private Control? _selectedControl;
    private StackPanel? _propertiesStack;
    private Control? _draggedControl;
    private Point _dragStartPoint;
    private bool _isDragging;
    private Border? _selectionBorder;
    private Rectangle?[] _resizeHandles = new Rectangle?[8];
    private bool _isResizing;
    private int _resizeHandleIndex = -1;
    private bool _isPreviewMode = false;
    
    public DesignerLayout(string dbPath)
    {
        LastChildFill = true;
        
        _statusDot = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Red,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        _statusText = new TextBlock
        {
            Text = "Ready",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 13,
            FontFamily = new FontFamily("Segoe UI")
        };
        
        _windowText = new TextBlock
        {
            Text = "Window: 1280×720",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11,
            FontFamily = new FontFamily("Segoe UI")
        };
        
        _mouseText = new TextBlock
        {
            Text = "Mouse: 0, 0",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11,
            FontFamily = new FontFamily("Segoe UI")
        };
        
        _designCanvas = new Canvas();
        SetupCanvasHandlers();
        
        _toolboxPanel = new DetachablePanel("Toolbox", CreateToolboxContent);
        _propertiesPanel = new DetachablePanel("Properties", CreatePropertiesContent);
        
        var statusBar = CreateStatusBar(dbPath);
        DockPanel.SetDock(statusBar, Dock.Bottom);
        Children.Add(statusBar);
        
        Children.Add(CreateMainGrid());
    }
    
    private Grid CreateMainGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("179,2,*,2,259"),
            Margin = new Avalonia.Thickness(2, 2, 2, 2)
        };
        
        _toolboxPanel.Width = 179;
        Grid.SetColumn(_toolboxPanel, 0);
        grid.Children.Add(_toolboxPanel);
        
        var canvasPanel = CreateCanvasPanel();
        Grid.SetColumn(canvasPanel, 2);
        grid.Children.Add(canvasPanel);
        
        _propertiesPanel.Width = 259;
        Grid.SetColumn(_propertiesPanel, 4);
        grid.Children.Add(_propertiesPanel);
        
        return grid;
    }
    
    private void SetupCanvasHandlers()
    {
        _designCanvas.PointerPressed += (s, e) =>
        {
            if (_draggedControl != null)
            {
                var pos = e.GetPosition(_designCanvas);
                var x = Math.Round(pos.X / 10) * 10;
                var y = Math.Round(pos.Y / 10) * 10;
                
                Canvas.SetLeft(_draggedControl, x);
                Canvas.SetTop(_draggedControl, y);
                _designCanvas.Children.Add(_draggedControl);
                
                SetupControlHandlers(_draggedControl);
                SelectControl(_draggedControl);
                
                UpdateStatus($"Added {_draggedControl.Tag} at ({(int)x}, {(int)y})", false);
                
                _draggedControl = null;
            }
            else if (!_isResizing && !_isDragging)
            {
                ClearSelection();
            }
        };
        
        _designCanvas.PointerMoved += (s, e) =>
        {
            var pos = e.GetPosition(_designCanvas);
            _mouseText.Text = $"Mouse: {(int)pos.X}, {(int)pos.Y}";
            
            if (_isResizing && _selectedControl != null && _resizeHandleIndex >= 0)
            {
                var currentPos = e.GetPosition(_designCanvas);
                var deltaX = currentPos.X - _dragStartPoint.X;
                var deltaY = currentPos.Y - _dragStartPoint.Y;
                
                var left = Canvas.GetLeft(_selectedControl);
                var top = Canvas.GetTop(_selectedControl);
                var width = _selectedControl.Width;
                var height = _selectedControl.Height;
                
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;
                if (double.IsNaN(width)) width = _selectedControl.Bounds.Width;
                if (double.IsNaN(height)) height = _selectedControl.Bounds.Height;
                
                switch (_resizeHandleIndex)
                {
                    case 0:
                        Canvas.SetLeft(_selectedControl, left + deltaX);
                        Canvas.SetTop(_selectedControl, top + deltaY);
                        _selectedControl.Width = Math.Max(20, width - deltaX);
                        _selectedControl.Height = Math.Max(20, height - deltaY);
                        break;
                    case 1:
                        Canvas.SetTop(_selectedControl, top + deltaY);
                        _selectedControl.Height = Math.Max(20, height - deltaY);
                        break;
                    case 2:
                        Canvas.SetTop(_selectedControl, top + deltaY);
                        _selectedControl.Width = Math.Max(20, width + deltaX);
                        _selectedControl.Height = Math.Max(20, height - deltaY);
                        break;
                    case 3:
                        _selectedControl.Width = Math.Max(20, width + deltaX);
                        break;
                    case 4:
                        _selectedControl.Width = Math.Max(20, width + deltaX);
                        _selectedControl.Height = Math.Max(20, height + deltaY);
                        break;
                    case 5:
                        _selectedControl.Height = Math.Max(20, height + deltaY);
                        break;
                    case 6:
                        Canvas.SetLeft(_selectedControl, left + deltaX);
                        _selectedControl.Width = Math.Max(20, width - deltaX);
                        _selectedControl.Height = Math.Max(20, height + deltaY);
                        break;
                    case 7:
                        Canvas.SetLeft(_selectedControl, left + deltaX);
                        _selectedControl.Width = Math.Max(20, width - deltaX);
                        break;
                }
                
                _dragStartPoint = currentPos;
                UpdateHandlePositions();
            }
            else if (_isDragging && _selectedControl != null)
            {
                var currentPos = e.GetPosition(_designCanvas);
                var deltaX = currentPos.X - _dragStartPoint.X;
                var deltaY = currentPos.Y - _dragStartPoint.Y;
                
                var currentLeft = Canvas.GetLeft(_selectedControl);
                var currentTop = Canvas.GetTop(_selectedControl);
                
                if (double.IsNaN(currentLeft)) currentLeft = 0;
                if (double.IsNaN(currentTop)) currentTop = 0;
                
                var newLeft = currentLeft + deltaX;
                var newTop = currentTop + deltaY;
                
                var width = double.IsNaN(_selectedControl.Width) ? _selectedControl.Bounds.Width : _selectedControl.Width;
                var height = double.IsNaN(_selectedControl.Height) ? _selectedControl.Bounds.Height : _selectedControl.Height;
                
                newLeft = Math.Max(0, Math.Min(newLeft, 800 - width));
                newTop = Math.Max(0, Math.Min(newTop, 600 - height));
                
                newLeft = Math.Round(newLeft / 10) * 10;
                newTop = Math.Round(newTop / 10) * 10;
                
                Canvas.SetLeft(_selectedControl, newLeft);
                Canvas.SetTop(_selectedControl, newTop);
                
                _dragStartPoint = currentPos;
                UpdateHandlePositions();
            }
        };
        
        _designCanvas.PointerReleased += (s, e) =>
        {
            _isDragging = false;
            _isResizing = false;
            _resizeHandleIndex = -1;
        };
    }
    
private void SetupControlHandlers(Control control)
{
    control.PointerPressed += (s, e) =>
    {
        // DISABLED: Context menu to test if it's interfering
        // if (e.GetCurrentPoint(control).Properties.IsRightButtonPressed)
        // {
        //     ShowContextMenu(control, e.GetPosition(_designCanvas));
        // }
        if (!_isPreviewMode)
        {
            SelectControl(control);
            _isDragging = true;
            _dragStartPoint = e.GetPosition(_designCanvas);
            
            // CRITICAL: Capture pointer so we get events outside control bounds!
            e.Pointer.Capture(control);
            
            // CRITICAL: Handle move/release on the control itself during drag!
            void onMove(object? s2, PointerEventArgs e2)
            {
                if (_isDragging && _selectedControl != null)
                {
                    var currentPos = e2.GetPosition(_designCanvas);
                    var deltaX = currentPos.X - _dragStartPoint.X;
                    var deltaY = currentPos.Y - _dragStartPoint.Y;
                    
                    var currentLeft = Canvas.GetLeft(_selectedControl);
                    var currentTop = Canvas.GetTop(_selectedControl);
                    
                    if (double.IsNaN(currentLeft)) currentLeft = 0;
                    if (double.IsNaN(currentTop)) currentTop = 0;
                    
                    var newLeft = currentLeft + deltaX;
                    var newTop = currentTop + deltaY;
                    
                    var width = double.IsNaN(_selectedControl.Width) ? _selectedControl.Bounds.Width : _selectedControl.Width;
                    var height = double.IsNaN(_selectedControl.Height) ? _selectedControl.Bounds.Height : _selectedControl.Height;
                    
                    newLeft = Math.Max(0, Math.Min(newLeft, 800 - width));
                    newTop = Math.Max(0, Math.Min(newTop, 600 - height));
                    
                    newLeft = Math.Round(newLeft / 10) * 10;
                    newTop = Math.Round(newTop / 10) * 10;
                    
                    Canvas.SetLeft(_selectedControl, newLeft);
                    Canvas.SetTop(_selectedControl, newTop);
                    
                    _dragStartPoint = currentPos;
                    UpdateHandlePositions();
                }
            }
            
            void onRelease(object? s2, PointerReleasedEventArgs e2)
            {
                _isDragging = false;
                e2.Pointer.Capture(null);  // Release capture!
                control.PointerMoved -= onMove;
                control.PointerReleased -= onRelease;
            }
            
            control.PointerMoved += onMove;
            control.PointerReleased += onRelease;
            e.Handled = true;  // Stop event bubbling!
        }
    };
}
    
    private void SelectControl(Control control)
    {
        _selectedControl = control;
        ShowPropertiesFor(control);
        ShowSelectionBorder(control);
        UpdateStatus($"Selected: {control.Tag ?? control.GetType().Name}", false);
    }
    
    private void ClearSelection()
    {
        _selectedControl = null;
        HideSelectionBorder();
        ShowPropertiesFor(_designCanvas);
    }
    
 private void ShowSelectionBorder(Control control)
    {
        HideSelectionBorder();
        
        var left = Canvas.GetLeft(control);
        var top = Canvas.GetTop(control);
        
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        
        var width = double.IsNaN(control.Width) ? control.Bounds.Width : control.Width;
        var height = double.IsNaN(control.Height) ? control.Bounds.Height : control.Height;
        
        // TRANSPARENT DRAG LAYER - VB5 style!
        _selectionBorder = new Border
        {
            BorderBrush = Brush.Parse("#2196F3"),
            BorderThickness = new Avalonia.Thickness(2),
            Width = width + 4,
            Height = height + 4,
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
            Cursor = new Cursor(StandardCursorType.SizeAll)
        };
        
        Canvas.SetLeft(_selectionBorder, left - 2);
        Canvas.SetTop(_selectionBorder, top - 2);
        
        // DRAG HANDLERS on the border
        _selectionBorder.PointerPressed += (s, e) =>
        {
            if (!_isResizing && !e.GetCurrentPoint(_selectionBorder).Properties.IsRightButtonPressed)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(_designCanvas);
                
                // Hide handles during drag
                foreach (var handle in _resizeHandles)
                {
                    if (handle != null) handle.IsVisible = false;
                }
                
                e.Handled = true;
            }
        };
        
        _selectionBorder.PointerMoved += (s, e) =>
        {
            if (_isDragging && _selectedControl != null)
            {
                var currentPos = e.GetPosition(_designCanvas);
                var deltaX = currentPos.X - _dragStartPoint.X;
                var deltaY = currentPos.Y - _dragStartPoint.Y;
                
                var currentLeft = Canvas.GetLeft(_selectedControl);
                var currentTop = Canvas.GetTop(_selectedControl);
                
                if (double.IsNaN(currentLeft)) currentLeft = 0;
                if (double.IsNaN(currentTop)) currentTop = 0;
                
                var newLeft = currentLeft + deltaX;
                var newTop = currentTop + deltaY;
                
                newLeft = Math.Max(0, Math.Min(newLeft, 800 - width));
                newTop = Math.Max(0, Math.Min(newTop, 600 - height));
                
                Canvas.SetLeft(_selectedControl, newLeft);
                Canvas.SetTop(_selectedControl, newTop);
                Canvas.SetLeft(_selectionBorder, newLeft - 2);
                Canvas.SetTop(_selectionBorder, newTop - 2);
                
                _dragStartPoint = currentPos;
            }
        };
        
        _selectionBorder.PointerReleased += (s, e) =>
        {
            if (_isResizing)
            {
                return; // Don't refresh if resizing
            }
            
            _isDragging = false;
            
            // NUCLEAR refresh after drag
            if (_selectedControl != null)
            {
                _designCanvas.Children.Remove(_selectionBorder);
                foreach (var handle in _resizeHandles)
                {
                    if (handle != null) _designCanvas.Children.Remove(handle);
                }
                _resizeHandles = new Rectangle?[8];
                _selectionBorder = null;
                
                ShowSelectionBorder(_selectedControl);
            }
        };
        
        _designCanvas.Children.Add(_selectionBorder);
        AddResizeHandles(left, top, width, height);
    }
    
    private void AddResizeHandles(double left, double top, double width, double height)
    {
        int handleSize = 6;
        var handleBrush = Brush.Parse("#2196F3");
        
        var positions = new[]
        {
            (left - handleSize/2.0, top - handleSize/2.0, StandardCursorType.TopLeftCorner),
            (left + width/2 - handleSize/2.0, top - handleSize/2.0, StandardCursorType.TopSide),
            (left + width - handleSize/2.0, top - handleSize/2.0, StandardCursorType.TopRightCorner),
            (left + width - handleSize/2.0, top + height/2 - handleSize/2.0, StandardCursorType.RightSide),
            (left + width - handleSize/2.0, top + height - handleSize/2.0, StandardCursorType.BottomRightCorner),
            (left + width/2 - handleSize/2.0, top + height - handleSize/2.0, StandardCursorType.BottomSide),
            (left - handleSize/2.0, top + height - handleSize/2.0, StandardCursorType.BottomLeftCorner),
            (left - handleSize/2.0, top + height/2 - handleSize/2.0, StandardCursorType.LeftSide)
        };
        
        for (int i = 0; i < positions.Length; i++)
        {
            var (x, y, cursorType) = positions[i];
            var handle = new Rectangle
            {
                Width = handleSize,
                Height = handleSize,
                Fill = handleBrush,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Cursor = new Cursor(cursorType),
                Tag = i
            };
            
            handle.PointerPressed += (s, e) =>
            {
                _isResizing = true;
                _isDragging = false;
                _resizeHandleIndex = (int)((Rectangle)s!).Tag!;
                _dragStartPoint = e.GetPosition(_designCanvas);
                e.Pointer.Capture((Rectangle)s!);  // Capture for resize!
                e.Handled = true;
            };

            handle.PointerReleased += (s, e) =>
            {
                _isResizing = false;
                _resizeHandleIndex = -1;
                e.Pointer.Capture(null);  // Release capture!
                
                // Refresh border to match new control size!
                if (_selectedControl != null)
                {
                    HideSelectionBorder();
                    ShowSelectionBorder(_selectedControl);
                }
                
                e.Handled = true;
            };
            
            Canvas.SetLeft(handle, x);
            Canvas.SetTop(handle, y);
            _designCanvas.Children.Add(handle);
            _resizeHandles[i] = handle;
        }
    }
    
    private void UpdateSelectionBorder()
    {
        if (_selectedControl != null)
        {
            ShowSelectionBorder(_selectedControl);
        }
    }

    private void UpdateHandlePositions()
    {
        if (_selectedControl == null || _selectionBorder == null) return;
        
        var left = Canvas.GetLeft(_selectedControl);
        var top = Canvas.GetTop(_selectedControl);
        var width = double.IsNaN(_selectedControl.Width) ? _selectedControl.Bounds.Width : _selectedControl.Width;
        var height = double.IsNaN(_selectedControl.Height) ? _selectedControl.Bounds.Height : _selectedControl.Height;
        
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        
        _selectionBorder.Width = width + 4;
        _selectionBorder.Height = height + 4;
        Canvas.SetLeft(_selectionBorder, left - 2);
        Canvas.SetTop(_selectionBorder, top - 2);
        
        int handleSize = 6;
        var positions = new[]
        {
            (left - handleSize/2.0, top - handleSize/2.0),
            (left + width/2 - handleSize/2.0, top - handleSize/2.0),
            (left + width - handleSize/2.0, top - handleSize/2.0),
            (left + width - handleSize/2.0, top + height/2 - handleSize/2.0),
            (left + width - handleSize/2.0, top + height - handleSize/2.0),
            (left + width/2 - handleSize/2.0, top + height - handleSize/2.0),
            (left - handleSize/2.0, top + height - handleSize/2.0),
            (left - handleSize/2.0, top + height/2 - handleSize/2.0)
        };
        
        for (int i = 0; i < _resizeHandles.Length; i++)
        {
            if (_resizeHandles[i] != null)
            {
                Canvas.SetLeft(_resizeHandles[i], positions[i].Item1);
                Canvas.SetTop(_resizeHandles[i], positions[i].Item2);
            }
        }
    }

    
    private void HideSelectionBorder()
    {
        if (_selectionBorder != null)
        {
            _designCanvas.Children.Remove(_selectionBorder);
            _selectionBorder = null;
        }
        
        foreach (var handle in _resizeHandles)
        {
            if (handle != null)
            {
                _designCanvas.Children.Remove(handle);
            }
        }
        _resizeHandles = new Rectangle?[8];
    }
    
    private void ShowContextMenu(Control control, Point position)
    {
        var menu = new ContextMenu
        {
            FontSize = 11,
            FontFamily = new FontFamily("Segoe UI"),
            Background = Brushes.White,
            BorderBrush = Brushes.Black,
            BorderThickness = new Avalonia.Thickness(2)
        };
        
        var cutItem = new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) };
        var copyItem = new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) };
        var pasteItem = new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) };
        var deleteItem = new MenuItem { Header = "Delete", InputGesture = new KeyGesture(Key.Delete) };
        
        var sep1 = new Separator();
        
        var bringToFrontItem = new MenuItem { Header = "Bring to Front" };
        bringToFrontItem.Click += (s, e) => BringToFront(control);
        
        var sendToBackItem = new MenuItem { Header = "Send to Back" };
        sendToBackItem.Click += (s, e) => SendToBack(control);
        
        var sep2 = new Separator();
        
        var propertiesItem = new MenuItem { Header = "Properties", InputGesture = new KeyGesture(Key.F4) };
        propertiesItem.Click += (s, e) => ShowPropertiesFor(control);
        
        var editScriptItem = new MenuItem { Header = "▶ Edit Script...", FontWeight = FontWeight.SemiBold };
        editScriptItem.Click += async (s, e) =>
        {
            var scriptDialog = new Dialogs.ScriptEditorDialog(control);
            await scriptDialog.ShowDialog((Window)this.VisualRoot!);
        };
        
        deleteItem.Click += (s, e) => DeleteControl(control);
        
        menu.Items.Add(cutItem);
        menu.Items.Add(copyItem);
        menu.Items.Add(pasteItem);
        menu.Items.Add(deleteItem);
        menu.Items.Add(sep1);
        menu.Items.Add(bringToFrontItem);
        menu.Items.Add(sendToBackItem);
        menu.Items.Add(sep2);
        menu.Items.Add(propertiesItem);
        menu.Items.Add(editScriptItem);
        
        menu.Open(control);
    }
    
    private void BringToFront(Control control)
    {
        _designCanvas.Children.Remove(control);
        _designCanvas.Children.Add(control);
        ShowSelectionBorder(control);
    }
    
    private void SendToBack(Control control)
    {
        _designCanvas.Children.Remove(control);
        _designCanvas.Children.Insert(0, control);
        ShowSelectionBorder(control);
    }
    
    private void DeleteControl(Control control)
    {
        _designCanvas.Children.Remove(control);
        ClearSelection();
        UpdateStatus("Control deleted", false);
    }
    
    public void TogglePreviewMode()
    {
        _isPreviewMode = !_isPreviewMode;
        
        if (_isPreviewMode)
        {
            HideSelectionBorder();
            UpdateStatus("Preview Mode - Controls are live!", false);
        }
        else
        {
            UpdateStatus("Editor Mode - Drag to move, right-click for options", false);
        }
    }
    
    private Control CreateToolboxContent()
    {
        var mainStack = new StackPanel
        {
            Spacing = 4,
            Margin = new Avalonia.Thickness(3)
        };
        
        AddToolboxCategory(mainStack, "Common", new[] {
            "Button", "TextBox", "TextBlock", "Label", "CheckBox", "RadioButton", "ComboBox", "ListBox", "Slider"
        });
        
        AddToolboxCategory(mainStack, "Layout", new[] {
            "Panel", "StackPanel", "Grid", "DockPanel", "WrapPanel", "Canvas", "Border", "ScrollViewer", "Expander"
        });
        
        AddToolboxCategory(mainStack, "Display", new[] {
            "Image", "ProgressBar", "Separator", "Rectangle", "Ellipse", "Line"
        });
        
        AddToolboxCategory(mainStack, "Containers", new[] {
            "TabControl", "TabItem", "GroupBox", "Viewbox"
        });
        
        AddToolboxCategory(mainStack, "Data", new[] {
            "TreeView", "Calendar", "DatePicker", "TimePicker"
        });
        
        AddToolboxCategory(mainStack, "Menus", new[] {
            "Menu", "MenuItem"
        });
        
        return new ScrollViewer
        {
            Content = mainStack,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }
    
    private void AddToolboxCategory(StackPanel parent, string category, string[] controls)
    {
        var expander = new Expander
        {
            Header = new TextBlock
            {
                Text = category,
                FontSize = 10.8,
                FontWeight = FontWeight.SemiBold,
                FontFamily = new FontFamily("Segoe UI"),
                Padding = new Avalonia.Thickness(1.5)
            },
            IsExpanded = category == "Common",
            Margin = new Avalonia.Thickness(0, 0, 0, 2),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        var stack = new StackPanel
        {
            Spacing = 3,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        foreach (var ctrl in controls)
        {
            var item = new Border
            {
                Background = Brush.Parse("#F5F5F5"),
                BorderBrush = Brush.Parse("#CCCCCC"),
                BorderThickness = new Avalonia.Thickness(1),
                Padding = new Avalonia.Thickness(4, 2),
                Cursor = new Cursor(StandardCursorType.Hand),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new TextBlock
                {
                    Text = ctrl,
                    FontSize = 10,
                    FontFamily = new FontFamily("Segoe UI")
                },
                Tag = ctrl
            };
            
            item.PointerPressed += (s, e) =>
            {
                var controlType = (s as Border)?.Tag as string;
                _draggedControl = CreateWireframeControl(controlType);
                UpdateStatus($"Click canvas to place {controlType}", false);
                e.Handled = true;
            };
            
            item.PointerEntered += (s, e) =>
            {
                ((Border)s!).Background = Brush.Parse("#E0E0E0");
            };
            
            item.PointerExited += (s, e) =>
            {
                ((Border)s!).Background = Brush.Parse("#F5F5F5");
            };
            
            stack.Children.Add(item);
        }
        
        expander.Content = stack;
        parent.Children.Add(expander);
    }
    
    private Control CreateWireframeControl(string? controlType)
    {
        var label = new TextBlock
        {
            Text = controlType ?? "Control",
            FontSize = 10,
            FontFamily = new FontFamily("Segoe UI"),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray
        };
        
        var (width, height) = controlType switch
        {
            "Button" => (80.0, 30.0),
            "TextBox" => (120.0, 30.0),
            "TextBlock" => (100.0, 20.0),
            "Label" => (80.0, 25.0),
            "CheckBox" => (100.0, 25.0),
            "RadioButton" => (100.0, 25.0),
            "ComboBox" => (120.0, 30.0),
            "ListBox" => (120.0, 100.0),
            "Slider" => (120.0, 30.0),
            "Panel" or "StackPanel" or "Grid" or "DockPanel" or "WrapPanel" or "Canvas" => (100.0, 100.0),
            "Border" => (100.0, 100.0),
            "ScrollViewer" => (120.0, 100.0),
            "Expander" => (120.0, 40.0),
            "Image" => (100.0, 100.0),
            "ProgressBar" => (120.0, 20.0),
            "Separator" => (120.0, 2.0),
            "Rectangle" => (80.0, 60.0),
            "Ellipse" => (60.0, 60.0),
            "Line" => (80.0, 2.0),
            "TabControl" => (150.0, 120.0),
            "TabItem" => (80.0, 30.0),
            "GroupBox" => (120.0, 100.0),
            "Viewbox" => (100.0, 100.0),
            "TreeView" => (120.0, 150.0),
            "Calendar" => (200.0, 180.0),
            "DatePicker" => (150.0, 30.0),
            "TimePicker" => (150.0, 30.0),
            "Menu" => (150.0, 30.0),
            "MenuItem" => (100.0, 25.0),
            _ => (100.0, 30.0)
        };
        
        return new Border
        {
            Width = width,
            Height = height,
            BorderBrush = Brush.Parse("#AAAAAA"),
            BorderThickness = new Avalonia.Thickness(1),
            Background = Brush.Parse("#F9F9F9"),
            Child = label,
            Tag = controlType
        };
    }
    
    private Control CreatePropertiesContent()
    {
        _propertiesStack = new StackPanel
        {
            Spacing = 3,
            Margin = new Avalonia.Thickness(3)
        };
        
        _propertiesStack.Children.Add(new TextBlock
        {
            Text = "No control selected",
            FontStyle = FontStyle.Italic,
            Foreground = Brushes.Gray,
            FontSize = 10,
            FontFamily = new FontFamily("Segoe UI")
        });
        
        return new ScrollViewer
        {
            Content = _propertiesStack,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }
    
    public void ShowPropertiesFor(Control control)
    {
        _selectedControl = control;
        if (_propertiesStack == null) return;
        
        _propertiesStack.Children.Clear();
        
        var controlType = control.Tag as string ?? control.GetType().Name;
        
        _propertiesStack.Children.Add(new TextBlock
        {
            Text = controlType,
            FontSize = 11.7,
            FontWeight = FontWeight.SemiBold,
            Margin = new Avalonia.Thickness(0, 0, 0, 6),
            Padding = new Avalonia.Thickness(1.5),
            Foreground = Brush.Parse("#2E7D32"),
            FontFamily = new FontFamily("Segoe UI")
        });
        
        var properties = control.GetType().GetProperties()
            .Where(p => p.CanRead)
            .GroupBy(p => GetPropertyCategory(p.Name))
            .OrderBy(g => g.Key);
        
        foreach (var group in properties)
        {
            var expander = new Expander
            {
                Header = new TextBlock
                {
                    Text = group.Key,
                    FontSize = 10.8,
                    FontWeight = FontWeight.SemiBold,
                    FontFamily = new FontFamily("Segoe UI"),
                    Padding = new Avalonia.Thickness(1.5)
                },
                IsExpanded = group.Key == "Common",
                Margin = new Avalonia.Thickness(0, 0, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            var propsStack = new StackPanel
            {
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            foreach (var prop in group.OrderBy(p => p.Name))
            {
                try
                {
                    var value = prop.GetValue(control);
                    AddPropertyEditor(propsStack, prop.Name, value, prop.PropertyType, control, prop);
                }
                catch { }
            }
            
            expander.Content = propsStack;
            _propertiesStack.Children.Add(expander);
        }
    }
    
    private string GetPropertyCategory(string propName)
    {
        if (new[] { "Name", "Width", "Height", "IsVisible", "IsEnabled", "Opacity" }.Contains(propName))
            return "Common";
        if (propName.Contains("Margin") || propName.Contains("Padding") || propName.Contains("Alignment"))
            return "Layout";
        if (propName.Contains("Color") || propName.Contains("Brush") || propName.Contains("Background") || propName.Contains("Foreground") || propName.Contains("Border"))
            return "Appearance";
        if (propName.Contains("Font") || propName.Contains("Text"))
            return "Text";
        return "Other";
    }
    
    private void AddPropertyEditor(StackPanel parent, string name, object? value, Type type, Control control, System.Reflection.PropertyInfo prop)
    {
        var propPanel = new StackPanel
        {
            Spacing = 2,
            Margin = new Avalonia.Thickness(0, 0, 0, 4),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        propPanel.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 9,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.DarkGreen,
            FontFamily = new FontFamily("Consolas, Courier New")
        });
        
        if (prop.CanWrite && (type == typeof(string) || type == typeof(double) || type == typeof(int) || type == typeof(bool)))
        {
            if (type == typeof(bool))
            {
                var checkbox = new CheckBox
                {
                    IsChecked = (bool?)value,
                    FontFamily = new FontFamily("Segoe UI")
                };
                checkbox.IsCheckedChanged += (s, e) =>
                {
                    try
                    {
                        prop.SetValue(control, checkbox.IsChecked);
                        if (control == _selectedControl)
                            UpdateSelectionBorder();
                    }
                    catch { }
                };
                propPanel.Children.Add(checkbox);
            }
            else
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? "",
                    FontSize = 9,
                    Height = 22,
                    FontFamily = new FontFamily("Consolas, Courier New"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                textBox.LostFocus += (s, e) =>
                {
                    try
                    {
                        if (type == typeof(double))
                            prop.SetValue(control, double.Parse(textBox.Text));
                        else if (type == typeof(int))
                            prop.SetValue(control, int.Parse(textBox.Text));
                        else
                            prop.SetValue(control, textBox.Text);
                        
                        if (control == _selectedControl)
                            UpdateSelectionBorder();
                    }
                    catch { }
                };
                propPanel.Children.Add(textBox);
            }
        }
        else
        {
            var valueStr = value?.ToString() ?? "null";
            if (valueStr.Length > 35)
                valueStr = valueStr.Substring(0, 32) + "...";
            
            propPanel.Children.Add(new TextBlock
            {
                Text = valueStr,
                FontSize = 8,
                Foreground = Brushes.Gray,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontFamily = new FontFamily("Segoe UI")
            });
        }
        
        parent.Children.Add(propPanel);
    }
    
    private Control CreateCanvasPanel()
    {
        // SIMPLIFIED - just one canvas, no layers!
        _designCanvas.Width = 800;
        _designCanvas.Height = 600;
        _designCanvas.Background = Brushes.White;
        
        // Draw grid directly on the canvas background
        DrawGrid(_designCanvas);
        
        var outerBorder = new Border
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Avalonia.Thickness(2),
            Background = Brush.Parse("#F5F5F5"),
            Padding = new Avalonia.Thickness(3),
            Child = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Canvas: 800×600",
                        FontSize = 10.8,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = Brushes.Gray,
                        Margin = new Avalonia.Thickness(3, 0, 0, 3),
                        [DockPanel.DockProperty] = Dock.Top
                    },
                    new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                        Padding = new Avalonia.Thickness(12),
                        Content = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Avalonia.Thickness(1),
                            Child = _designCanvas
                        }
                    }
                }
            }
        };
        
        return outerBorder;
    }
    
    private Border CreateStatusBar(string dbPath)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
        };
        
        var statusStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Avalonia.Thickness(10, 0)
        };
        
        statusStack.Children.Add(_statusDot);
        statusStack.Children.Add(_statusText);
        Grid.SetColumn(statusStack, 0);
        grid.Children.Add(statusStack);
        
        grid.Children.Add(new TextBlock
        {
            Text = $"Database: {System.IO.Path.GetFileName(dbPath)}",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 10,
            FontFamily = new FontFamily("Segoe UI")
        });
        Grid.SetColumn(grid.Children[^1], 1);
        
        var rightStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Margin = new Avalonia.Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        
        rightStack.Children.Add(_windowText);
        rightStack.Children.Add(_mouseText);
        
        Grid.SetColumn(rightStack, 2);
        grid.Children.Add(rightStack);
        
        return new Border
        {
            Background = Brush.Parse("#2E7D32"),
            Height = 30,
            Child = grid
        };
    }
    
    private void DrawGrid(Canvas canvas)
    {
        const int gridSize = 10;
        var gridBrush = new SolidColorBrush(Color.Parse("#E8E8E8"));
        
        for (int x = 0; x <= 800; x += gridSize)
        {
            canvas.Children.Add(new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, 600),
                Stroke = gridBrush,
                StrokeThickness = x % 50 == 0 ? 1 : 0.5
            });
        }
        
        for (int y = 0; y <= 600; y += gridSize)
        {
            canvas.Children.Add(new Line
            {
                StartPoint = new Point(0, y),
                EndPoint = new Point(800, y),
                Stroke = gridBrush,
                StrokeThickness = y % 50 == 0 ? 1 : 0.5
            });
        }
    }
    
    public void UpdateStatus(string status, bool connected)
    {
        _statusText.Text = status;
        _statusDot.Fill = connected ? Brushes.Blue : Brushes.Red;
    }
    
    public void UpdateWindowInfo(double width, double height)
    {
        _windowText.Text = $"Window: {width:F0}×{height:F0}";
    }
}
