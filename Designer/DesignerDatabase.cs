using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigUI.Designer
{
    public class DesignerDatabase
    {
        private ScriptDatabase _database;
        private List<DesignerControl> _controls;

        public DesignerDatabase(ScriptDatabase database, List<DesignerControl> controls)
        {
            _database = database;
            _controls = controls;
        }

        public void LoadFromDatabaseOrYaml()
        {
            var dbControls = _database.GetAllControls();
            
            if (dbControls.Count == 0)
            {
                Console.WriteLine("🔄 First run - importing visual-designer.yaml to SQLite...");
                ImportFromYaml();
            }
            else
            {
                Console.WriteLine("📂 Loading designer from SQLite database...");
                LoadFromDatabase(dbControls);
            }
        }

        public void ImportFromYaml()
        {
            var yamlPath = DesignerHelpers.FindYamlFile("visual-designer.yaml");
            if (yamlPath == null)
            {
                Console.WriteLine("❌ visual-designer.yaml not found!");
                return;
            }
            
            try
            {
                var yamlContent = File.ReadAllText(yamlPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                
                var data = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);
                
                if (data.ContainsKey("controls") && data["controls"] is List<object> controlsList)
                {
                    foreach (var item in controlsList)
                    {
                        if (item is Dictionary<object, object> controlData)
                        {
                            SaveControlToDatabase(controlData, null);
                        }
                    }
                }
                
                Console.WriteLine("✅ Imported visual-designer.yaml to SQLite");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error importing YAML: {ex.Message}");
            }
        }

        private int SaveControlToDatabase(Dictionary<object, object> controlData, int? parentId)
        {
            var type = controlData.ContainsKey("type") ? controlData["type"].ToString() : "label";
            var name = controlData.ContainsKey("name") ? controlData["name"].ToString() : "control";
            var x = controlData.ContainsKey("x") ? Convert.ToDouble(controlData["x"]) : 0;
            var y = controlData.ContainsKey("y") ? Convert.ToDouble(controlData["y"]) : 0;
            var width = controlData.ContainsKey("width") ? Convert.ToDouble(controlData["width"]) : 100;
            var height = controlData.ContainsKey("height") ? Convert.ToDouble(controlData["height"]) : 30;
            var caption = controlData.ContainsKey("caption") ? controlData["caption"]?.ToString() : null;
            var text = controlData.ContainsKey("text") ? controlData["text"]?.ToString() : null;
            var visible = controlData.ContainsKey("visible") ? Convert.ToBoolean(controlData["visible"]) : true;
            var enabled = controlData.ContainsKey("enabled") ? Convert.ToBoolean(controlData["enabled"]) : true;
            
            var controlId = _database.SaveControl(
                type ?? "label",
                name ?? "control",
                parentId,
                x, y, width, height,
                caption, text,
                visible, enabled
            );
            
            // Save additional properties (including font_size!)
            foreach (var kvp in controlData)
            {
                var propName = kvp.Key.ToString();
                if (propName != null && !new[] { "type", "name", "x", "y", "width", "height", "caption", "text", "visible", "enabled", "controls", "items" }.Contains(propName))
                {
                    _database.SaveProperty(controlId, propName, kvp.Value?.ToString() ?? "");
                }
            }
            
            // ✨ RECURSIVELY IMPORT NESTED CONTROLS ✨
            if (controlData.ContainsKey("controls") && controlData["controls"] is List<object> nestedControls)
            {
                foreach (var nestedItem in nestedControls)
                {
                    if (nestedItem is Dictionary<object, object> nestedData)
                    {
                        SaveControlToDatabase(nestedData, controlId);
                    }
                }
            }
            
            return controlId;
        }

        private void LoadFromDatabase(List<Dictionary<string, object>> dbControls)
        {
            Console.WriteLine($"📊 Found {dbControls.Count} controls in database");
        }

        public void SaveAllToDatabase()
        {
            foreach (var control in _controls)
            {
                if (!control.DatabaseId.HasValue)
                {
                    control.DatabaseId = _database.SaveControl(
                        control.Type, control.Name, null,
                        control.X, control.Y, control.Width, control.Height,
                        control.Caption, control.Text, control.Visible, control.Enabled
                    );
                }
                else
                {
                    _database.UpdateControl(
                        control.DatabaseId.Value,
                        control.X, control.Y, control.Width, control.Height,
                        control.Caption, control.Text, control.Visible, control.Enabled
                    );
                }
            }
            
            Console.WriteLine($"✅ Saved {_controls.Count} controls to database");
        }

        public List<DesignerControl> LoadAllFromDatabase()
        {
            var controls = _database.GetAllControls();
            var result = new List<DesignerControl>();
            
            foreach (var controlData in controls)
            {
                var control = new DesignerControl
                {
                    DatabaseId = Convert.ToInt32(controlData["id"]),
                    Type = controlData["type"]?.ToString() ?? "label",
                    Name = controlData["name"]?.ToString() ?? "control",
                    X = Convert.ToDouble(controlData["x"]),
                    Y = Convert.ToDouble(controlData["y"]),
                    Width = Convert.ToDouble(controlData["width"]),
                    Height = Convert.ToDouble(controlData["height"]),
                    Caption = controlData["caption"]?.ToString(),
                    Text = controlData["text"]?.ToString(),
                    Visible = Convert.ToInt32(controlData["visible"]) == 1,
                    Enabled = Convert.ToInt32(controlData["enabled"]) == 1
                };
                
                result.Add(control);
            }
            
            Console.WriteLine($"✅ Loaded {controls.Count} controls from database");
            return result;
        }

        public int SaveControl(DesignerControl control)
        {
            if (control.DatabaseId.HasValue)
            {
                _database.UpdateControl(
                    control.DatabaseId.Value,
                    control.X, control.Y, control.Width, control.Height,
                    control.Caption, control.Text, control.Visible, control.Enabled
                );
                return control.DatabaseId.Value;
            }
            else
            {
                var id = _database.SaveControl(
                    control.Type, control.Name, null,
                    control.X, control.Y, control.Width, control.Height,
                    control.Caption, control.Text, control.Visible, control.Enabled
                );
                control.DatabaseId = id;
                Console.WriteLine($"💾 Saved control to database with ID: {id}");
                return id;
            }
        }

        public void DeleteControl(DesignerControl control)
        {
            if (control.DatabaseId.HasValue)
            {
                _database.DeleteControl(control.DatabaseId.Value);
                Console.WriteLine($"🗑️ Deleted control from database: {control.Name}");
            }
        }

        public void ResetDatabase()
        {
            _database.ClearAllData();
            Console.WriteLine("✅ Database reset - all controls cleared");
        }
    }
}
