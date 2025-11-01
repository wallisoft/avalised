using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Data.Sqlite;

namespace Avalised
{
    /// <summary>
    /// Executes soft-coded actions defined in AVML
    /// Reads action definitions from database and executes accordingly
    /// </summary>
    public class ActionExecutor
    {
        private readonly string _dbPath;
        private readonly MainWindow _mainWindow;
        private DesignerLayout? _designerLayout;

        public ActionExecutor(string dbPath, MainWindow mainWindow)
        {
            _dbPath = dbPath;
            _mainWindow = mainWindow;
        }

        public void SetDesignerLayout(DesignerLayout designerLayout)
        {
            _designerLayout = designerLayout;
        }

        /// <summary>
        /// Execute an action by name with parameters
        /// </summary>
        public async Task<object?> ExecuteAsync(string actionName, Dictionary<string, string> parameters)
        {
            // Validate action exists
            if (!ValidateAction(actionName))
            {
                Console.WriteLine($"❌ Unknown action: {actionName}");
                return null;
            }

            Console.WriteLine($"🎬 Executing action: {actionName}");

            try
            {
                return actionName switch
                {
                    // Dialog actions
                    "dialog.info" => await ExecuteDialogInfo(parameters),
                    "dialog.warning" => await ExecuteDialogWarning(parameters),
                    "dialog.error" => await ExecuteDialogError(parameters),
                    "dialog.confirm" => await ExecuteDialogConfirm(parameters),
                    "dialog.input" => await ExecuteDialogInput(parameters),
                    "dialog.open" => await ExecuteDialogOpen(parameters),
                    "dialog.save" => await ExecuteDialogSave(parameters),
                    "dialog.folder" => await ExecuteDialogFolder(parameters),

                    // File actions
                    "file.new" => await ExecuteFileNew(parameters),
                    "file.open" => await ExecuteFileOpen(parameters),
                    "file.save" => await ExecuteFileSave(parameters),
                    "file.reload" => await ExecuteFileReload(parameters),
                    "file.export" => await ExecuteFileExport(parameters),
                    "file.exit" => await ExecuteFileExit(parameters),

                    // Status actions
                    "status.update" => ExecuteStatusUpdate(parameters),
                    "status.clear" => ExecuteStatusClear(parameters),

                    // App actions
                    "app.about" => await ExecuteAppAbout(parameters),
                    "app.help" => await ExecuteAppHelp(parameters),
                    "app.options" => await ExecuteAppOptions(parameters),

                    // Canvas actions
                    "canvas.addcontrol" => await ExecuteCanvasAddControl(parameters),
                    // Panel actions
                    "panel.toggle" => ExecutePanelToggle(parameters),

                    _ => throw new NotImplementedException($"Action '{actionName}' not implemented yet")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Action execution failed: {ex.Message}");
                await DialogService.ShowError("Action Error", $"Failed to execute {actionName}:\n{ex.Message}");
                return null;
            }
        }

        private bool ValidateAction(string actionName)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM actions WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", actionName);
            
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        // ========== DIALOG ACTIONS ==========

        private async Task<object?> ExecuteDialogInfo(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Information");
            var message = parameters.GetValueOrDefault("message", "");
            await DialogService.ShowInfo(title, message);
            return null;
        }

        private async Task<object?> ExecuteDialogWarning(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Warning");
            var message = parameters.GetValueOrDefault("message", "");
            await DialogService.ShowWarning(title, message);
            return null;
        }

        private async Task<object?> ExecuteDialogError(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Error");
            var message = parameters.GetValueOrDefault("message", "");
            await DialogService.ShowError(title, message);
            return null;
        }

        private async Task<object?> ExecuteDialogConfirm(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Confirm");
            var message = parameters.GetValueOrDefault("message", "Are you sure?");
            var result = await DialogService.ShowConfirm(title, message);
            
            // Execute follow-up actions if defined
            if (result && parameters.ContainsKey("on_yes"))
            {
                await ExecuteAsync(parameters["on_yes"], new Dictionary<string, string>());
            }
            else if (!result && parameters.ContainsKey("on_no"))
            {
                await ExecuteAsync(parameters["on_no"], new Dictionary<string, string>());
            }
            
            return result;
        }

        private async Task<object?> ExecuteDialogInput(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Input");
            var message = parameters.GetValueOrDefault("message", "Enter value:");
            var defaultValue = parameters.GetValueOrDefault("default", "");
            var result = await DialogService.ShowInput(title, message, defaultValue);
            
            if (result != null)
            {
                _designerLayout?.UpdateStatus($"Input: {result}", false);
            }
            
            return result;
        }

        private async Task<object?> ExecuteDialogOpen(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Open File");
            var filter = parameters.GetValueOrDefault("filter", "*.*");
            var directory = parameters.GetValueOrDefault("directory");
            
            var result = await DialogService.ShowOpenFile(title, directory, filter);
            
            if (result != null)
            {
                _designerLayout?.UpdateStatus($"Selected: {result}", false);
                
                // Execute target action if defined
                if (parameters.ContainsKey("target"))
                {
                    var targetParams = new Dictionary<string, string> { ["file"] = result };
                    await ExecuteAsync(parameters["target"], targetParams);
                }
            }
            
            return result;
        }

        private async Task<object?> ExecuteDialogSave(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Save File");
            var extension = parameters.GetValueOrDefault("extension", "txt");
            var directory = parameters.GetValueOrDefault("directory");
            
            var result = await DialogService.ShowSaveFile(title, directory, extension);
            
            if (result != null)
            {
                _designerLayout?.UpdateStatus($"Save to: {result}", false);
            }
            
            return result;
        }

        private async Task<object?> ExecuteDialogFolder(Dictionary<string, string> parameters)
        {
            var title = parameters.GetValueOrDefault("title", "Select Folder");
            var directory = parameters.GetValueOrDefault("directory");
            
            var result = await DialogService.ShowSelectFolder(title, directory);
            
            if (result != null)
            {
                _designerLayout?.UpdateStatus($"Selected folder: {result}", false);
            }
            
            return result;
        }

        // ========== FILE ACTIONS ==========

        private async Task<object?> ExecuteFileNew(Dictionary<string, string> parameters)
        {
            _designerLayout?.UpdateStatus("Creating new form...", false);
            return null;
        }

        private async Task<object?> ExecuteFileOpen(Dictionary<string, string> parameters)
        {
            var file = await DialogService.ShowOpenFile("Open AVML File", null, "AVML Files (*.avml)|*.avml|All Files (*.*)|*.*");
            if (file != null)
            {
                _designerLayout?.UpdateStatus($"Opening: {file}", false);
            }
            return file;
        }

        private async Task<object?> ExecuteFileSave(Dictionary<string, string> parameters)
        {
            var file = await DialogService.ShowSaveFile("Save AVML File", null, "avml");
            if (file != null)
            {
                _designerLayout?.UpdateStatus($"Saving: {file}", false);
            }
            return file;
        }

        private async Task<object?> ExecuteFileReload(Dictionary<string, string> parameters)
        {
            _designerLayout?.UpdateStatus("Reloading form...", false);
            return null;
        }

        private async Task<object?> ExecuteFileExport(Dictionary<string, string> parameters)
        {
            _designerLayout?.UpdateStatus("Exporting to AVML...", false);
            return null;
        }

        private async Task<object?> ExecuteFileExit(Dictionary<string, string> parameters)
        {
            _mainWindow.Close();
            return null;
        }

        // ========== STATUS ACTIONS ==========

        private object? ExecuteStatusUpdate(Dictionary<string, string> parameters)
        {
            var message = parameters.GetValueOrDefault("message", "");
            _designerLayout?.UpdateStatus(message, false);
            return null;
        }

        private object? ExecuteStatusClear(Dictionary<string, string> parameters)
        {
            _designerLayout?.UpdateStatus("", false);
            return null;
        }

        // ========== APP ACTIONS ==========

        private async Task<object?> ExecuteAppAbout(Dictionary<string, string> parameters)
        {
            await DialogService.ShowInfo("About Avalised", 
                "Avalised Designer 1.0\n" +
                "YAML-driven RAD IDE\n\n" +
                "© 2024 Steve Wallis\n" +
                "Built with Claude (Anthropic)");
            return null;
        }

        private async Task<object?> ExecuteAppHelp(Dictionary<string, string> parameters)
        {
            await DialogService.ShowInfo("Help", "Help system coming soon!");
            return null;
        }

        private async Task<object?> ExecuteAppOptions(Dictionary<string, string> parameters)
        {
            await DialogService.ShowInfo("Options", "Options dialog coming soon!");
            return null;
        }

        // ========== CANVAS ACTIONS ==========
        
        private async Task<object?> ExecuteCanvasAddControl(Dictionary<string, string> parameters)
        {
            var type = parameters.GetValueOrDefault("type", "Button");
            var x = double.Parse(parameters.GetValueOrDefault("x", "100"));
            var y = double.Parse(parameters.GetValueOrDefault("y", "100"));
            
            _designerLayout?.AddControlToCanvas(type, x, y, parameters);
            
            return await Task.FromResult<object?>(null);
        }
        private object? ExecutePanelToggle(Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("panel", out var panelName)) return null;
            var containerName = panelName + "Container";
            Console.WriteLine($"🔍 Searching for: {containerName}");
            var container = _designerLayout?.FindControlByName(_mainWindow.Content as Control, containerName);
            if (container != null) container.IsVisible = !container.IsVisible;
            else Console.WriteLine($"❌ Not found: {containerName}");
                Console.WriteLine($"✅ Toggled {containerName}: IsVisible={container.IsVisible}");
            return null;
        }

    }
}
