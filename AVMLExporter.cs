using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;

namespace Avalised;

/// <summary>
/// Export Avalonia controls back to AVML format
/// The reverse of AVMLLoader - completing the round-trip
/// </summary>
public class AVMLExporter
{
    private int _indent = 0;
    private StringBuilder _yaml = new();
    
    /// <summary>
    /// Export a control tree to AVML string
    /// </summary>
    public string ExportToString(Control control)
    {
        _yaml.Clear();
        _indent = 0;
        
        ExportControl(control, control.Name ?? "Root");
        
        return _yaml.ToString();
    }
    
    /// <summary>
    /// Export a control tree to AVML file
    /// </summary>
    public void ExportToFile(Control control, string filePath)
    {
        string avml = ExportToString(control);
        File.WriteAllText(filePath, avml);
    }
    
    private void ExportControl(Control control, string name)
    {
        string controlType = GetControlType(control);
        
        WriteLine($"{name}:");
        _indent++;
        WriteLine($"Type: {controlType}");
        
        // Export properties
        ExportProperties(control);
        
        // Export children
        ExportChildren(control);
        
        _indent--;
    }
    
    private void ExportProperties(Control control)
    {
        // Common properties
        if (control.Width > 0 && !double.IsNaN(control.Width))
            WriteLine($"Width: {control.Width}");
            
        if (control.Height > 0 && !double.IsNaN(control.Height))
            WriteLine($"Height: {control.Height}");
            
        if (control.Margin != default)
            WriteLine($"Margin: {FormatThickness(control.Margin)}");
            
        if (control.HorizontalAlignment != HorizontalAlignment.Stretch)
            WriteLine($"HorizontalAlignment: {control.HorizontalAlignment}");
            
        if (control.VerticalAlignment != VerticalAlignment.Stretch)
            WriteLine($"VerticalAlignment: {control.VerticalAlignment}");
        
        // Control-specific properties
        switch (control)
        {
            case Window window:
                if (!string.IsNullOrEmpty(window.Title))
                    WriteLine($"Title: {window.Title}");
                if (window.MinWidth > 0)
                    WriteLine($"MinWidth: {window.MinWidth}");
                if (window.MinHeight > 0)
                    WriteLine($"MinHeight: {window.MinHeight}");
                break;
                
            case CheckBox checkBox:
                if (checkBox.Content != null)
                    WriteLine($"Content: {checkBox.Content}");
                if (checkBox.IsChecked == true)
                    WriteLine($"IsChecked: true");
                break;
                
            case Button button:
                if (button.Content != null)
                    WriteLine($"Content: {button.Content}");
                break;
                
            case TextBlock textBlock:
                if (!string.IsNullOrEmpty(textBlock.Text))
                    WriteLine($"Text: {textBlock.Text}");
                if (textBlock.FontSize != 12)
                    WriteLine($"FontSize: {textBlock.FontSize}");
                if (textBlock.FontWeight != FontWeight.Normal)
                    WriteLine($"FontWeight: {textBlock.FontWeight}");
                if (textBlock.Foreground is SolidColorBrush brush)
                    WriteLine($"Foreground: \"{FormatColor(brush.Color)}\"");
                break;
                
            case TextBox textBox:
                if (!string.IsNullOrEmpty(textBox.Text))
                    WriteLine($"Text: {textBox.Text}");
                if (!string.IsNullOrEmpty(textBox.Watermark))
                    WriteLine($"Watermark: {textBox.Watermark}");
                break;
                
            case StackPanel stackPanel:
                if (stackPanel.Orientation != Orientation.Vertical)
                    WriteLine($"Orientation: {stackPanel.Orientation}");
                if (stackPanel.Spacing > 0)
                    WriteLine($"Spacing: {stackPanel.Spacing}");
                if (stackPanel.Background != null)
                    WriteLine($"Background: \"{FormatBrush(stackPanel.Background)}\"");
                break;
                
            case DockPanel dockPanel:
                if (!dockPanel.LastChildFill)
                    WriteLine($"LastChildFill: false");
                if (dockPanel.Background != null)
                    WriteLine($"Background: \"{FormatBrush(dockPanel.Background)}\"");
                break;
                
            case Border border:
                if (border.Background != null)
                    WriteLine($"Background: \"{FormatBrush(border.Background)}\"");
                if (border.BorderThickness != default)
                    WriteLine($"BorderThickness: {FormatThickness(border.BorderThickness)}");
                if (border.BorderBrush != null)
                    WriteLine($"BorderBrush: \"{FormatBrush(border.BorderBrush)}\"");
                if (border.Padding != default)
                    WriteLine($"Padding: {FormatThickness(border.Padding)}");
                break;
                
            case Canvas canvas:
                if (canvas.Background != null)
                    WriteLine($"Background: \"{FormatBrush(canvas.Background)}\"");
                break;
        }
    }
    
    private void ExportChildren(Control control)
    {
        List<Control> children = GetChildren(control);
        
        if (children.Count == 0)
            return;
            
        if (children.Count == 1)
        {
            // Single child - use Content
            WriteLine("Content:");
            _indent++;
            ExportControl(children[0], children[0].Name ?? "Child");
            _indent--;
        }
        else
        {
            // Multiple children - use Children list
            WriteLine("Children:");
            _indent++;
            
            foreach (var child in children)
            {
                WriteLine($"- {child.Name ?? "Child"}:");
                _indent++;
                WriteLine($"Type: {GetControlType(child)}");
                ExportProperties(child);
                ExportChildren(child);
                _indent--;
            }
            
            _indent--;
        }
    }
    
    private List<Control> GetChildren(Control control)
    {
        var children = new List<Control>();
        
        switch (control)
        {
            case Panel panel:
                children.AddRange(panel.Children);
                break;
            case Border border when border.Child != null:
                children.Add(border.Child);
                break;
            case ContentControl content when content.Content is Control child:
                children.Add(child);
                break;
            case Decorator decorator when decorator.Child != null:
                children.Add(decorator.Child);
                break;
        }
        
        return children;
    }
    
    private string GetControlType(Control control)
    {
        return control switch
        {
            Window => "Window",
            CheckBox => "CheckBox",
            RadioButton => "RadioButton",
            Button => "Button",
            TextBox => "TextBox",
            TextBlock => "TextBlock",
            Label => "Label",
            ComboBox => "ComboBox",
            StackPanel => "StackPanel",
            DockPanel => "DockPanel",
            Canvas => "Canvas",
            Grid => "Grid",
            Border => "Border",
            ScrollViewer => "ScrollViewer",
            MenuItem => "MenuItem",
            Menu => "Menu",
            Separator => "Separator",
            _ => control.GetType().Name
        };
    }
    
    private string FormatThickness(Thickness thickness)
    {
        if (thickness.Left == thickness.Right && 
            thickness.Top == thickness.Bottom)
        {
            if (thickness.Left == thickness.Top)
                return $"{thickness.Left}";
            return $"{thickness.Left},{thickness.Top}";
        }
        return $"{thickness.Left},{thickness.Top},{thickness.Right},{thickness.Bottom}";
    }
    
    private string FormatColor(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    
    private string FormatBrush(IBrush brush)
    {
        if (brush is SolidColorBrush solid)
            return FormatColor(solid.Color);
        return "Transparent";
    }
    
    private void WriteLine(string line)
    {
        _yaml.Append(new string(' ', _indent * 2));
        _yaml.AppendLine(line);
    }
}


