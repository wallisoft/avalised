using Avalonia.Controls;
using System;
using System.IO;
using System.Linq;

namespace ConfigUI.Designer
{
    public static class DesignerHelpers
    {
        public static T? FindControlByName<T>(Canvas canvas, string name) where T : Control
        {
            foreach (var child in canvas.Children)
            {
                if (child is T control && control.Name == name) return control;
                if (child is Border border)
                {
                    if (border.Name == name && border is T t) return t;
                    if (border.Child is T innerControl && innerControl.Name == name) return innerControl;
                    if (border.Child is Canvas innerCanvas)
                    {
                        var found = FindControlByName<T>(innerCanvas, name);
                        if (found != null) return found;
                    }
                    if (border.Child is Panel panel)
                    {
                        foreach (var panelChild in panel.Children)
                        {
                            if (panelChild is T panelControl && panelControl.Name == name) return panelControl;
                            if (panelChild is Border innerBorder && innerBorder.Child is T innerT && innerT.Name == name) return innerT;
                        }
                    }
                }
            }
            return null;
        }

        public static Canvas? FindCanvasByName(Canvas canvas, string name)
        {
            var canvasPanelBorder = canvas.Children.OfType<Border>().FirstOrDefault(b => b.Name == "CanvasPanel");
            if (canvasPanelBorder?.Child is Canvas canvasPanelCanvas)
            {
                var scrollBorder = canvasPanelCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Name == "CanvasScrollContainer");
                if (scrollBorder?.Child is Canvas scrollCanvas)
                {
                    var designBorder = scrollCanvas.Children.OfType<Border>().FirstOrDefault(b => b.Name == "DesignCanvas");
                    if (designBorder?.Child is Canvas designCanvas)
                    {
                        designCanvas.Width = designBorder.Width;
                        designCanvas.Height = designBorder.Height;
                        return designCanvas;
                    }
                }
            }
            return null;
        }

        public static string? FindYamlFile(string filename)
        {
            if (File.Exists(filename)) return filename;
            var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (exeDir != null)
            {
                var exePath = Path.Combine(exeDir, filename);
                if (File.Exists(exePath)) return exePath;
            }
            return null;
        }

        public static string[] GetEventsForControlType(string type)
        {
            return type.ToLower() switch
            {
                "button" => new[] { "onClick" },
                "textbox" => new[] { "onChange", "onEnter", "onLeave" },
                "checkbox" => new[] { "onChange" },
                "radiobutton" => new[] { "onChange" },
                "combobox" => new[] { "onSelectionChanged" },
                "listbox" => new[] { "onSelectionChanged", "onDoubleClick" },
                "timer" => new[] { "onTick" },
                "panel" => new[] { "onClick" },
                "label" => new[] { "onClick" },
                "form" => new[] { "onLoad", "onClose" },
                _ => new[] { "onClick" }
            };
        }
    }
}
