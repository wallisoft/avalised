using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ConfigUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace ConfigUI.Services;

public class UIBuilder
{
    private readonly ScriptEngine _scriptEngine;
    private readonly Dictionary<string, Control> _controls = new();
    
    public UIBuilder(ScriptEngine scriptEngine)
    {
        _scriptEngine = scriptEngine;
    }
    
    public Panel BuildUI(ConfigDefinition config)
    {
        var canvas = new Canvas
        {
            Background = Brushes.White
        };
        
        foreach (var field in config.Fields)
        {
            var control = CreateControl(field);
            if (control != null)
            {
                _controls[field.Name] = control;
                
                if (field.Position != null)
                {
                    Canvas.SetLeft(control, field.Position.X);
                    Canvas.SetTop(control, field.Position.Y);
                    control.Width = field.Position.Width;
                    control.Height = field.Position.Height;
                }
                
                canvas.Children.Add(control);
            }
        }
        
        return canvas;
    }
    
    private Control? CreateControl(ControlDefinition field)
    {
        Control? control = field.Type.ToLower() switch
        {
            "textbox" => CreateTextBox(field),
            "button" => CreateButton(field),
            "label" => CreateLabel(field),
            "combobox" => CreateComboBox(field),
            "datagrid" => CreateDataGrid(field),
            _ => null
        };
        
        return control;
    }
    
    private TextBox CreateTextBox(ControlDefinition field)
    {
        var textBox = new TextBox
        {
            Watermark = field.Label
        };
        
        _scriptEngine.RegisterControl(
            field.Name,
            () => textBox.Text ?? "",
            value => textBox.Text = value
        );
        
        if (!string.IsNullOrEmpty(field.OnChange))
        {
            textBox.TextChanged += async (s, e) =>
            {
                await _scriptEngine.ExecuteScriptAsync(field.OnChange);
            };
        }
        
        return textBox;
    }
    
    private Button CreateButton(ControlDefinition field)
    {
        var button = new Button
        {
            Content = field.Text ?? "Button"
        };
        
        if (!string.IsNullOrEmpty(field.OnClick))
        {
            button.Click += async (s, e) =>
            {
                await _scriptEngine.ExecuteScriptAsync(field.OnClick);
            };
        }
        
        return button;
    }
    
    private TextBlock CreateLabel(ControlDefinition field)
    {
        var label = new TextBlock
        {
            Text = field.Text ?? "",
            VerticalAlignment = VerticalAlignment.Center
        };
        
        _scriptEngine.RegisterControl(
            field.Name,
            () => label.Text ?? "",
            value => label.Text = value
        );
        
        return label;
    }
    
    private ComboBox CreateComboBox(ControlDefinition field)
    {
        var comboBox = new ComboBox
        {
            IsEditable = true
        };
        
        _scriptEngine.RegisterControl(
            field.Name,
            () => comboBox.SelectedItem?.ToString() ?? comboBox.Text ?? "",
            value => comboBox.Text = value
        );
        
        if (!string.IsNullOrEmpty(field.OnLoad))
        {
            _ = Task.Run(async () =>
            {
                var output = await _scriptEngine.ExecuteScriptAsync(field.OnLoad);
                var items = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                 .Where(s => !s.StartsWith("GET_") && !s.StartsWith("SET_"))
                                 .ToList();
                
                Dispatcher.UIThread.Post(() =>
                {
                    comboBox.ItemsSource = items;
                });
            });
        }
        
        return comboBox;
    }
    
    private DataGrid CreateDataGrid(ControlDefinition field)
    {
        var dataGrid = new DataGrid
        {
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        };
        
        // Placeholder for now
        return dataGrid;
    }
}
