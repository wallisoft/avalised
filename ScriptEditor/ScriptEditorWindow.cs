using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigUI
{
    public class ScriptEditorWindow : Window
    {
        private ScriptDatabase _database;
        private int _controlId;
        private string _controlName;
        private string _controlType;
        
        private ListBox? _eventListBox;
        private TextBox? _scriptEditor;
        private TextBlock? _statusLabel;
        private Button? _saveButton;
        private Button? _cancelButton;
        private Button? _deleteButton;
        
        private Dictionary<string, string> _scripts = new Dictionary<string, string>();
        private string? _selectedEvent = null;

        public ScriptEditorWindow(ScriptDatabase database, int controlId, string controlName, string controlType)
        {
            _database = database;
            _controlId = controlId;
            _controlName = controlName;
            _controlType = controlType;
            
            Width = 800;
            Height = 600;
            Title = $"Script Editor - {controlName} ({controlType})";
            Background = new SolidColorBrush(Color.Parse("#f0f0f0"));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            BuildUI();
            LoadScripts();
        }

        private void BuildUI()
        {
            var mainCanvas = new Canvas
            {
                Width = 800,
                Height = 600,
                Background = new SolidColorBrush(Color.Parse("#f0f0f0"))
            };

            // Header
            var header = new Border
            {
                Width = 800,
                Height = 50,
                Background = new SolidColorBrush(Color.Parse("#2196F3")),
                Child = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"📝 Script Editor",
                            FontSize = 18,
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"{_controlName} ({_controlType})",
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.Parse("#E3F2FD")),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                }
            };
            Canvas.SetLeft(header, 0);
            Canvas.SetTop(header, 0);
            mainCanvas.Children.Add(header);

            // Left Panel - Events List
            var leftPanel = new Border
            {
                Width = 200,
                Height = 490,
                Background = new SolidColorBrush(Color.Parse("#ffffff")),
                BorderBrush = new SolidColorBrush(Color.Parse("#c0c0c0")),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(leftPanel, 10);
            Canvas.SetTop(leftPanel, 60);

            var leftStack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var eventsHeader = new Border
            {
                Width = 200,
                Height = 35,
                Background = new SolidColorBrush(Color.Parse("#e0e0e0")),
                Child = new TextBlock
                {
                    Text = "⚡ Events",
                    FontSize = 13,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            leftStack.Children.Add(eventsHeader);

            _eventListBox = new ListBox
            {
                Width = 200,
                Height = 455,
                Background = new SolidColorBrush(Colors.White)
            };
            _eventListBox.SelectionChanged += OnEventSelected;
            leftStack.Children.Add(_eventListBox);

            leftPanel.Child = leftStack;
            mainCanvas.Children.Add(leftPanel);

            // Right Panel - Script Editor
            var rightPanel = new Border
            {
                Width = 570,
                Height = 490,
                Background = new SolidColorBrush(Color.Parse("#ffffff")),
                BorderBrush = new SolidColorBrush(Color.Parse("#c0c0c0")),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(rightPanel, 220);
            Canvas.SetTop(rightPanel, 60);

            var rightStack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var editorHeader = new Border
            {
                Width = 570,
                Height = 35,
                Background = new SolidColorBrush(Color.Parse("#e0e0e0")),
                Child = new TextBlock
                {
                    Text = "💻 Script (Bash)",
                    FontSize = 13,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            rightStack.Children.Add(editorHeader);

            _scriptEditor = new TextBox
            {
                Width = 570,
                Height = 410,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.NoWrap,
                FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                FontSize = 12,
                Background = new SolidColorBrush(Color.Parse("#1e1e1e")),
                Foreground = new SolidColorBrush(Color.Parse("#d4d4d4")),
                Watermark = "# Write your bash script here...\n# Example:\necho 'Hello World'\n",
                IsEnabled = false
            };
            rightStack.Children.Add(_scriptEditor);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 45,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children =
                {
                    new Button
                    {
                        Content = "💾 Save Script",
                        Width = 130,
                        Height = 35,
                        Margin = new Thickness(5),
                        Background = new SolidColorBrush(Color.Parse("#4CAF50")),
                        Foreground = new SolidColorBrush(Colors.White),
                        FontWeight = FontWeight.Bold
                    },
                    new Button
                    {
                        Content = "🗑️ Delete Script",
                        Width = 130,
                        Height = 35,
                        Margin = new Thickness(5),
                        Background = new SolidColorBrush(Color.Parse("#f44336")),
                        Foreground = new SolidColorBrush(Colors.White),
                        FontWeight = FontWeight.Bold
                    }
                }
            };

            _saveButton = buttonPanel.Children[0] as Button;
            _deleteButton = buttonPanel.Children[1] as Button;

            if (_saveButton != null)
                _saveButton.Click += OnSaveScript;
            if (_deleteButton != null)
                _deleteButton.Click += OnDeleteScript;

            rightStack.Children.Add(buttonPanel);
            rightPanel.Child = rightStack;
            mainCanvas.Children.Add(rightPanel);

            // Bottom Status Bar
            var statusBar = new Border
            {
                Width = 780,
                Height = 30,
                Background = new SolidColorBrush(Color.Parse("#e0e0e0")),
                BorderBrush = new SolidColorBrush(Color.Parse("#c0c0c0")),
                BorderThickness = new Thickness(1, 1, 1, 0)
            };
            Canvas.SetLeft(statusBar, 10);
            Canvas.SetTop(statusBar, 560);

            _statusLabel = new TextBlock
            {
                Text = "Select an event to edit its script",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.Parse("#666666"))
            };
            statusBar.Child = _statusLabel;
            mainCanvas.Children.Add(statusBar);

            // Close Button
            var closeButton = new Button
            {
                Content = "✖️ Close",
                Width = 100,
                Height = 35,
                Background = new SolidColorBrush(Color.Parse("#757575")),
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeight.Bold
            };
            Canvas.SetLeft(closeButton, 690);
            Canvas.SetTop(closeButton, 557);
            closeButton.Click += (s, e) => Close();
            mainCanvas.Children.Add(closeButton);

            this.Content = mainCanvas;
        }

        private void LoadScripts()
        {
            if (_eventListBox == null) return;

            // Get available events for this control type
            var availableEvents = GetEventsForControlType(_controlType);

            // Load existing scripts from database
            _scripts = _database.GetAllScriptsForControl(_controlId);

            // Populate event list
            _eventListBox.Items.Clear();
            foreach (var eventName in availableEvents)
            {
                var hasScript = _scripts.ContainsKey(eventName) && !string.IsNullOrWhiteSpace(_scripts[eventName]);
                var displayText = hasScript ? $"✅ {eventName}" : $"⚪ {eventName}";
                
                var item = new ListBoxItem
                {
                    Content = displayText,
                    Tag = eventName,
                    FontSize = 12
                };
                _eventListBox.Items.Add(item);
            }

            UpdateStatus("Loaded scripts from database");
        }

        private string[] GetEventsForControlType(string type)
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

        private void OnEventSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (_eventListBox?.SelectedItem is ListBoxItem item && _scriptEditor != null)
            {
                _selectedEvent = item.Tag?.ToString();
                
                if (_selectedEvent != null)
                {
                    // Load script text for this event
                    if (_scripts.ContainsKey(_selectedEvent))
                    {
                        _scriptEditor.Text = _scripts[_selectedEvent];
                    }
                    else
                    {
                        _scriptEditor.Text = $"# Script for {_selectedEvent}\n\n";
                    }
                    
                    _scriptEditor.IsEnabled = true;
                    UpdateStatus($"Editing: {_selectedEvent}");
                }
            }
        }

        private void OnSaveScript(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_selectedEvent == null || _scriptEditor == null)
            {
                UpdateStatus("⚠️ No event selected");
                return;
            }

            var scriptText = _scriptEditor.Text ?? "";
            
            try
            {
                // Save to database
                _database.SaveScript(_controlId, _selectedEvent, scriptText);
                
                // Update local cache
                _scripts[_selectedEvent] = scriptText;
                
                // Refresh event list to show checkmark
                LoadScripts();
                
                // Reselect the event
                if (_eventListBox != null)
                {
                    foreach (var item in _eventListBox.Items.Cast<ListBoxItem>())
                    {
                        if (item.Tag?.ToString() == _selectedEvent)
                        {
                            _eventListBox.SelectedItem = item;
                            break;
                        }
                    }
                }
                
                UpdateStatus($"✅ Saved script for {_selectedEvent}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error saving: {ex.Message}");
            }
        }

        private void OnDeleteScript(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_selectedEvent == null)
            {
                UpdateStatus("⚠️ No event selected");
                return;
            }

            try
            {
                _database.DeleteScript(_controlId, _selectedEvent);
                
                if (_scripts.ContainsKey(_selectedEvent))
                    _scripts.Remove(_selectedEvent);
                
                if (_scriptEditor != null)
                    _scriptEditor.Text = $"# Script for {_selectedEvent}\n\n";
                
                LoadScripts();
                
                UpdateStatus($"✅ Deleted script for {_selectedEvent}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error deleting: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = message;
                Console.WriteLine($"📝 ScriptEditor: {message}");
            }
        }
    }
}
