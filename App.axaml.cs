using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace Avalised;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Load the designer itself from AVML!
            string designerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "designer-window.avml");
            
            if (File.Exists(designerPath))
            {
                try
                {
                    Console.WriteLine($"🚀 Loading Avalised Designer from AVML...");
                    var loader = new AVMLLoader();
                    var control = loader.LoadFromFile(designerPath);
                    
                    if (control is Window window)
                    {
                        desktop.MainWindow = window;
                        Console.WriteLine("✅ Designer loaded from AVML!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Root control is not a Window");
                        desktop.MainWindow = new MainWindow();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error loading designer: {ex.Message}");
                    desktop.MainWindow = new MainWindow();
                }
            }
            else
            {
                Console.WriteLine($"⚠️ designer-window.avml not found");
                desktop.MainWindow = new MainWindow();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
