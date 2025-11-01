using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Avalised;

public class UITreeBuilder
{
    private readonly string _dbPath;
    private readonly Dictionary<int, Control> _controlCache = new();
    private readonly List<ControlAction> _actions = new();

    public List<ControlAction> Actions => _actions;

    public UITreeBuilder(string dbPath)
    {
        _dbPath = dbPath;
    }

    public Control BuildUI()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var rootId = GetRootId(connection);
        if (rootId == 0)
            return new TextBlock { Text = "No root element found in database" };

        return BuildControl(connection, rootId);
    }

    private int GetRootId(SqliteConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE is_root = 1 LIMIT 1";
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    private Control BuildControl(SqliteConnection connection, int id)
    {
        var (controlType, name) = GetControlInfo(connection, id);
        
        var control = CreateControl(controlType);
        if (control == null)
            return new TextBlock { Text = $"Unknown control type: {controlType}" };
               
        control.Name = name;

        ApplyProperties(connection, id, control);
        ApplyAttachedProperties(connection, id, control);

        if (control is Panel panel)
        {
            BuildChildren(connection, id, panel);
        }
        else if (control is Menu menu)
        {
            BuildMenuItems(connection, id, menu);
        }
        else if (control is MenuItem menuItem)
        {
            BuildMenuItems(connection, id, menuItem);
        }
        else if (control is Border border)
        {
            BuildBorderContent(connection, id, border);
        }
                else if (control is ContentControl contentControl && controlType != "Button")
        {
            BuildChildren(connection, id, contentControl);
        }
        else if (control is ScrollViewer scrollViewer)
        {
            BuildScrollViewerContent(connection, id, scrollViewer);
        }

        _controlCache[id] = control;
        return control;
    }

    private (string type, string name) GetControlInfo(SqliteConnection connection, int id)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT control_type, name FROM ui_tree WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return (reader.GetString(0), reader.IsDBNull(1) ? "" : reader.GetString(1));
        }
        return ("", "");
    }

    private Control? CreateControl(string controlType)
    {
        return controlType switch
        {
            "TextBlock" => new TextBlock(),
            "Button" => new Button(),
            "TextBox" => new TextBox(),
            "StackPanel" => new StackPanel(),
            "DockPanel" => new DockPanel(),
            "Grid" => new Grid(),
            "Canvas" => new Canvas(),
            "Border" => new Border(),
            "ScrollViewer" => new ScrollViewer(),
            "Menu" => new Menu(),
            "MenuItem" => new MenuItem(),
            "Separator" => new Separator(),
            "Window" => new Window(),
            _ => null
        };
    }

    private void ApplyProperties(SqliteConnection connection, int id, Control control)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT property_name, property_value FROM ui_properties WHERE ui_tree_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        string? actionName = null;
        string? actionParams = null;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var propName = reader.GetString(0);
            var propValue = reader.GetString(1);

            // Capture Action properties for later wiring
            if (propName == "Action")
            {
                actionName = propValue;
            }
            else if (propName == "ActionParams")
            {
                actionParams = propValue;
            }
            else
            {
                SetProperty(control, propName, propValue);
            }
        }

        // Store action for wiring
        if (!string.IsNullOrEmpty(actionName))
        {
            var parameters = ParseActionParams(actionParams);
            _actions.Add(new ControlAction
            {
                Control = control,
                ActionName = actionName,
                Parameters = parameters
            });
        }
    }

    private Dictionary<string, string> ParseActionParams(string? actionParams)
    {
        var parameters = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(actionParams))
            return parameters;

        // Parse semicolon-separated key=value pairs
        var pairs = actionParams.Split(';');
        foreach (var pair in pairs)
        {
            var parts = pair.Split(new[] { '=' }, 2);
            if (parts.Length == 2)
            {
                parameters[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return parameters;
    }

    private void SetProperty(Control control, string name, string value)
    {
        try
        {
            switch (name)
            {
                case "Width":
                    control.Width = double.Parse(value);
                    break;
                case "Height":
                    control.Height = double.Parse(value);
                    break;
                case "Background":
                    if (control is Panel panel)
                        panel.Background = Brush.Parse(value);
                    else if (control is Border border)
                        border.Background = Brush.Parse(value);
                    else if (control is Menu menu)
                        menu.Background = Brush.Parse(value);
                    break;
                case "Foreground":
                    if (control is TextBlock tb)
                        tb.Foreground = Brush.Parse(value);
                    else if (control is MenuItem mi)
                        mi.Foreground = Brush.Parse(value);
                    else if (control is Menu menu2)
                        menu2.Foreground = Brush.Parse(value);
                    break;
                case "Text":
                    if (control is TextBlock textBlock)
                        textBlock.Text = value;
                    break;
                case "Content":
                    if (control is Button button)
                        button.Content = value;
                    break;
                case "Header":
                    if (control is MenuItem menuItem)
                        menuItem.Header = value;
                    break;
                case "InputGesture":
                    // Ignore for now
                    break;
                case "FontSize":
                    if (control is TextBlock textBlock2)
                        textBlock2.FontSize = double.Parse(value);
                    else if (control is Button button2)
                        button2.FontSize = double.Parse(value);
                    else if (control is TextBox textBox)
                        textBox.FontSize = double.Parse(value);
                    break;
                case "Margin":
                    control.Margin = Thickness.Parse(value);
                    break;
                case "Padding":
                    if (control is Border border4)
                        border4.Padding = Thickness.Parse(value);
                    else if (control is Decorator decorator)
                        decorator.Padding = Thickness.Parse(value);
                    break;
                case "HorizontalAlignment":
                    control.HorizontalAlignment = Enum.Parse<HorizontalAlignment>(value, true);
                    break;
                case "VerticalAlignment":
                    control.VerticalAlignment = Enum.Parse<VerticalAlignment>(value, true);
                    break;
                case "Orientation":
                    if (control is StackPanel sp)
                        sp.Orientation = Enum.Parse<Orientation>(value, true);
                    break;
                case "LastChildFill":
                    if (control is DockPanel dp)
                        dp.LastChildFill = bool.Parse(value);
                    break;
                case "BorderThickness":
                    if (control is Border border2)
                        border2.BorderThickness = Thickness.Parse(value);
                    break;
                case "BorderBrush":
                    if (control is Border border3)
                        border3.BorderBrush = Brush.Parse(value);
                    break;
            }
        }
        catch { }
    }

    private void ApplyAttachedProperties(SqliteConnection connection, int id, Control control)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT attached_property_name, property_value FROM ui_attached_properties WHERE ui_tree_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            SetAttachedProperty(control, reader.GetString(0), reader.GetString(1));
        }
    }

    private void SetAttachedProperty(Control control, string name, string value)
    {
        try
        {
            if (name == "DockPanel.Dock")
                DockPanel.SetDock(control, Enum.Parse<Dock>(value, true));
            else if (name.StartsWith("Grid."))
            {
                var gridProp = name.Substring(5);
                switch (gridProp)
                {
                    case "Row": Grid.SetRow(control, int.Parse(value)); break;
                    case "Column": Grid.SetColumn(control, int.Parse(value)); break;
                    case "RowSpan": Grid.SetRowSpan(control, int.Parse(value)); break;
                    case "ColumnSpan": Grid.SetColumnSpan(control, int.Parse(value)); break;
                }
            }
        }
        catch { }
    }

    private void BuildChildren(SqliteConnection connection, int parentId, Panel panel)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE parent_id = @pid ORDER BY display_order";
        cmd.Parameters.AddWithValue("@pid", parentId);

        var childIds = new List<int>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                childIds.Add(reader.GetInt32(0));
        }

        foreach (var childId in childIds)
            panel.Children.Add(BuildControl(connection, childId));
    }

    private void BuildMenuItems(SqliteConnection connection, int parentId, Menu menu)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE parent_id = @pid ORDER BY display_order";
        cmd.Parameters.AddWithValue("@pid", parentId);

        var childIds = new List<int>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                childIds.Add(reader.GetInt32(0));
        }

        foreach (var childId in childIds)
        {
            var child = BuildControl(connection, childId);
            menu.Items.Add(child);
        }
    }

    private void BuildMenuItems(SqliteConnection connection, int parentId, MenuItem menuItem)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE parent_id = @pid ORDER BY display_order";
        cmd.Parameters.AddWithValue("@pid", parentId);

        var childIds = new List<int>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                childIds.Add(reader.GetInt32(0));
        }

        foreach (var childId in childIds)
        {
            var child = BuildControl(connection, childId);
            menuItem.Items.Add(child);
        }
    }

    private void BuildChildren(SqliteConnection connection, int parentId, ContentControl contentControl)
    {
        Console.WriteLine($"Building children for ContentControl parent_id={parentId}, name={contentControl.Name}");
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE parent_id = @pid ORDER BY display_order LIMIT 1";
        cmd.Parameters.AddWithValue("@pid", parentId);

        var result = cmd.ExecuteScalar();
        Console.WriteLine($"  Found child id: {result}");
        if (result != null)
        {
            var child = BuildControl(connection, Convert.ToInt32(result));
            Console.WriteLine($"  Created child: {child.GetType().Name} [{child.Name}]");
            contentControl.Content = child;
            Console.WriteLine($"  Set content on {contentControl.Name}");
        }
        else
        {
            Console.WriteLine($"  No children found for {contentControl.Name}");
        }
    }

    private void BuildScrollViewerContent(SqliteConnection connection, int parentId, ScrollViewer scrollViewer)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE parent_id = @pid ORDER BY display_order LIMIT 1";
        cmd.Parameters.AddWithValue("@pid", parentId);

        var result = cmd.ExecuteScalar();
        if (result != null)
            scrollViewer.Content = BuildControl(connection, Convert.ToInt32(result));
    }

    private void BuildBorderContent(SqliteConnection connection, int parentId, Border border)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM ui_tree WHERE parent_id = @pid ORDER BY display_order LIMIT 1";
        cmd.Parameters.AddWithValue("@pid", parentId);

        var result = cmd.ExecuteScalar();
        if (result != null)
        {
            border.Child = BuildControl(connection, Convert.ToInt32(result));
        }
    }
}


/// <summary>
/// Stores action data for a control
/// </summary>
public class ControlAction
{
    public Control Control { get; set; }
    public string ActionName { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
}


