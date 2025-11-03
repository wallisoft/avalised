using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;

namespace VB;

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
            var vmlPath = Path.Combine(Directory.GetCurrentDirectory(), "designer.vml");
            var vml = VmlLoader.Load(vmlPath);
            var builder = new ControlBuilder(vml);
            var window = builder.BuildWindow() as MainWindow;
            
            if (window != null)
            {
                var commonStack = builder.GetControl("CommonControlsStack") as StackPanel;
                var inputStack = builder.GetControl("InputControlsStack") as StackPanel;
                var layoutStack = builder.GetControl("LayoutControlsStack") as StackPanel;
                var displayStack = builder.GetControl("DisplayControlsStack") as StackPanel;
                var containerStack = builder.GetControl("ContainerControlsStack") as StackPanel;
                var canvas = builder.GetControl("DesignCanvas") as Canvas;
                var statusControl = builder.GetControl("StatusControl") as TextBlock;
                var statusWindowSize = builder.GetControl("StatusWindowSize") as TextBlock;
                var statusCanvasSize = builder.GetControl("StatusCanvasSize") as TextBlock;
                
                window.InitializeDesigner(commonStack, inputStack, layoutStack, displayStack, containerStack,
                    canvas, statusControl, statusWindowSize, statusCanvasSize);
            }
            
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
