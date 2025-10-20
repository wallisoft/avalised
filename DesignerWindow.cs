using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ConfigUI.Designer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigUI
{
    public class DesignerWindow : Window
    {
        private Canvas? _designCanvas;
        private TextBox? _yamlEditor;
        private TextBlock? _selectedControlLabel;
        private TextBlock? _propType;
        
        private TextBox? _propName;
        private TextBox? _propX;
        private TextBox? _propY;
        private TextBox? _propWidth;
        private TextBox? _propHeight;
        private TextBox? _propCaption;
        private TextBox? _propText;
        private TextBox? _propBackgroundColor;
        private TextBox? _propForegroundColor;
        private TextBox? _propFontFamily;
        private TextBox? _propFontSize;
        private CheckBox? _propFontBold;
        private CheckBox? _propVisible;
        private CheckBox? _propEnabled;
        private ComboBox? _propAlignment;
        
        private Panel? _toolboxPanel;
        private Panel? _propertiesPanel;
        private Panel? _editorPanel;
        
        private Panel? _panelIdentity;
        private Panel? _panelPosition;
        private Panel? _panelAppearance;
        private Panel? _panelFont;
        private Button? _groupIdentity;
        private Button? _groupPosition;
        private Button? _groupAppearance;
        private Button? _groupFont;
        
        private List<DesignerControl> _designerControls = new List<DesignerControl>();
        private DesignerControl? _selectedControl = null;
        private Dictionary<string, int> _controlCounters = new Dictionary<string, int>();
        
        private DesignerControl? _draggedControl = null;
        private Point _dragStartPoint;
        private Point _dragStartControlPosition;
        
        private string? _draggingControlType = null;
        private Border? _ghostControl = null;
        
        private bool _updatingProperties = false;
        
        private ScriptDatabase? _scriptDatabase;
        private DesignerDatabase? _designerDatabase;
        private DesignerImportExport? _importExport;

        public DesignerWindow()
        {
            Width = 1400;
            Height = 900;
            Title = "Visualised Markup - Form Designer v1.0";
            Background = new SolidColorBrush(Color.Parse("#f0f0f0"));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            _scriptDatabase = new ScriptDatabase();
            _designerDatabase = new DesignerDatabase(_scriptDatabase, _designerControls);
            _importExport = new DesignerImportExport(_designerControls, _scriptDatabase, this);
            
	    ImportDesignerYamlToDatabase();

            this.Loaded += (s, e) => LoadDesignerUI();
        }

	// Add this method to DesignerWindow.cs

private void ImportDesignerYamlToDatabase()
{
    if (_designerDatabase == null) return;

    Console.WriteLine("🔄 Importing visual-designer.yaml to database...");
    _designerDatabase.ImportFromYaml();
    Console.WriteLine("✅ Designer YAML imported to SQLite!");
}

        private void LoadDesignerUI()
        {
            try
            {
                var mainCanvas = new Canvas
                {
                    Width = this.Width,
                    Height = this.Height,
                    Background = new SolidColorBrush(Color.Parse("#f0f0f0"))
                };
                
                this.Content = mainCanvas;
                
                var yamlPath = DesignerHelpers.FindYamlFile("visual-designer.yaml");
                if (yamlPath == null)
                {
                    Console.WriteLine("❌ visual-designer.yaml not found!");
                    return;
                }

                var mainWindow = new MainWindow(yamlPath);
                mainWindow.Show();
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var sourceCanvas = mainWindow.FindControl<Canvas>("MainCanvas");
                    
                    if (sourceCanvas != null)
                    {
                        var childrenToMove = sourceCanvas.Children.ToList();
                        sourceCanvas.Children.Clear();
                        
                        foreach (var child in childrenToMove)
                        {
                            mainCanvas.Children.Add(child);
                        }
                        
                        mainWindow.Close();
                        
                        InitializeUIReferences(mainCanvas);
                        HookupToolboxButtons(mainCanvas);
                        HookupMenuItems(mainCanvas);
                        HookupPropertyGroups(mainCanvas);
                        SetupPropertyEditors();
                        
                        _designCanvas = DesignerHelpers.FindCanvasByName(mainCanvas, "DesignCanvas");
                        
                        if (_designCanvas != null)
                        {
                            Console.WriteLine($"✅ Found DesignCanvas: {_designCanvas.Width}x{_designCanvas.Height}");
                            SetupCanvasEvents(_designCanvas);
                        }
                        else
                        {
                            Console.WriteLine("❌ DesignCanvas not found!");
                        }
                        
                        Console.WriteLine("✅ Visual Designer UI loaded successfully!");
                    }
                }, Avalonia.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load designer UI: {ex.Message}");
            }
        }

        private void InitializeUIReferences(Canvas mainCanvas)
        {
            _toolboxPanel = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "ToolboxPanel");
            _propertiesPanel = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "PropertiesPanel");
            _editorPanel = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "EditorPanel");
            
            _yamlEditor = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "YamlEditor");
            _selectedControlLabel = DesignerHelpers.FindControlByName<TextBlock>(mainCanvas, "SelectedControlLabel");
            _propType = DesignerHelpers.FindControlByName<TextBlock>(mainCanvas, "PropType");
            
            _propName = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropName");
            _propX = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropX");
            _propY = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropY");
            _propWidth = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropWidth");
            _propHeight = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropHeight");
            _propCaption = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropCaption");
            _propText = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropText");
            _propBackgroundColor = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropBackgroundColor");
            _propForegroundColor = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropForegroundColor");
            _propFontFamily = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropFontFamily");
            _propFontSize = DesignerHelpers.FindControlByName<TextBox>(mainCanvas, "PropFontSize");
            _propFontBold = DesignerHelpers.FindControlByName<CheckBox>(mainCanvas, "PropFontBold");
            _propVisible = DesignerHelpers.FindControlByName<CheckBox>(mainCanvas, "PropVisible");
            _propEnabled = DesignerHelpers.FindControlByName<CheckBox>(mainCanvas, "PropEnabled");
            _propAlignment = DesignerHelpers.FindControlByName<ComboBox>(mainCanvas, "PropAlignment");
            
            _panelIdentity = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "PanelIdentity");
            _panelPosition = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "PanelPosition");
            _panelAppearance = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "PanelAppearance");
            _panelFont = DesignerHelpers.FindControlByName<Panel>(mainCanvas, "PanelFont");
            
            _groupIdentity = DesignerHelpers.FindControlByName<Button>(mainCanvas, "GroupIdentity");
            _groupPosition = DesignerHelpers.FindControlByName<Button>(mainCanvas, "GroupPosition");
            _groupAppearance = DesignerHelpers.FindControlByName<Button>(mainCanvas, "GroupAppearance");
            _groupFont = DesignerHelpers.FindControlByName<Button>(mainCanvas, "GroupFont");
        }

        private void HookupPropertyGroups(Canvas mainCanvas)
        {
            HookupButton(mainCanvas, "GroupIdentity", () => TogglePropertyGroup("Identity"));
            HookupButton(mainCanvas, "GroupPosition", () => TogglePropertyGroup("Position"));
            HookupButton(mainCanvas, "GroupAppearance", () => TogglePropertyGroup("Appearance"));
            HookupButton(mainCanvas, "GroupFont", () => TogglePropertyGroup("Font"));
        }

        private void TogglePropertyGroup(string groupName)
        {
            Panel? panel = groupName switch
            {
                "Identity" => _panelIdentity,
                "Position" => _panelPosition,
                "Appearance" => _panelAppearance,
                "Font" => _panelFont,
                _ => null
            };
            
            Button? button = groupName switch
            {
                "Identity" => _groupIdentity,
                "Position" => _groupPosition,
                "Appearance" => _groupAppearance,
                "Font" => _groupFont,
                _ => null
            };
            
            if (panel != null && button != null)
            {
                panel.IsVisible = !panel.IsVisible;
                var caption = button.Content?.ToString() ?? "";
                if (panel.IsVisible)
                {
                    button.Content = caption.Replace("▶", "▼");
                }
                else
                {
                    button.Content = caption.Replace("▼", "▶");
                }
            }
        }

        private void SetupPropertyEditors()
        {
            if (_propName != null)
                _propName.LostFocus += (s, e) => UpdateControlFromProperty("name", _propName.Text);
            if (_propX != null)
                _propX.LostFocus += (s, e) => UpdateControlFromProperty("x", _propX.Text);
            if (_propY != null)
                _propY.LostFocus += (s, e) => UpdateControlFromProperty("y", _propY.Text);
            if (_propWidth != null)
                _propWidth.LostFocus += (s, e) => UpdateControlFromProperty("width", _propWidth.Text);
            if (_propHeight != null)
                _propHeight.LostFocus += (s, e) => UpdateControlFromProperty("height", _propHeight.Text);
            if (_propCaption != null)
                _propCaption.LostFocus += (s, e) => UpdateControlFromProperty("caption", _propCaption.Text);
            if (_propText != null)
                _propText.LostFocus += (s, e) => UpdateControlFromProperty("text", _propText.Text);
        }

        private void UpdateControlFromProperty(string property, string? value)
        {
            if (_updatingProperties || _selectedControl == null || _designCanvas == null) return;
            
            try
            {
                switch (property)
                {
                    case "name":
                        _selectedControl.Name = value ?? "";
                        RefreshVisualControl(_selectedControl);
                        break;
                    case "x":
                        if (double.TryParse(value, out double x))
                        {
                            _selectedControl.X = x;
                            var border = _designCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Tag == _selectedControl);
                            if (border != null) Canvas.SetLeft(border, x);
                        }
                        break;
                    case "y":
                        if (double.TryParse(value, out double y))
                        {
                            _selectedControl.Y = y;
                            var border = _designCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Tag == _selectedControl);
                            if (border != null) Canvas.SetTop(border, y);
                        }
                        break;
                    case "width":
                        if (double.TryParse(value, out double width))
                        {
                            _selectedControl.Width = width;
                            RefreshVisualControl(_selectedControl);
                        }
                        break;
                    case "height":
                        if (double.TryParse(value, out double height))
                        {
                            _selectedControl.Height = height;
                            RefreshVisualControl(_selectedControl);
                        }
                        break;
                    case "caption":
                        _selectedControl.Caption = value;
                        RefreshVisualControl(_selectedControl);
                        break;
                    case "text":
                        _selectedControl.Text = value;
                        RefreshVisualControl(_selectedControl);
                        break;
                }
                
                if (_designerDatabase != null && _selectedControl != null)
                {
                    _designerDatabase.SaveControl(_selectedControl);
                }
                
                UpdateYamlEditor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error: {ex.Message}");
            }
        }

        private void RefreshVisualControl(DesignerControl control)
        {
            if (_designCanvas == null) return;
            
            var border = _designCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Tag == control);
            if (border == null) return;
            
            var isSelected = border.BorderBrush is SolidColorBrush brush && brush.Color == Colors.Blue;
            
            _designCanvas.Children.Remove(border);
            
            var newBorder = CreateVisualControl(control);
            if (isSelected)
            {
                newBorder.BorderBrush = new SolidColorBrush(Colors.Blue);
                newBorder.BorderThickness = new Thickness(2);
            }
            
            _designCanvas.Children.Add(newBorder);
        }

        private void HookupToolboxButtons(Canvas mainCanvas)
        {
            Console.WriteLine("🔧 Hooking up toolbox buttons...");
            HookupButton(mainCanvas, "PreviewButton", () => LaunchPreview());
            SetupToolboxDrag(mainCanvas);
            Console.WriteLine("✅ Toolbox buttons hooked up!");
        }

        private void SetupToolboxDrag(Canvas mainCanvas)
        {
            Console.WriteLine("🔧 Setting up toolbox drag events...");
            
            var toolboxButtons = new[] {
                ("AddLabel", "label"), ("AddButton", "button"), ("AddTextBox", "textbox"),
                ("AddCheckBox", "checkbox"), ("AddRadioButton", "radiobutton"),
                ("AddComboBox", "combobox"), ("AddListBox", "listbox"),
                ("AddPanel", "panel"), ("AddTabControl", "tabcontrol"),
                ("AddMenuBar", "menubar"), ("AddToolBar", "toolbar"),
                ("AddProgressBar", "progressbar"), ("AddTimer", "timer"),
                ("AddData", "data"), ("AddGrid", "grid"), 
                ("AddHScrollBar", "hscrollbar"), ("AddVScrollBar", "vscrollbar")
            };

            foreach (var (buttonName, controlType) in toolboxButtons)
            {
                var button = DesignerHelpers.FindControlByName<Button>(mainCanvas, buttonName);
                if (button != null)
                {
                    button.AddHandler(PointerPressedEvent, (EventHandler<PointerPressedEventArgs>)((s, e) =>
                    {
                        if (e.GetCurrentPoint(button).Properties.IsLeftButtonPressed)
                        {
                            _draggingControlType = controlType;
                            CreateGhostControl(controlType);
                            
                            if (_ghostControl != null && _designCanvas != null)
                            {
                                _ghostControl.Width = button.Bounds.Width;
                                _ghostControl.Height = button.Bounds.Height;
                                
                                var buttonPos = button.TranslatePoint(new Point(0, 0), _designCanvas);
                                if (buttonPos.HasValue)
                                {
                                    Canvas.SetLeft(_ghostControl, buttonPos.Value.X);
                                    Canvas.SetTop(_ghostControl, buttonPos.Value.Y);
                                }
                            }
                            
                            Console.WriteLine($"👻 Started dragging {controlType}");
                        }
                    }), RoutingStrategies.Tunnel);
                }
            }
            
            Console.WriteLine("✅ Toolbox drag setup complete!");
        }

        private void HookupMenuItems(Canvas mainCanvas)
        {
            Console.WriteLine("🔧 Hooking up menu items...");
            var menu = DesignerHelpers.FindControlByName<Menu>(mainCanvas, "MainMenu");
            if (menu != null)
            {
                foreach (var topItem in menu.Items.Cast<MenuItem>())
                {
                    if (topItem.Header?.ToString() == "File")
                    {
                        foreach (var fileItem in topItem.Items.Cast<MenuItem>())
                        {
                            var header = fileItem.Header?.ToString();
                            if (header == "Reload Form")
                                fileItem.Click += (s, e) => LoadDesignerUI();
                            else if (header == "Save Form...")
                                fileItem.Click += async (s, e) => await _importExport?.ShowExportDialog()!;
                            else if (header == "Open Form...")
                                fileItem.Click += async (s, e) => await _importExport?.ShowImportDialog()!;
                            else if (header == "Save to Database")
                                fileItem.Click += (s, e) => _designerDatabase?.SaveAllToDatabase();
                            else if (header == "Load from Database")
                                fileItem.Click += (s, e) => LoadAllFromDatabase();
                            else if (header == "Export YAML from Database")
                                fileItem.Click += async (s, e) => { LoadAllFromDatabase(); await _importExport?.ShowExportDialog()!; };
                            else if (header == "Reset Database")
                                fileItem.Click += (s, e) => ResetDatabase();
                        }
                    }
                    else if (topItem.Header?.ToString() == "View")
                    {
                        foreach (var viewItem in topItem.Items.Cast<MenuItem>())
                        {
                            var header = viewItem.Header?.ToString();
                            if (header == "Toolbox")
                                viewItem.Click += (s, e) => { if (_toolboxPanel != null) _toolboxPanel.IsVisible = !_toolboxPanel.IsVisible; };
                            else if (header == "Properties")
                                viewItem.Click += (s, e) => { if (_propertiesPanel != null) _propertiesPanel.IsVisible = !_propertiesPanel.IsVisible; };
                            else if (header == "YAML Editor")
                                viewItem.Click += (s, e) => { if (_editorPanel != null) _editorPanel.IsVisible = !_editorPanel.IsVisible; };
                        }
                    }
                    else if (topItem.Header?.ToString() == "Help")
                    {
                        foreach (var helpItem in topItem.Items.Cast<MenuItem>())
                        {
                            if (helpItem.Header?.ToString() == "About")
                            {
                                helpItem.Click += (s, e) => ShowAbout();
                            }
                        }
                    }
                }
            }
            Console.WriteLine("✅ Menu items hooked up!");
        }

        private void ShowAbout()
        {
            var aboutPath = DesignerHelpers.FindYamlFile("about-form.yaml");
            if (aboutPath != null)
            {
                var aboutWindow = new MainWindow(aboutPath);
                aboutWindow.Show();
            }
        }

        private void HookupButton(Canvas canvas, string buttonName, Action action)
        {
            var button = DesignerHelpers.FindControlByName<Button>(canvas, buttonName);
            if (button != null)
            {
                button.Click += (s, e) => action();
            }
        }

        private void SetupCanvasEvents(Canvas canvas)
        {
            this.PointerMoved += (s, e) =>
            {
                if (_draggingControlType != null && _ghostControl != null && _designCanvas != null)
                {
                    var pos = e.GetPosition(_designCanvas);
                    Canvas.SetLeft(_ghostControl, pos.X - (_ghostControl.Width / 2));
                    Canvas.SetTop(_ghostControl, pos.Y - (_ghostControl.Height / 2));
                }
                
                if (_draggedControl != null && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _designCanvas != null)
                {
                    var currentPos = e.GetPosition(_designCanvas);
                    var deltaX = currentPos.X - _dragStartPoint.X;
                    var deltaY = currentPos.Y - _dragStartPoint.Y;
                    _draggedControl.X = _dragStartControlPosition.X + deltaX;
                    _draggedControl.Y = _dragStartControlPosition.Y + deltaY;
                    
                    var border = _designCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Tag == _draggedControl);
                    if (border != null)
                    {
                        Canvas.SetLeft(border, _draggedControl.X);
                        Canvas.SetTop(border, _draggedControl.Y);
                    }
                    
                    _updatingProperties = true;
                    if (_propX != null) _propX.Text = _draggedControl.X.ToString("F0");
                    if (_propY != null) _propY.Text = _draggedControl.Y.ToString("F0");
                    _updatingProperties = false;
                }
            };
            
            this.PointerReleased += (s, e) =>
            {
                if (_draggingControlType != null && _ghostControl != null)
                {
                    Console.WriteLine($"🔽 Releasing {_draggingControlType}");
                    
                    if (_designCanvas != null)
                    {
                        var releasePos = e.GetPosition(_designCanvas);
                        
                        if (releasePos.X >= 0 && releasePos.Y >= 0 && 
                            releasePos.X <= _designCanvas.Width && releasePos.Y <= _designCanvas.Height)
                        {
                            Console.WriteLine($"🎯 Creating at ({releasePos.X}, {releasePos.Y})");
                            RemoveGhostControl();
                            AddControlAtPosition(_draggingControlType, releasePos.X, releasePos.Y);
                        }
                        else
                        {
                            Console.WriteLine($"❌ Outside canvas");
                            RemoveGhostControl();
                        }
                    }
                    
                    _draggingControlType = null;
                    Console.WriteLine($"✅ Drag complete");
                }
                
                if (_draggedControl != null)
                {
                    if (_designerDatabase != null)
                    {
                        _designerDatabase.SaveControl(_draggedControl);
                    }
                    UpdateYamlEditor();
                    _draggedControl = null;
                }
            };
            
            canvas.PointerPressed += (s, e) =>
            {
                if (_draggingControlType != null) return;
                
                var clickedElement = e.Source as Control;
                var clickedBorder = clickedElement as Border ?? (clickedElement?.Parent as Border);
                if (clickedBorder != null && clickedBorder.Tag is DesignerControl control)
                {
                    SelectControl(control);
                    _draggedControl = control;
                    _dragStartPoint = e.GetPosition(canvas);
                    _dragStartControlPosition = new Point(control.X, control.Y);
                }
            };
        }

        private void CreateGhostControl(string controlType)
        {
            if (_designCanvas == null) return;
            
            RemoveGhostControl();
            
            var width = controlType switch
            {
                "textbox" or "combobox" or "listbox" => 200.0,
                "panel" => 300.0,
                "progressbar" => 250.0,
                _ => 150.0
            };
            
            var height = controlType switch
            {
                "listbox" => 100.0,
                "panel" => 200.0,
                "progressbar" => 25.0,
                _ => 30.0
            };
            
            _ghostControl = new Border
            {
                Width = width,
                Height = height,
                Background = new SolidColorBrush(Color.Parse("#80E3F2FD")),
                BorderBrush = new SolidColorBrush(Color.Parse("#4a90e2")),
                BorderThickness = new Thickness(2),
                Child = new TextBlock
                {
                    Text = controlType.ToUpper(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse("#1976D2")),
                    FontSize = 11,
                    FontWeight = FontWeight.Bold,
                    IsHitTestVisible = false
                },
                IsHitTestVisible = false,
                Opacity = 0.8
            };
            
            _designCanvas.Children.Add(_ghostControl);
            Console.WriteLine($"👻 Ghost created for {controlType}");
        }

        private void RemoveGhostControl()
        {
            if (_ghostControl != null && _designCanvas != null)
            {
                _designCanvas.Children.Remove(_ghostControl);
                _ghostControl = null;
                Console.WriteLine($"🧹 Ghost removed");
            }
        }

        private void AddControlAtPosition(string controlType, double x, double y)
        {
            if (_designCanvas == null) return;
            
            var controlWidth = controlType switch
            {
                "textbox" or "combobox" or "listbox" => 200.0,
                "panel" => 300.0,
                "progressbar" => 250.0,
                _ => 150.0
            };
            
            var controlHeight = controlType switch
            {
                "listbox" => 100.0,
                "panel" => 200.0,
                "progressbar" => 25.0,
                _ => 30.0
            };
            
            if (!_controlCounters.ContainsKey(controlType))
                _controlCounters[controlType] = 1;
            var controlName = $"{controlType}_{_controlCounters[controlType]++}";
            
            var designerControl = new DesignerControl
            {
                Type = controlType,
                Name = controlName,
                X = x,
                Y = y,
                Width = controlWidth,
                Height = controlHeight,
                FontSize = 12,
                Enabled = true,
                Visible = true
            };
            
            switch (controlType)
            {
                case "label": designerControl.Caption = "Label"; break;
                case "button": designerControl.Caption = "Button"; break;
                case "textbox": designerControl.Text = ""; break;
                case "checkbox": designerControl.Caption = "CheckBox"; break;
                case "radiobutton": designerControl.Caption = "RadioButton"; break;
                case "combobox": designerControl.Caption = "ComboBox"; break;
                case "listbox": designerControl.Caption = "ListBox"; break;
                case "panel": designerControl.BackgroundColor = "#f0f0f0"; break;
                case "progressbar": designerControl.Caption = "ProgressBar"; break;
                case "menubar": designerControl.Caption = "MenuBar"; break;
                case "toolbar": designerControl.Caption = "ToolBar"; break;
                case "tabcontrol": designerControl.Caption = "TabControl"; break;
                case "timer": designerControl.Interval = 1000; break;
            }
            
            if (_designerDatabase != null)
            {
                _designerDatabase.SaveControl(designerControl);
            }
            
            var visual = CreateVisualControl(designerControl);
            _designCanvas.Children.Add(visual);
            _designerControls.Add(designerControl);
            UpdateYamlEditor();
            SelectControl(designerControl);
            
            Console.WriteLine($"✅ Created {controlName} at ({x}, {y})");
        }

        private Border CreateVisualControl(DesignerControl control)
        {
            var bgColor = !string.IsNullOrEmpty(control.BackgroundColor) 
                ? Color.Parse(control.BackgroundColor) 
                : Colors.White;
            
            var mainBorder = new Border
            {
                Width = control.Width, 
                Height = control.Height,
                Background = new SolidColorBrush(bgColor),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1), 
                Tag = control
            };
            Canvas.SetLeft(mainBorder, control.X);
            Canvas.SetTop(mainBorder, control.Y);
            
            var fgColor = !string.IsNullOrEmpty(control.ForegroundColor)
                ? Color.Parse(control.ForegroundColor)
                : Colors.Black;
            
            var fontSize = control.FontSize ?? 12;
            var fontWeight = control.FontBold ? FontWeight.Bold : FontWeight.Normal;
            
            Control innerControl = control.Type switch
            {
                "label" => new TextBlock { 
                    Text = control.Name,
                    VerticalAlignment = VerticalAlignment.Center, 
                    Margin = new Thickness(5), 
                    IsHitTestVisible = false,
                    Foreground = new SolidColorBrush(fgColor),
                    FontSize = fontSize,
                    FontWeight = fontWeight
                },
                "button" => new TextBlock { 
                    Text = control.Name,
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    IsHitTestVisible = false,
                    Foreground = new SolidColorBrush(fgColor),
                    FontSize = fontSize,
                    FontWeight = fontWeight
                },
                "textbox" => new TextBlock { 
                    Text = control.Name,
                    VerticalAlignment = VerticalAlignment.Center, 
                    Margin = new Thickness(5), 
                    Foreground = new SolidColorBrush(Colors.Gray), 
                    IsHitTestVisible = false,
                    FontSize = fontSize
                },
                "checkbox" => new TextBlock { 
                    Text = "☐ " + control.Name,
                    VerticalAlignment = VerticalAlignment.Center, 
                    Margin = new Thickness(5), 
                    IsHitTestVisible = false,
                    FontSize = fontSize
                },
                _ => new TextBlock { 
                    Text = control.Name,
                    VerticalAlignment = VerticalAlignment.Center, 
                    Margin = new Thickness(5), 
                    IsHitTestVisible = false 
                }
            };
            
            mainBorder.Child = innerControl;
            AttachContextMenu(control, mainBorder);
            
            return mainBorder;
        }

        private void AttachContextMenu(DesignerControl control, Border border)
        {
            var contextMenu = new ContextMenu();
            
            var scriptsItem = new MenuItem { Header = "📝 Edit Scripts..." };
            scriptsItem.Click += (s, e) => OpenScriptEditor(control);
            contextMenu.Items.Add(scriptsItem);
            
            contextMenu.Items.Add(new Separator());
            
            var deleteItem = new MenuItem { Header = "🗑️ Delete" };
            deleteItem.Click += (s, e) => DeleteControl(control);
            contextMenu.Items.Add(deleteItem);
            
            var duplicateItem = new MenuItem { Header = "📋 Duplicate" };
            duplicateItem.Click += (s, e) => DuplicateControl(control);
            contextMenu.Items.Add(duplicateItem);
            
            contextMenu.Items.Add(new Separator());
            
            var frontItem = new MenuItem { Header = "⬆️ Bring to Front" };
            frontItem.Click += (s, e) => BringToFront(border);
            contextMenu.Items.Add(frontItem);
            
            var backItem = new MenuItem { Header = "⬇️ Send to Back" };
            backItem.Click += (s, e) => SendToBack(border);
            contextMenu.Items.Add(backItem);
            
            border.ContextMenu = contextMenu;
        }

        private void OpenScriptEditor(DesignerControl control)
        {
            if (_scriptDatabase == null)
            {
                Console.WriteLine("❌ Database not initialized!");
                return;
            }
            
            if (!control.DatabaseId.HasValue && _designerDatabase != null)
            {
                _designerDatabase.SaveControl(control);
            }
            
            if (control.DatabaseId.HasValue)
            {
                var scriptEditor = new ScriptEditorWindow(
                    _scriptDatabase,
                    control.DatabaseId.Value,
                    control.Name,
                    control.Type
                );
                
                scriptEditor.Show();
            }
        }

        private void DeleteControl(DesignerControl control)
        {
            if (_designCanvas == null) return;
            
            var border = _designCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Tag == control);
            if (border != null)
            {
                _designCanvas.Children.Remove(border);
                _designerControls.Remove(control);
                
                if (_designerDatabase != null)
                {
                    _designerDatabase.DeleteControl(control);
                }
                
                if (_selectedControl == control)
                {
                    _selectedControl = null;
                    if (_selectedControlLabel != null)
                        _selectedControlLabel.Text = "No control selected";
                }
                
                UpdateYamlEditor();
                Console.WriteLine($"✅ Deleted control: {control.Name}");
            }
        }

        private void DuplicateControl(DesignerControl control)
        {
            AddControlAtPosition(control.Type, control.X + 20, control.Y + 20);
        }

        private void BringToFront(Border border)
        {
            if (_designCanvas == null) return;
            
            _designCanvas.Children.Remove(border);
            _designCanvas.Children.Add(border);
            Console.WriteLine("✅ Brought to front");
        }

        private void SendToBack(Border border)
        {
            if (_designCanvas == null) return;
            
            _designCanvas.Children.Remove(border);
            _designCanvas.Children.Insert(0, border);
            Console.WriteLine("✅ Sent to back");
        }

        private void SelectControl(DesignerControl control)
        {
            _selectedControl = control;
            if (_designCanvas != null)
            {
                foreach (var child in _designCanvas.Children.OfType<Border>())
                {
                    if (child.Tag == control)
                    {
                        child.BorderBrush = new SolidColorBrush(Colors.Blue);
                        child.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        child.BorderBrush = new SolidColorBrush(Colors.Black);
                        child.BorderThickness = new Thickness(1);
                    }
                }
            }
            if (_selectedControlLabel != null)
            {
                _selectedControlLabel.Text = $"{control.Name} ({control.Type})";
            }
            
            _updatingProperties = true;
            if (_propType != null) _propType.Text = control.Type;
            if (_propName != null) _propName.Text = control.Name;
            if (_propX != null) _propX.Text = control.X.ToString("F0");
            if (_propY != null) _propY.Text = control.Y.ToString("F0");
            if (_propWidth != null) _propWidth.Text = control.Width.ToString("F0");
            if (_propHeight != null) _propHeight.Text = control.Height.ToString("F0");
            if (_propCaption != null) _propCaption.Text = control.Caption ?? "";
            if (_propText != null) _propText.Text = control.Text ?? "";
            if (_propBackgroundColor != null) _propBackgroundColor.Text = control.BackgroundColor ?? "";
            if (_propForegroundColor != null) _propForegroundColor.Text = control.ForegroundColor ?? "";
            _updatingProperties = false;
        }

        private void UpdateYamlEditor()
        {
            if (_yamlEditor == null || _importExport == null) return;
            _yamlEditor.Text = _importExport.ExportToYAML();
        }

        private void LaunchPreview()
        {
            if (_designerControls.Count == 0) return;
            if (_importExport == null) return;
            
            var yaml = _importExport.ExportToYAML();
            var tempPath = Path.Combine(Path.GetTempPath(), "preview-form.yaml");
            File.WriteAllText(tempPath, yaml);
            
            var previewWindow = new MainWindow(tempPath);
            previewWindow.Show();
        }

        private void LoadAllFromDatabase()
        {
            if (_designerDatabase == null || _designCanvas == null) return;
            
            var controls = _designerDatabase.LoadAllFromDatabase();
            
            _designerControls.Clear();
            _designCanvas.Children.Clear();
            
            foreach (var control in controls)
            {
                var visual = CreateVisualControl(control);
                _designCanvas.Children.Add(visual);
                _designerControls.Add(control);
            }
            
            UpdateYamlEditor();
        }

        private void ResetDatabase()
        {
            if (_designerDatabase == null || _designCanvas == null) return;
            
            _designerDatabase.ResetDatabase();
            _designerControls.Clear();
            _designCanvas.Children.Clear();
            UpdateYamlEditor();
        }
    }
}
