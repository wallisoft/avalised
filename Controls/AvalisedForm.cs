using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalised.Controls;

/// <summary>
/// 🌳 Avalised™ Form control with special child handling
/// MenuBar auto-docks top, StatusBar bottom, content fills center
/// </summary>
public class AvalisedForm : Window
{
    private DockPanel _rootPanel;
    private Control? _menuBar;
    private Control? _toolBar;
    private Control? _statusBar;
    private Control? _content;

    public AvalisedForm()
    {
        // VB5 default properties
        Width = 800;
        Height = 600;
        CanResize = true;
        
        // Create root DockPanel
        _rootPanel = new DockPanel
        {
            LastChildFill = true
        };
        
        Content = _rootPanel;
    }

    #region VB5-Style Properties

    /// <summary>
    /// BorderStyle: 0=None, 1=FixedSingle, 2=Sizable, 3=FixedDialog, 4=FixedToolWindow, 5=SizableToolWindow
    /// </summary>
    public int BorderStyle
    {
        get => (int)GetValue(BorderStyleProperty);
        set
        {
            SetValue(BorderStyleProperty, value);
            ApplyBorderStyle(value);
        }
    }
    public static readonly StyledProperty<int> BorderStyleProperty =
        AvaloniaProperty.Register<AvalisedForm, int>(nameof(BorderStyle), 2);

    /// <summary>
    /// StartPosition: 0=Manual, 1=CenterOwner, 2=CenterScreen, 3=WindowsDefaultLocation
    /// </summary>
    public int StartPosition
    {
        get => (int)GetValue(StartPositionProperty);
        set => SetValue(StartPositionProperty, value);
    }
    public static readonly StyledProperty<int> StartPositionProperty =
        AvaloniaProperty.Register<AvalisedForm, int>(nameof(StartPosition), 2);

    /// <summary>
    /// Caption (Title)
    /// </summary>
    public string Caption
    {
        get => Title ?? "";
        set => Title = value;
    }

    /// <summary>
    /// BackColor
    /// </summary>
    public IBrush? BackColor
    {
        get => Background;
        set => Background = value;
    }

    /// <summary>
    /// ForeColor
    /// </summary>
    public IBrush? ForeColor
    {
        get => Foreground;
        set => Foreground = value;
    }

    #endregion

    #region Special Child Injection (VB5-style)

    /// <summary>
    /// Set the MenuBar - automatically docks to top
    /// </summary>
    public void SetMenuBar(Control menuBar)
    {
        if (_menuBar != null)
            _rootPanel.Children.Remove(_menuBar);

        _menuBar = menuBar;
        DockPanel.SetDock(_menuBar, Dock.Top);
        _rootPanel.Children.Insert(0, _menuBar);
    }

    /// <summary>
    /// Set the ToolBar - automatically docks below menu
    /// </summary>
    public void SetToolBar(Control toolBar)
    {
        if (_toolBar != null)
            _rootPanel.Children.Remove(_toolBar);

        _toolBar = toolBar;
        DockPanel.SetDock(_toolBar, Dock.Top);
        
        // Insert after menu if it exists
        int insertAt = _menuBar != null ? 1 : 0;
        _rootPanel.Children.Insert(insertAt, _toolBar);
    }

    /// <summary>
    /// Set the StatusBar - automatically docks to bottom
    /// </summary>
    public void SetStatusBar(Control statusBar)
    {
        if (_statusBar != null)
            _rootPanel.Children.Remove(_statusBar);

        _statusBar = statusBar;
        DockPanel.SetDock(_statusBar, Dock.Bottom);
        
        // Always insert before content (last child fills)
        int insertAt = _rootPanel.Children.Count;
        if (_content != null)
            insertAt = _rootPanel.Children.IndexOf(_content);
        
        _rootPanel.Children.Insert(insertAt, _statusBar);
    }

    /// <summary>
    /// Set the main content - fills remaining space
    /// </summary>
    public void SetContent(Control content)
    {
        if (_content != null)
            _rootPanel.Children.Remove(_content);

        _content = content;
        // Content is always last (LastChildFill = true)
        _rootPanel.Children.Add(_content);
    }

    #endregion

    #region Helper Methods

    private void ApplyBorderStyle(int style)
    {
        switch (style)
        {
            case 0: // None
                SystemDecorations = SystemDecorations.None;
                CanResize = false;
                break;
            case 1: // FixedSingle
                SystemDecorations = SystemDecorations.Full;
                CanResize = false;
                break;
            case 2: // Sizable (default)
                SystemDecorations = SystemDecorations.Full;
                CanResize = true;
                break;
            case 3: // FixedDialog
                SystemDecorations = SystemDecorations.Full;
                CanResize = false;
                ShowInTaskbar = false;
                break;
            case 4: // FixedToolWindow
                SystemDecorations = SystemDecorations.BorderOnly;
                CanResize = false;
                ShowInTaskbar = false;
                break;
            case 5: // SizableToolWindow
                SystemDecorations = SystemDecorations.BorderOnly;
                CanResize = true;
                ShowInTaskbar = false;
                break;
        }
    }

    #endregion
}
