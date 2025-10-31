using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

namespace Avalised
{
    /// <summary>
    /// Generic dialog handler for system and user-defined dialogs
    /// Provides unified interface to Avalonia dialogs callable from scripts
    /// </summary>
    public static class DialogService
    {
        private static Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }

        /// <summary>
        /// Show information dialog
        /// </summary>
        public static async Task ShowInfo(string title, string message)
        {
            var window = GetMainWindow();
            if (window == null) return;

            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20
            };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });

            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Width = 100
            };
            button.Click += (s, e) => dialog.Close();

            panel.Children.Add(button);
            dialog.Content = panel;

            await dialog.ShowDialog(window);
        }

        /// <summary>
        /// Show warning dialog
        /// </summary>
        public static async Task ShowWarning(string title, string message)
        {
            await ShowInfo($"⚠️ {title}", message);
        }

        /// <summary>
        /// Show error dialog
        /// </summary>
        public static async Task ShowError(string title, string message)
        {
            await ShowInfo($"❌ {title}", message);
        }

        /// <summary>
        /// Show confirmation dialog - returns true if Yes/OK clicked
        /// </summary>
        public static async Task<bool> ShowConfirm(string title, string message)
        {
            var window = GetMainWindow();
            if (window == null) return false;

            var result = false;
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20
            };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };

            var yesButton = new Button
            {
                Content = "Yes",
                Width = 80
            };
            yesButton.Click += (s, e) =>
            {
                result = true;
                dialog.Close();
            };

            var noButton = new Button
            {
                Content = "No",
                Width = 80
            };
            noButton.Click += (s, e) =>
            {
                result = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;
            await dialog.ShowDialog(window);

            return result;
        }

        /// <summary>
        /// Show input dialog - returns entered text or null if cancelled
        /// </summary>
        public static async Task<string?> ShowInput(string title, string message, string defaultValue = "")
        {
            var window = GetMainWindow();
            if (window == null) return null;

            string? result = null;
            var dialog = new Window
            {
                Title = title,
                Width = 450,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15
            };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });

            var textBox = new TextBox
            {
                Text = defaultValue,
                Watermark = "Enter value..."
            };
            panel.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80
            };
            okButton.Click += (s, e) =>
            {
                result = textBox.Text;
                dialog.Close();
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80
            };
            cancelButton.Click += (s, e) =>
            {
                result = null;
                dialog.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;
            await dialog.ShowDialog(window);

            return result;
        }

        /// <summary>
        /// Show file open dialog
        /// </summary>
        public static async Task<string?> ShowOpenFile(string title = "Open File", string? initialDirectory = null, string filter = "All Files (*.*)|*.*")
        {
            var window = GetMainWindow();
            if (window == null) return null;

            var dialog = new OpenFileDialog
            {
                Title = title,
                Directory = initialDirectory,
                AllowMultiple = false
            };

            var result = await dialog.ShowAsync(window);
            return result?.Length > 0 ? result[0] : null;
        }

        /// <summary>
        /// Show file save dialog
        /// </summary>
        public static async Task<string?> ShowSaveFile(string title = "Save File", string? initialDirectory = null, string defaultExtension = "txt")
        {
            var window = GetMainWindow();
            if (window == null) return null;

            var dialog = new SaveFileDialog
            {
                Title = title,
                Directory = initialDirectory,
                DefaultExtension = defaultExtension
            };

            return await dialog.ShowAsync(window);
        }

        /// <summary>
        /// Show folder selection dialog
        /// </summary>
        public static async Task<string?> ShowSelectFolder(string title = "Select Folder", string? initialDirectory = null)
        {
            var window = GetMainWindow();
            if (window == null) return null;

            var dialog = new OpenFolderDialog
            {
                Title = title,
                Directory = initialDirectory
            };

            return await dialog.ShowAsync(window);
        }
    }
}
