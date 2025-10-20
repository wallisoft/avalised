using System.Collections.Generic;

namespace ConfigUI.Models;

public class ConfigDefinition
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public bool RunAsSudo { get; set; } = false;
    public WindowSettings? Window { get; set; }
    public List<MenuDefinition>? Menus { get; set; }
    public List<ControlDefinition> Fields { get; set; } = new();
}

public class WindowSettings
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public string Title { get; set; } = "Config Editor";
}

public class MenuDefinition
{
    public string Text { get; set; } = "";
    public List<MenuItemDefinition> Items { get; set; } = new();
}

public class MenuItemDefinition
{
    public string Text { get; set; } = "";
    public string? OnClick { get; set; }
    public bool IsSeparator { get; set; } = false;
}

public class ControlDefinition
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "textbox";
    public string? Text { get; set; }
    public string? Label { get; set; }
    public PositionSettings? Position { get; set; }
    
    // Event handlers
    public string? OnClick { get; set; }
    public string? OnChange { get; set; }
    public string? OnLoad { get; set; }
    public string? OnRowDelete { get; set; }
    public string? OnRowSelect { get; set; }
    public string? OnFocus { get; set; }
    public string? OnBlur { get; set; }
    
    // Validation
    public string? Validation { get; set; }
    
    // DataGrid specific
    public List<ColumnDefinition>? Columns { get; set; }
}

public class PositionSettings
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class ColumnDefinition
{
    public string Name { get; set; } = "";
    public string Header { get; set; } = "";
    public int Width { get; set; } = 100;
}
