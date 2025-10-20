using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigUI.Designer
{
    public class DesignerImportExport
    {
        private List<DesignerControl> _controls;
        private ScriptDatabase _database;
        private Window _parentWindow;

        public DesignerImportExport(List<DesignerControl> controls, ScriptDatabase database, Window parentWindow)
        {
            _controls = controls;
            _database = database;
            _parentWindow = parentWindow;
        }

        public async Task ShowExportDialog()
        {
            var dialog = new Window
            {
                Title = "Export Form",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#f0f0f0"))
            };

            var stack = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10
            };

            stack.Children.Add(new TextBlock
            {
                Text = "📤 Export Form As:",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            });

            var formats = new[] { "YAML", "JSON", "XML", "CSV", "INI", "TOML", "SQL" };
            foreach (var format in formats)
            {
                var btn = new Button
                {
                    Content = $"Export as {format}",
                    Height = 35,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    Padding = new Avalonia.Thickness(10, 0, 0, 0)
                };
                btn.Click += async (s, e) =>
                {
                    await ExportAs(format);
                    dialog.Close();
                };
                stack.Children.Add(btn);
            }

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Height = 35,
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            };
            cancelBtn.Click += (s, e) => dialog.Close();
            stack.Children.Add(cancelBtn);

            dialog.Content = stack;
            await dialog.ShowDialog(_parentWindow);
        }

        public async Task ShowImportDialog()
        {
            var dialog = new Window
            {
                Title = "Import Form",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#f0f0f0"))
            };

            var stack = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10
            };

            stack.Children.Add(new TextBlock
            {
                Text = "📥 Import Form From:",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            });

            var formats = new[] { "YAML", "JSON", "XML", "CSV", "SQL" };
            foreach (var format in formats)
            {
                var btn = new Button
                {
                    Content = $"Import from {format}",
                    Height = 35,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    Padding = new Avalonia.Thickness(10, 0, 0, 0)
                };
                btn.Click += async (s, e) =>
                {
                    await ImportFrom(format);
                    dialog.Close();
                };
                stack.Children.Add(btn);
            }

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Height = 35,
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            };
            cancelBtn.Click += (s, e) => dialog.Close();
            stack.Children.Add(cancelBtn);

            dialog.Content = stack;
            await dialog.ShowDialog(_parentWindow);
        }

        private async Task ExportAs(string format)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = $"Export as {format}",
                    DefaultExtension = format.ToLower(),
                    InitialFileName = $"form.{format.ToLower()}"
                };

                var result = await saveDialog.ShowAsync(_parentWindow);
                if (result != null)
                {
                    var content = format switch
                    {
                        "YAML" => ExportToYAML(),
                        "JSON" => ExportToJSON(),
                        "XML" => ExportToXML(),
                        "CSV" => ExportToCSV(),
                        "INI" => ExportToINI(),
                        "TOML" => ExportToTOML(),
                        "SQL" => ExportToSQL(),
                        _ => ""
                    };

                    await File.WriteAllTextAsync(result, content);
                    Console.WriteLine($"✅ Exported to {format}: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Export error: {ex.Message}");
            }
        }

        private async Task ImportFrom(string format)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = $"Import from {format}",
                    AllowMultiple = false
                };

                var result = await openDialog.ShowAsync(_parentWindow);
                if (result != null && result.Length > 0)
                {
                    var content = await File.ReadAllTextAsync(result[0]);
                    
                    var importedControls = format switch
                    {
                        "YAML" => ImportFromYAML(content),
                        "JSON" => ImportFromJSON(content),
                        "XML" => ImportFromXML(content),
                        "CSV" => ImportFromCSV(content),
                        "SQL" => ImportFromSQL(content),
                        _ => new List<DesignerControl>()
                    };

                    if (importedControls.Count > 0)
                    {
                        _controls.Clear();
                        _controls.AddRange(importedControls);
                    }

                    Console.WriteLine($"✅ Imported {importedControls.Count} controls from {format}: {result[0]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Import error: {ex.Message}");
            }
        }

        public string ExportToYAML()
        {
            var sb = new StringBuilder();
            sb.AppendLine("window:");
            sb.AppendLine("  width: 800");
            sb.AppendLine("  height: 600");
            sb.AppendLine("  title: \"Exported Form\"");
            sb.AppendLine();
            sb.AppendLine("controls:");
            
            foreach (var control in _controls)
            {
                sb.AppendLine($"  - type: {control.Type}");
                sb.AppendLine($"    name: {control.Name}");
                if (!string.IsNullOrEmpty(control.Caption))
                    sb.AppendLine($"    caption: \"{control.Caption}\"");
                if (!string.IsNullOrEmpty(control.Text))
                    sb.AppendLine($"    text: \"{control.Text}\"");
                sb.AppendLine($"    x: {control.X:F0}");
                sb.AppendLine($"    y: {control.Y:F0}");
                sb.AppendLine($"    width: {control.Width:F0}");
                sb.AppendLine($"    height: {control.Height:F0}");
                
                if (control.DatabaseId.HasValue)
                {
                    var scripts = _database.GetAllScriptsForControl(control.DatabaseId.Value);
                    foreach (var kvp in scripts)
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Value))
                        {
                            sb.AppendLine($"    {kvp.Key}: |");
                            foreach (var line in kvp.Value.Split('\n'))
                            {
                                sb.AppendLine($"      {line}");
                            }
                        }
                    }
                }
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private string ExportToJSON()
        {
            var data = new
            {
                window = new { width = 800, height = 600, title = "Exported Form" },
                controls = _controls.Select(c => new
                {
                    type = c.Type,
                    name = c.Name,
                    caption = c.Caption,
                    text = c.Text,
                    x = c.X,
                    y = c.Y,
                    width = c.Width,
                    height = c.Height,
                    visible = c.Visible,
                    enabled = c.Enabled,
                    scripts = c.DatabaseId.HasValue 
                        ? _database.GetAllScriptsForControl(c.DatabaseId.Value) 
                        : new Dictionary<string, string>()
                }).ToList()
            };
            
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        private string ExportToXML()
        {
            var root = new XElement("Form",
                new XElement("Window",
                    new XElement("Width", 800),
                    new XElement("Height", 600),
                    new XElement("Title", "Exported Form")
                ),
                new XElement("Controls",
                    _controls.Select(c =>
                        new XElement("Control",
                            new XElement("Type", c.Type),
                            new XElement("Name", c.Name),
                            new XElement("Caption", c.Caption ?? ""),
                            new XElement("Text", c.Text ?? ""),
                            new XElement("X", c.X),
                            new XElement("Y", c.Y),
                            new XElement("Width", c.Width),
                            new XElement("Height", c.Height),
                            new XElement("Visible", c.Visible),
                            new XElement("Enabled", c.Enabled),
                            c.DatabaseId.HasValue
                                ? new XElement("Scripts",
                                    _database.GetAllScriptsForControl(c.DatabaseId.Value)
                                        .Select(kvp => new XElement("Script",
                                            new XAttribute("event", kvp.Key),
                                            new XCData(kvp.Value))))
                                : null
                        )
                    )
                )
            );
            
            return root.ToString();
        }

        private string ExportToCSV()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Type,Name,Caption,Text,X,Y,Width,Height,Visible,Enabled");
            
            foreach (var control in _controls)
            {
                sb.AppendLine($"{control.Type},{control.Name}," +
                    $"\"{control.Caption ?? ""}\",\"{control.Text ?? ""}\"," +
                    $"{control.X},{control.Y},{control.Width},{control.Height}," +
                    $"{control.Visible},{control.Enabled}");
            }
            
            return sb.ToString();
        }

        private string ExportToINI()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Window]");
            sb.AppendLine("Width=800");
            sb.AppendLine("Height=600");
            sb.AppendLine("Title=Exported Form");
            sb.AppendLine();
            
            foreach (var control in _controls)
            {
                sb.AppendLine($"[{control.Name}]");
                sb.AppendLine($"Type={control.Type}");
                if (!string.IsNullOrEmpty(control.Caption))
                    sb.AppendLine($"Caption={control.Caption}");
                if (!string.IsNullOrEmpty(control.Text))
                    sb.AppendLine($"Text={control.Text}");
                sb.AppendLine($"X={control.X:F0}");
                sb.AppendLine($"Y={control.Y:F0}");
                sb.AppendLine($"Width={control.Width:F0}");
                sb.AppendLine($"Height={control.Height:F0}");
                sb.AppendLine($"Visible={control.Visible}");
                sb.AppendLine($"Enabled={control.Enabled}");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private string ExportToTOML()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[window]");
            sb.AppendLine("width = 800");
            sb.AppendLine("height = 600");
            sb.AppendLine("title = \"Exported Form\"");
            sb.AppendLine();
            
            foreach (var control in _controls)
            {
                sb.AppendLine($"[[controls]]");
                sb.AppendLine($"type = \"{control.Type}\"");
                sb.AppendLine($"name = \"{control.Name}\"");
                if (!string.IsNullOrEmpty(control.Caption))
                    sb.AppendLine($"caption = \"{control.Caption}\"");
                if (!string.IsNullOrEmpty(control.Text))
                    sb.AppendLine($"text = \"{control.Text}\"");
                sb.AppendLine($"x = {control.X:F0}");
                sb.AppendLine($"y = {control.Y:F0}");
                sb.AppendLine($"width = {control.Width:F0}");
                sb.AppendLine($"height = {control.Height:F0}");
                sb.AppendLine($"visible = {control.Visible.ToString().ToLower()}");
                sb.AppendLine($"enabled = {control.Enabled.ToString().ToLower()}");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private string ExportToSQL()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Form Export to SQL");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS controls (");
            sb.AppendLine("    id INTEGER PRIMARY KEY,");
            sb.AppendLine("    type TEXT,");
            sb.AppendLine("    name TEXT,");
            sb.AppendLine("    caption TEXT,");
            sb.AppendLine("    text TEXT,");
            sb.AppendLine("    x REAL,");
            sb.AppendLine("    y REAL,");
            sb.AppendLine("    width REAL,");
            sb.AppendLine("    height REAL,");
            sb.AppendLine("    visible INTEGER,");
            sb.AppendLine("    enabled INTEGER");
            sb.AppendLine(");");
            sb.AppendLine();
            
            int id = 1;
            foreach (var control in _controls)
            {
                sb.AppendLine($"INSERT INTO controls VALUES ({id++}, " +
                    $"'{control.Type}', '{control.Name}', " +
                    $"'{control.Caption?.Replace("'", "''")}', " +
                    $"'{control.Text?.Replace("'", "''")}', " +
                    $"{control.X}, {control.Y}, {control.Width}, {control.Height}, " +
                    $"{(control.Visible ? 1 : 0)}, {(control.Enabled ? 1 : 0)});");
            }
            
            sb.AppendLine();
            sb.AppendLine("CREATE TABLE IF NOT EXISTS scripts (");
            sb.AppendLine("    id INTEGER PRIMARY KEY,");
            sb.AppendLine("    control_id INTEGER,");
            sb.AppendLine("    event_name TEXT,");
            sb.AppendLine("    script_text TEXT");
            sb.AppendLine(");");
            sb.AppendLine();
            
            int scriptId = 1;
            id = 1;
            foreach (var control in _controls)
            {
                if (control.DatabaseId.HasValue)
                {
                    var scripts = _database.GetAllScriptsForControl(control.DatabaseId.Value);
                    foreach (var kvp in scripts)
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Value))
                        {
                            sb.AppendLine($"INSERT INTO scripts VALUES ({scriptId++}, {id}, " +
                                $"'{kvp.Key}', '{kvp.Value.Replace("'", "''")}');");
                        }
                    }
                }
                id++;
            }
            
            return sb.ToString();
        }

        private List<DesignerControl> ImportFromYAML(string content)
        {
            var result = new List<DesignerControl>();
            
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                
                var data = deserializer.Deserialize<Dictionary<string, object>>(content);
                
                if (data.ContainsKey("controls") && data["controls"] is List<object> controlsList)
                {
                    foreach (var item in controlsList)
                    {
                        if (item is Dictionary<object, object> controlData)
                        {
                            result.Add(ConvertToDesignerControl(controlData));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ YAML import error: {ex.Message}");
            }
            
            return result;
        }

        private List<DesignerControl> ImportFromJSON(string content)
        {
            var result = new List<DesignerControl>();
            
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
                
                if (data != null && data.ContainsKey("controls"))
                {
                    foreach (var controlJson in data["controls"].EnumerateArray())
                    {
                        var controlData = new Dictionary<object, object>();
                        foreach (var prop in controlJson.EnumerateObject())
                        {
                            controlData[prop.Name] = prop.Value.ValueKind == JsonValueKind.String 
                                ? prop.Value.GetString() 
                                : prop.Value.ToString();
                        }
                        result.Add(ConvertToDesignerControl(controlData));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JSON import error: {ex.Message}");
            }
            
            return result;
        }

        private List<DesignerControl> ImportFromXML(string content)
        {
            var result = new List<DesignerControl>();
            
            try
            {
                var doc = XDocument.Parse(content);
                var controls = doc.Root?.Element("Controls")?.Elements("Control");
                
                if (controls != null)
                {
                    foreach (var control in controls)
                    {
                        var controlData = new Dictionary<object, object>
                        {
                            ["type"] = control.Element("Type")?.Value ?? "label",
                            ["name"] = control.Element("Name")?.Value ?? "control",
                            ["caption"] = control.Element("Caption")?.Value ?? "",
                            ["text"] = control.Element("Text")?.Value ?? "",
                            ["x"] = control.Element("X")?.Value ?? "0",
                            ["y"] = control.Element("Y")?.Value ?? "0",
                            ["width"] = control.Element("Width")?.Value ?? "100",
                            ["height"] = control.Element("Height")?.Value ?? "30"
                        };
                        result.Add(ConvertToDesignerControl(controlData));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ XML import error: {ex.Message}");
            }
            
            return result;
        }

        private List<DesignerControl> ImportFromCSV(string content)
        {
            var result = new List<DesignerControl>();
            
            try
            {
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 2) return result;
                
                for (int i = 1; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 8)
                    {
                        var controlData = new Dictionary<object, object>
                        {
                            ["type"] = parts[0].Trim(),
                            ["name"] = parts[1].Trim(),
                            ["caption"] = parts[2].Trim('"'),
                            ["text"] = parts[3].Trim('"'),
                            ["x"] = parts[4].Trim(),
                            ["y"] = parts[5].Trim(),
                            ["width"] = parts[6].Trim(),
                            ["height"] = parts[7].Trim()
                        };
                        result.Add(ConvertToDesignerControl(controlData));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CSV import error: {ex.Message}");
            }
            
            return result;
        }

        private List<DesignerControl> ImportFromSQL(string content)
        {
            Console.WriteLine("⚠️ SQL import not yet implemented - use database directly");
            return new List<DesignerControl>();
        }

        private DesignerControl ConvertToDesignerControl(Dictionary<object, object> data)
        {
            var type = data.ContainsKey("type") ? data["type"].ToString() : "label";
            var name = data.ContainsKey("name") ? data["name"].ToString() : "control";
            var x = data.ContainsKey("x") ? Convert.ToDouble(data["x"]) : 0;
            var y = data.ContainsKey("y") ? Convert.ToDouble(data["y"]) : 0;
            var width = data.ContainsKey("width") ? Convert.ToDouble(data["width"]) : 100;
            var height = data.ContainsKey("height") ? Convert.ToDouble(data["height"]) : 30;
            
            return new DesignerControl
            {
                Type = type ?? "label",
                Name = name ?? "control",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Caption = data.ContainsKey("caption") ? data["caption"]?.ToString() : null,
                Text = data.ContainsKey("text") ? data["text"]?.ToString() : null,
                Visible = true,
                Enabled = true
            };
        }
    }
}
