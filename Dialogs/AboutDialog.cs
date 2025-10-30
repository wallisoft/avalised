using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalised.Dialogs;

public class AboutDialog : Window
{
    public AboutDialog()
    {
        Title = "🌳 Avalised™ - About";
        Width = 600;
        Height = 500;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var stack = new StackPanel
        {
            Spacing = 12,
            Margin = new Avalonia.Thickness(40)
        };

        // Logo/Title
        stack.Children.Add(new TextBlock
        {
            Text = "🌳 Avalised™",
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#2E7D32"),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Database-Driven RAD IDE",
            FontSize = 18,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, -4, 0, 0)
        });

        // Version
        stack.Children.Add(new TextBlock
        {
            Text = "Version 1.0.0",
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 8, 0, 0)
        });

        // Separator
        stack.Children.Add(new Border
        {
            Height = 1,
            Background = Brushes.LightGray,
            Margin = new Avalonia.Thickness(0, 12, 0, 12)
        });

        // Credits
        stack.Children.Add(new TextBlock
        {
            Text = "Created by:",
            FontWeight = FontWeight.Bold,
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Steve \"Recursion Hurts My Head\" Wallis  &  Claude \"set: paste\" (Anthropic)",
            FontSize = 11,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.DarkSlateGray
        });

        // Separator
        stack.Children.Add(new Border
        {
            Height = 1,
            Background = Brushes.LightGray,
            Margin = new Avalonia.Thickness(0, 12, 0, 12)
        });

        // Foundation info
        stack.Children.Add(new TextBlock
        {
            Text = "A project of The Avalised Foundation",
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Wallisoft • October 2025",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray
        });

        // Tagline - MOVED UP (reduced margin from 12 to 4)
        stack.Children.Add(new TextBlock
        {
            Text = "Building the future of RAD development",
            FontSize = 12,
            FontStyle = FontStyle.Italic,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brush.Parse("#2E7D32"),
            Margin = new Avalonia.Thickness(0, 4, 0, 0)
        });

        // Legal
        stack.Children.Add(new Border
        {
            Height = 1,
            Background = Brushes.LightGray,
            Margin = new Avalonia.Thickness(0, 12, 0, 12)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "UK Patents Applied For",
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Free for personal and educational use\nCommercial licenses available",
            FontSize = 10,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray
        });

        // Close button
        var closeBtn = new Button
        {
            Content = "Close",
            Width = 120,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        closeBtn.Click += (s, e) => Close();
        stack.Children.Add(closeBtn);

        return stack;
    }
}
