using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives; 
using Avalonia.Layout;
using Avalonia.Media;
using ConfigUI.Models;
using System;
using System.Collections.Generic;

namespace ConfigUI.Services
{
    public class UIBuilder
    {
        public Control CreateControl(ConfigDefinition config)
        {
            Control control;

            switch (config.Type.ToLower())
            {
                case "label":
                    control = new TextBlock
                    {
                        Text = config.Caption ?? config.Text ?? "",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    break;

                case "button":
                    control = new Button
                    {
                        Content = config.Caption ?? config.Text ?? "Button",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    break;

                case "textbox":
                    control = new TextBox
                    {
                        Text = config.Text ?? "",
                        Watermark = config.Placeholder
                    };
                    break;

                case "checkbox":
                    control = new CheckBox
                    {
                        Content = config.Caption ?? config.Text ?? "CheckBox",
                        IsChecked = config.Checked ?? false
                    };
                    break;

                case "combobox":
                    control = new ComboBox
                    {
                        PlaceholderText = config.Placeholder ?? config.Caption
                    };
                    break;

                case "panel":
                    control = new Panel();
                    break;

                case "canvas":
                    control = new Canvas();
                    break;

                case "stackpanel":
                    control = new StackPanel();
                    break;

                case "menubar":
                    var menu = new Menu();
                    if (config.Items != null)
                    {
                        BuildMenuItems(menu.Items, config.Items);
                    }
                    control = menu;
                    break;

                case "expander":
                    var expander = new Expander
                    {
                        IsExpanded = true
                    };
                    
                    if (!string.IsNullOrEmpty(config.Caption))
                    {
                        expander.Header = new TextBlock
                        {
                            Text = config.Caption,
                            FontSize = config.FontSize ?? 11,
                            FontWeight = config.FontBold ? FontWeight.Bold : FontWeight.Normal,
                            Foreground = !string.IsNullOrEmpty(config.ForegroundColor) 
                                ? new SolidColorBrush(Color.Parse(config.ForegroundColor))
                                : Brushes.Black
                        };
                    }
                    
                    control = expander;
                    break;

                default:
                    control = new TextBlock
                    {
                        Text = $"[{config.Type}]",
                        Foreground = Brushes.Red
                    };
                    break;
            }

            // Apply common properties
            if (config.Width.HasValue)
                control.Width = config.Width.Value;
            if (config.Height.HasValue)
                control.Height = config.Height.Value;

            if (!string.IsNullOrEmpty(config.BackgroundColor) && control is Panel panel)
            {
                panel.Background = new SolidColorBrush(Color.Parse(config.BackgroundColor));
            }

            if (!string.IsNullOrEmpty(config.ForegroundColor) && control is TemplatedControl textControl)
            {
                textControl.Foreground = new SolidColorBrush(Color.Parse(config.ForegroundColor));
            }

            if (!string.IsNullOrEmpty(config.BorderColor) && control is Border border)
            {
                border.BorderBrush = new SolidColorBrush(Color.Parse(config.BorderColor));
            }

            if (config.BorderThickness.HasValue && control is Border borderControl)
            {
                borderControl.BorderThickness = new Thickness(config.BorderThickness.Value);
            }

            control.IsVisible = config.Visible;
            control.IsEnabled = config.Enabled;

            if (!string.IsNullOrEmpty(config.Name))
            {
                control.Name = config.Name;
            }

            // Force font properties to override system defaults
            if (control is TemplatedControl tc)            {
                if (config.FontSize.HasValue && config.FontSize.Value > 0)
                {
                    tc.FontSize = config.FontSize.Value;
                }
                if (!string.IsNullOrEmpty(config.FontFamily))
                {
                    tc.FontFamily = new FontFamily(config.FontFamily);
                }
                if (config.FontBold)
                {
                    tc.FontWeight = FontWeight.Bold;
                }
            }

            return control;
        }

        private void BuildMenuItems(Avalonia.Controls.ItemCollection targetItems, List<Dictionary<string, object>> sourceItems)
        {
            foreach (var itemData in sourceItems)
            {
                var header = itemData.ContainsKey("header") ? itemData["header"].ToString() : "";

                if (header == "---")
                {
                    targetItems.Add(new Separator());
                    continue;
                }

                var menuItem = new MenuItem
                {
                    Header = header
                };

                if (itemData.ContainsKey("items") && itemData["items"] is List<Dictionary<string, object>> subItems)
                {
                    BuildMenuItems(menuItem.Items, subItems);
                }

                targetItems.Add(menuItem);
            }
        }
    }
}