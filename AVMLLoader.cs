using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Avalised;

/// <summary>
/// Direct AVML → Avalonia Control Loader v2
/// Reads proper YAML and builds controls on-the-fly
/// Format: ControlName: { Type: Window, Property: Value, ... }
/// </summary>
public class AVMLLoader
{
    private readonly IDeserializer _deserializer;
    
    public AVMLLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }
    
    public Control LoadFromFile(string avmlPath)
    {
        if (!File.Exists(avmlPath))
            throw new FileNotFoundException($"AVML file not found: {avmlPath}");
            
        string yaml = File.ReadAllText(avmlPath);
        return LoadFromString(yaml);
    }
    
    public Control LoadFromString(string avmlContent)
    {
        var data = _deserializer.Deserialize<Dictionary<object, object>>(avmlContent);
        
        if (data == null || data.Count == 0)
            throw new Exception("Empty or invalid AVML");
            
        // Get the root control (first key-value pair)
        foreach (var entry in data)
        {
            string controlName = entry.Key.ToString() ?? "Root";
            var properties = entry.Value as Dictionary<object, object>;
            return BuildControl(controlName, properties);
        }
        
        throw new Exception("No root control found");
    }
    
    private Control BuildControl(string name, Dictionary<object, object>? properties)
    {
        if (properties == null || !properties.ContainsKey("Type"))
            throw new Exception($"Control '{name}' missing Type property");
            
        string controlType = properties["Type"].ToString() ?? "Panel";
        Control control = CreateControl(controlType);
        control.Name = name;
        
        // Apply all properties
        foreach (var prop in properties)
        {
            string propName = prop.Key.ToString() ?? "";
            
            if (propName == "Type")
                continue; // Already handled
                
            if (propName == "Content")
            {
                // Single child
                ApplyContent(control, prop.Value);
            }
            else if (propName == "Children")
            {
                // Multiple children
                ApplyChildren(control, prop.Value);
            }
            else
            {
                // Regular property
                ApplyProperty(control, propName, prop.Value);
            }
        }
        
        return control;
    }
    
    private Control CreateControl(string type)
    {
        return type switch
        {
            "Window" => new Window(),
            "Button" => new Button(),
            "TextBox" => new TextBox(),
            "TextBlock" => new TextBlock(),
            "Label" => new Label(),
            "CheckBox" => new CheckBox(),
            "RadioButton" => new RadioButton(),
            "ComboBox" => new ComboBox(),
            "Panel" => new Panel(),
            "StackPanel" => new StackPanel(),
            "DockPanel" => new DockPanel(),
            "Canvas" => new Canvas(),
            "Grid" => new Grid(),
            "Border" => new Border(),
            "ScrollViewer" => new ScrollViewer(),
            "Menu" => new Menu(),
            "MenuItem" => new MenuItem(),
            "Separator" => new Separator(),
            _ => throw new Exception($"Unknown control type: {type}")
        };
    }
    
    private void ApplyContent(Control parent, object contentData)
    {
        if (contentData is not Dictionary<object, object> contentDict)
            return;
            
        foreach (var entry in contentDict)
        {
            string childName = entry.Key.ToString() ?? "Child";
            var childProps = entry.Value as Dictionary<object, object>;
            var childControl = BuildControl(childName, childProps);
            AddChildToParent(parent, childControl);
            break; // Content is single child
        }
    }
    
    private void ApplyChildren(Control parent, object childrenData)
    {
        if (childrenData is not List<object> childList)
            return;
            
        foreach (var childItem in childList)
        {
            if (childItem is not Dictionary<object, object> childDict)
                continue;
                
            foreach (var entry in childDict)
            {
                string childName = entry.Key.ToString() ?? "Child";
                var childProps = entry.Value as Dictionary<object, object>;
                var childControl = BuildControl(childName, childProps);
                AddChildToParent(parent, childControl);
                break; // Each list item has one control
            }
        }
    }
    
    private void AddChildToParent(Control parent, Control child)
    {
        switch (parent)
        {
            case Panel panel:
                panel.Children.Add(child);
                break;
            case Border border:
                border.Child = child;
                break;
            case ScrollViewer scroll:
                scroll.Content = child;
                break;
            case ContentControl content:
                content.Content = child;
                break;
            case Decorator decorator:
                decorator.Child = child;
                break;
            default:
                throw new Exception($"Cannot add children to {parent.GetType().Name}");
        }
    }
    
    private void ApplyProperty(Control control, string propName, object propValue)
    {
        string valueStr = propValue?.ToString() ?? "";
        
        // Common properties
        switch (propName)
        {
            case "Width":
                control.Width = ParseDouble(valueStr);
                return;
            case "Height":
                control.Height = ParseDouble(valueStr);
                return;
            case "MinWidth":
                control.MinWidth = ParseDouble(valueStr);
                return;
            case "MinHeight":
                control.MinHeight = ParseDouble(valueStr);
                return;
            case "Margin":
                control.Margin = ParseThickness(valueStr);
                return;
            case "HorizontalAlignment":
                control.HorizontalAlignment = ParseEnum<HorizontalAlignment>(valueStr);
                return;
            case "VerticalAlignment":
                control.VerticalAlignment = ParseEnum<VerticalAlignment>(valueStr);
                return;
        }
        
        // Control-specific properties
        switch (control)
        {
            case Window window:
                ApplyWindowProperty(window, propName, valueStr);
                break;
            case CheckBox checkBox:
                ApplyCheckBoxProperty(checkBox, propName, valueStr);
                break;
            case Button button:
                ApplyButtonProperty(button, propName, valueStr);
                break;
            case TextBlock textBlock:
                ApplyTextBlockProperty(textBlock, propName, valueStr);
                break;
            case TextBox textBox:
                ApplyTextBoxProperty(textBox, propName, valueStr);
                break;
            case StackPanel stackPanel:
                ApplyStackPanelProperty(stackPanel, propName, valueStr);
                break;
            case DockPanel dockPanel:
                ApplyDockPanelProperty(dockPanel, propName, valueStr);
                break;
            case Border border:
                ApplyBorderProperty(border, propName, valueStr);
                break;
            case Panel panel:
                ApplyPanelProperty(panel, propName, valueStr);
                break;
        }
    }
    
    private void ApplyWindowProperty(Window window, string propName, string value)
    {
        switch (propName)
        {
            case "Title":
                window.Title = value;
                break;
            case "CanResize":
                window.CanResize = ParseBool(value);
                break;
            case "ShowInTaskbar":
                window.ShowInTaskbar = ParseBool(value);
                break;
        }
    }
    
    private void ApplyButtonProperty(Button button, string propName, string value)
    {
        if (propName == "Content")
            button.Content = value;
    }
    
    private void ApplyTextBlockProperty(TextBlock textBlock, string propName, string value)
    {
        switch (propName)
        {
            case "Text":
                textBlock.Text = value;
                break;
            case "FontSize":
                textBlock.FontSize = ParseDouble(value);
                break;
            case "FontWeight":
                textBlock.FontWeight = ParseFontWeight(value);
                break;
            case "Foreground":
                textBlock.Foreground = ParseBrush(value);
                break;
        }
    }
    
    private void ApplyTextBoxProperty(TextBox textBox, string propName, string value)
    {
        switch (propName)
        {
            case "Text":
                textBox.Text = value;
                break;
            case "Watermark":
                textBox.Watermark = value;
                break;
        }
    }
    
    private void ApplyCheckBoxProperty(CheckBox checkBox, string propName, string value)
    {
        switch (propName)
        {
            case "Content":
                checkBox.Content = value;
                break;
            case "IsChecked":
                checkBox.IsChecked = ParseBool(value);
                break;
        }
    }
    
    private void ApplyStackPanelProperty(StackPanel stackPanel, string propName, string value)
    {
        switch (propName)
        {
            case "Orientation":
                stackPanel.Orientation = ParseEnum<Orientation>(value);
                break;
            case "Spacing":
                stackPanel.Spacing = ParseDouble(value);
                break;
            case "Background":
                stackPanel.Background = ParseBrush(value);
                break;
        }
    }
    
    private void ApplyDockPanelProperty(DockPanel dockPanel, string propName, string value)
    {
        switch (propName)
        {
            case "LastChildFill":
                dockPanel.LastChildFill = ParseBool(value);
                break;
            case "Background":
                dockPanel.Background = ParseBrush(value);
                break;
        }
    }
    
    private void ApplyBorderProperty(Border border, string propName, string value)
    {
        switch (propName)
        {
            case "Background":
                border.Background = ParseBrush(value);
                break;
            case "BorderThickness":
                border.BorderThickness = ParseThickness(value);
                break;
            case "BorderBrush":
                border.BorderBrush = ParseBrush(value);
                break;
            case "CornerRadius":
                border.CornerRadius = new CornerRadius(ParseDouble(value));
                break;
            case "Padding":
                border.Padding = ParseThickness(value);
                break;
        }
    }
    
    private void ApplyPanelProperty(Panel panel, string propName, string value)
    {
        if (propName == "Background")
            panel.Background = ParseBrush(value);
    }
    
    // Parsing helpers
    private double ParseDouble(string value) => 
        double.TryParse(value, out var result) ? result : 0;
        
    private bool ParseBool(string value) => 
        bool.TryParse(value, out var result) && result;
        
    private T ParseEnum<T>(string value) where T : struct => 
        Enum.TryParse<T>(value, true, out var result) ? result : default;
    
    private FontWeight ParseFontWeight(string value)
    {
        return value.ToLower() switch
        {
            "bold" => FontWeight.Bold,
            "normal" => FontWeight.Normal,
            "light" => FontWeight.Light,
            _ => FontWeight.Normal
        };
    }
        
    private Thickness ParseThickness(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Thickness(0);
            
        var parts = value.Split(',');
        return parts.Length switch
        {
            1 => new Thickness(ParseDouble(parts[0])),
            2 => new Thickness(ParseDouble(parts[0]), ParseDouble(parts[1])),
            4 => new Thickness(ParseDouble(parts[0]), ParseDouble(parts[1]), 
                              ParseDouble(parts[2]), ParseDouble(parts[3])),
            _ => new Thickness(0)
        };
    }
    
    private IBrush ParseBrush(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Brushes.Black;
            
        try
        {
            return Brush.Parse(value);
        }
        catch
        {
            return Brushes.Black;
        }
    }
}

