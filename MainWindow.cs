using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Avalised;

public class MainWindow : Window
{
    private DesignerLayout? _designerLayout;
    
    public MainWindow()
    {
        Title = "🌳 Avalised™ Designer";
        Width = 1280;
        Height = 720;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        LoadUI();
        
        this.Opened += (s, e) => WireUpEventHandlers();
        this.PropertyChanged += OnWindowPropertyChanged;
    }

    private void LoadUI()
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Avalised",
            "designer.db"
        );

        // Build menu
        var builder = new UITreeBuilder(dbPath);
        var menu = builder.BuildUI();
        
        // Create designer layout
        _designerLayout = new DesignerLayout(dbPath);
        
        // PROPER HIERARCHY: Root DockPanel contains everything
        var rootPanel = new DockPanel { LastChildFill = true };
        
        // Menu at top (properly docked)
        DockPanel.SetDock(menu, Dock.Top);
        rootPanel.Children.Add(menu);
        
        // Designer layout fills remaining space (includes status bar)
        rootPanel.Children.Add(_designerLayout);
        
        Content = rootPanel;
    }

    private void OnWindowPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if ((e.Property == WidthProperty || e.Property == HeightProperty) && _designerLayout != null)
        {
            _designerLayout.UpdateWindowInfo(Width, Height);
        }
    }

    private void WireUpEventHandlers()
    {
        var menuItems = new Dictionary<string, MenuItem>();
        FindMenuItems(this, menuItems);
        
        WireMenuItem(menuItems, "HelpAbout", OnHelpAbout);
        WireMenuItem(menuItems, "HelpSystemInfo", OnHelpSystemInfo);
        WireMenuItem(menuItems, "HelpWiki", OnHelpWiki);
        WireMenuItem(menuItems, "FileExit", OnFileExit);
        WireMenuItem(menuItems, "PreviewToggle", OnPreviewToggle);
        WireMenuItem(menuItems, "PreviewRun", OnPreviewRun);
        WireMenuItem(menuItems, "ViewOptions", OnViewOptions);
        WireMenuItem(menuItems, "ToolsMarkupEditor", OnMarkupEditor);
        WireMenuItem(menuItems, "ToolsImportVB5", OnImportVB5);
        WireMenuItem(menuItems, "ToolsDebugUser1", OnDebugUser1);
        WireMenuItem(menuItems, "ToolsDebugUser2", OnDebugUser2);
        WireMenuItem(menuItems, "ToolsDebugUser3", OnDebugUser3);
    }

    private void FindMenuItems(Control control, Dictionary<string, MenuItem> menuItems)
    {
        if (control is MenuItem menuItem)
        {
            if (!string.IsNullOrEmpty(menuItem.Name))
                menuItems[menuItem.Name] = menuItem;
            
            foreach (var item in menuItem.Items)
            {
                if (item is MenuItem childMenuItem)
                    FindMenuItems(childMenuItem, menuItems);
            }
        }

        foreach (var child in control.GetVisualChildren())
        {
            if (child is Control childControl)
                FindMenuItems(childControl, menuItems);
        }
    }

    private void WireMenuItem(Dictionary<string, MenuItem> menuItems, string name, EventHandler<RoutedEventArgs> handler)
    {
        if (menuItems.TryGetValue(name, out var menuItem))
            menuItem.Click += handler;
    }

    #region Event Handlers

    private async void OnHelpAbout(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.AboutDialog();
        await dialog.ShowDialog(this);
    }

    private async void OnHelpSystemInfo(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.SystemInfoDialog();
        await dialog.ShowDialog(this);
    }

    private void OnHelpWiki(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/wallisoft/avalised/wiki",
                UseShellExecute = true
            });
        }
        catch { }
    }

    private void OnFileExit(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private bool _previewMode = false;
    private void OnPreviewToggle(object? sender, RoutedEventArgs e)
    {
        _previewMode = !_previewMode;
        Title = _previewMode ? "🌳 Avalised™ Designer - Preview Mode" : "🌳 Avalised™ Designer";
        _designerLayout?.UpdateStatus(_previewMode ? "Preview Mode" : "Ready", false);
    }

    private void OnPreviewRun(object? sender, RoutedEventArgs e)
    {
        _designerLayout?.UpdateStatus("Running application...", false);
    }

    private async void OnViewOptions(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.OptionsDialog();
        await dialog.ShowDialog(this);
    }

    private async void OnMarkupEditor(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.MarkupEditorDialog();
        await dialog.ShowDialog(this);
    }

    private void OnImportVB5(object? sender, RoutedEventArgs e)
    {
        _designerLayout?.UpdateStatus("Importing VB5 project...", false);
    }

    private void OnDebugUser1(object? sender, RoutedEventArgs e)
    {
        _designerLayout?.UpdateStatus("Running User Defined 1...", false);
    }

    private void OnDebugUser2(object? sender, RoutedEventArgs e)
    {
        _designerLayout?.UpdateStatus("Running User Defined 2...", false);
    }

    private void OnDebugUser3(object? sender, RoutedEventArgs e)
    {
        _designerLayout?.UpdateStatus("Running User Defined 3...", false);
    }

    #endregion

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        if (e.Key == Key.F8)
            OnPreviewToggle(this, new RoutedEventArgs());
        else if (e.Key == Key.F9)
            OnPreviewRun(this, new RoutedEventArgs());
    }
}
