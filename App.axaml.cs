using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;

namespace ConfigUI
{
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
                var args = desktop.Args;
                string? yamlPath = null;

                if (args != null && args.Length > 0)
                {
                    if (args[0] == "--designer")
                    {
                        desktop.MainWindow = new DesignerWindow();
                    }
                    else
                    {
                        yamlPath = args[0];
                        
                        // If loading the designer YAML, use DesignerWindow for full functionality
                        var fileName = Path.GetFileName(yamlPath);
                        if (fileName == "visual-designer.yaml" || fileName == "designer.yaml")
                        {
                            desktop.MainWindow = new DesignerWindow();
                        }
                        else
                        {
                            desktop.MainWindow = new MainWindow(yamlPath);
                        }
                    }
                }
                else
                {
                    // No args = load splash screen
                    desktop.MainWindow = new MainWindow(yamlPath);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
