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
    private ActionExecutor? _actionExecutor;

    public MainWindow()
    {
        Title = "🌳 Avalised™ Designer";
        Width = 1280;
        Height = 720;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;



        LoadUI();

        this.Opened += (s, e) => {
            WireUpEventHandlers();
            DumpVisualTree(this);
        };
        this.PropertyChanged += OnWindowPropertyChanged;
    }

    private void LoadUI()
    {
        // Try repo directory first (for fresh clones!)
        var repoDbPath = "designer.db";
        
        string dbPath;
        if (System.IO.File.Exists(repoDbPath))
        {
            // Use the one in the repo directory
            dbPath = repoDbPath;
            Console.WriteLine($"✅ Using designer.db from repo: {System.IO.Path.GetFullPath(dbPath)}");
        }
        else
        {
            // Fall back to AppData location
            var appDataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Avalised",
                "designer.db"
            );
            
            if (!System.IO.File.Exists(appDataPath))
            {
                throw new Exception($"designer.db not found in current directory or {appDataPath}");
            }
            
            dbPath = appDataPath;
            Console.WriteLine($"✅ Using designer.db from AppData: {dbPath}");
        }

        // Create designer layout - it loads everything from database
        _designerLayout = new DesignerLayout(dbPath);

        // Create action executor
        _actionExecutor = new ActionExecutor(dbPath, this);
        _actionExecutor.SetDesignerLayout(_designerLayout);

        // That's it! DesignerWindow includes menu, canvas, status bar
        Content = _designerLayout;
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
        if (_designerLayout?.Actions == null || _actionExecutor == null)
        {
            Console.WriteLine("⚠️  No actions to wire up");
            return;
        }

        Console.WriteLine($"🔗 Wiring up {_designerLayout.Actions.Count} soft-coded actions...");

        foreach (var action in _designerLayout.Actions)
        {
            if (action.Control is MenuItem menuItem)
            {
                // Wire MenuItem to ActionExecutor
                menuItem.Click += async (s, e) =>
                {
                    await _actionExecutor.ExecuteAsync(action.ActionName, action.Parameters);
                };
                Console.WriteLine($"   ✓ {menuItem.Name}: {action.ActionName}");
            }
            else if (action.Control is Button button)
            {
                // Wire Button to ActionExecutor (toolbox buttons!)
                button.Click += async (s, e) =>
                {
                    await _actionExecutor.ExecuteAsync(action.ActionName, action.Parameters);
                };
                Console.WriteLine($"   ✓ {button.Name}: {action.ActionName}");
            }
        }

        Console.WriteLine("✅ All actions wired!");

        // Keep legacy handlers for items not yet converted to Action system
        var menuItems = new Dictionary<string, MenuItem>();
        FindMenuItems(this, menuItems);

        // These still need hardcoded handlers (not yet in AVML as Actions)
        WireMenuItem(menuItems, "HelpSystemInfo", OnHelpSystemInfo);
        WireMenuItem(menuItems, "HelpWiki", OnHelpWiki);
        WireMenuItem(menuItems, "PreviewToggle", OnPreviewToggle);
        WireMenuItem(menuItems, "PreviewRun", OnPreviewRun);
        WireMenuItem(menuItems, "ViewOptions", OnViewOptions);
        WireMenuItem(menuItems, "ToolsOptions", OnViewOptions);
        WireMenuItem(menuItems, "ToolsMarkupEditor", OnMarkupEditor);
        WireMenuItem(menuItems, "TestCreateControl", OnTestCreateControl);
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

    private async void OnHelpGettingStarted(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.AboutDialog();
        await dialog.ShowDialog(this);
    }

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

    private void OnFileReload(object? sender, RoutedEventArgs e)
    {
        // Reload the designer from database
        if (_designerLayout != null)
        {
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Avalised",
                "designer.db"
            );

            // Recreate designer layout
            _designerLayout = new DesignerLayout(dbPath);
            Content = _designerLayout;

            // Rewire event handlers
            WireUpEventHandlers();
            DumpVisualTree(this);
        }
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

    // File and Test menu handlers are now soft-coded via ActionExecutor!
    // See Action properties in designer-window.avml

    #endregion

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.F8)
            OnPreviewToggle(this, new RoutedEventArgs());
        else if (e.Key == Key.F9)
            OnPreviewRun(this, new RoutedEventArgs());
    }

    private void DumpVisualTree(Control? control, int indent = 0, System.IO.StreamWriter? writer = null)
    {
        if (control == null) return;
        bool ownWriter = writer == null;
        if (ownWriter)
        {
            writer = new System.IO.StreamWriter(System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "avalised-tree.txt"));
        }

        writer.WriteLine($"{new string(' ', indent)}{control.GetType().Name} [{control.Name ?? "(unnamed)"}]" +
                        (control.IsVisible ? "" : " [HIDDEN]"));

        foreach (var child in control.GetVisualChildren())
        {
            if (child is Control c)
                DumpVisualTree(c, indent + 2, writer);
        }

        if (ownWriter)
        {
            writer.Close();
            Console.WriteLine("✅ Visual tree dumped to ~/avalised-tree.txt");
        }
    }

    private void OnTestCreateControl(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("🧪 TEST: Menu clicked, calling TestCreateControl...");
        _designerLayout?.TestCreateControl();
    }

}





