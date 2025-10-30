using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Interactivity;

namespace Avalised;

public class DetachablePanel : Border
{
    private readonly string _title;
    private readonly Func<Control> _contentFactory;
    private DockPanel? _mainContainer;
    private ScrollViewer? _scrollViewer;
    private Button? _detachButton;
    private Window? _detachedWindow;
    private bool _isDetached = false;
    
    public event EventHandler? Closed;
    
    public DetachablePanel(string title, Func<Control> contentFactory)
    {
        _title = title;
        _contentFactory = contentFactory;
        
        BorderBrush = Brushes.Black;
        BorderThickness = new Thickness(1);
        Background = Brushes.White;
        
        Child = CreatePanel();
    }
    
    private Control CreatePanel()
    {
        _mainContainer = new DockPanel { LastChildFill = true };
        
        // Title bar
        var titleBar = new Border
        {
            Background = Brush.Parse("#C8E6C9"),
            Height = 28,
            Child = CreateTitleBar()
        };
        DockPanel.SetDock(titleBar, Dock.Top);
        _mainContainer.Children.Add(titleBar);
        
        // Content with scroll
        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Padding = new Thickness(8),
            Content = _contentFactory()
        };
        _mainContainer.Children.Add(_scrollViewer);
        
        return _mainContainer;
    }
    
    private Control CreateTitleBar()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto")
        };
        
        grid.Children.Add(new TextBlock
        {
            Text = _title,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
            FontWeight = FontWeight.Bold
        });
        
        _detachButton = new Button
        {
            Content = "📌",
            Width = 24,
            Height = 24,
            Padding = new Thickness(0),
            Margin = new Thickness(2, 2, 0, 2)
        };
        _detachButton.Click += OnDetachClick;
        Grid.SetColumn(_detachButton, 1);
        grid.Children.Add(_detachButton);
        
        var closeButton = new Button
        {
            Content = "✕",
            Width = 24,
            Height = 24,
            Padding = new Thickness(0),
            Margin = new Thickness(2)
        };
        closeButton.Click += OnCloseClick;
        Grid.SetColumn(closeButton, 2);
        grid.Children.Add(closeButton);
        
        return grid;
    }
    
    private void OnDetachClick(object? sender, RoutedEventArgs e)
    {
        if (_isDetached)
            Reattach();
        else
            Detach();
    }
    
    private void Detach()
    {
        if (_isDetached) return;
        
        _isDetached = true;
        IsVisible = false;
        
        _detachedWindow = new Window
        {
            Title = $"🌳 Avalised™ - {_title}",
            Width = Width > 0 ? Width : 300,
            Height = 600,
            Content = CreatePanel() // Creates new content
        };
        
        _detachedWindow.Closing += (s, e) => Reattach();
        
        if (_detachButton != null)
            _detachButton.Content = "📍";
        
        _detachedWindow.Show();
    }
    
    private void Reattach()
    {
        if (!_isDetached) return;
        
        _isDetached = false;
        IsVisible = true;
        
        if (_detachButton != null)
            _detachButton.Content = "📌";
        
        _detachedWindow?.Close();
        _detachedWindow = null;
    }
    
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        if (_isDetached)
        {
            _detachedWindow?.Close();
            _detachedWindow = null;
            _isDetached = false;
        }
        
        IsVisible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
