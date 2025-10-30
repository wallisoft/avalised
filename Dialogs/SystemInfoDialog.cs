using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalised.Dialogs;

public class SystemInfoDialog : Window
{
    public SystemInfoDialog()
    {
        Title = "🌳 Avalised™ - System Information";
        Width = 700;
        Height = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var scroll = new ScrollViewer
        {
            Padding = new Avalonia.Thickness(24)
        };

        var stack = new StackPanel
        {
            Spacing = 14
        };

        // Header
        stack.Children.Add(new TextBlock
        {
            Text = "🌳 Avalised™ System Information",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#2E7D32"),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        });

        // Operating System
        AddSection(stack, "Operating System", GetOSInfo());
        AddSection(stack, "Kernel", GetKernelInfo());
        AddSection(stack, "Memory (RAM)", GetMemoryInfo());
        AddSection(stack, "Storage (HDD/SSD)", GetStorageInfo());
        AddSection(stack, "System Uptime", GetUptimeInfo());
        AddSection(stack, "Network (Local)", GetLocalNetworkInfo());
        AddSection(stack, "Network (Internet)", GetInternetInfo());
        AddSection(stack, "Screen", GetScreenInfo());
        AddSection(stack, "Window", $"{Width} × {Height}");
        AddSection(stack, "Canvas", "800 × 600 (Design Surface)");

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

        scroll.Content = stack;
        return scroll;
    }

    private void AddSection(StackPanel stack, string title, string content)
    {
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            FontSize = 15,
            Foreground = Brush.Parse("#2E7D32")
        });

        stack.Children.Add(new TextBlock
        {
            Text = content,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(12, 2, 0, 10),
            FontSize = 13
        });
    }

    private string GetOSInfo()
    {
        return $"{RuntimeInformation.OSDescription}\n" +
               $"Architecture: {RuntimeInformation.OSArchitecture}\n" +
               $"Framework: {RuntimeInformation.FrameworkDescription}";
    }

    private string GetKernelInfo()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "uname",
                        Arguments = "-r",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                var kernel = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return kernel;
            }
            return Environment.OSVersion.Version.ToString();
        }
        catch
        {
            return "Unable to determine kernel version";
        }
    }

    private string GetMemoryInfo()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var memInfo = File.ReadAllLines("/proc/meminfo");
                var totalMem = memInfo.First(l => l.StartsWith("MemTotal:")).Split(':')[1].Trim();
                var availMem = memInfo.First(l => l.StartsWith("MemAvailable:")).Split(':')[1].Trim();
                return $"Total: {totalMem}\nAvailable: {availMem}";
            }
            return $"Working Set: {Environment.WorkingSet / 1024 / 1024} MB";
        }
        catch
        {
            return "Unable to determine memory info";
        }
    }

    private string GetStorageInfo()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => $"{d.Name}: {FormatBytes(d.AvailableFreeSpace)} free of {FormatBytes(d.TotalSize)}")
                .ToArray();
            return string.Join("\n", drives);
        }
        catch
        {
            return "Unable to determine storage info";
        }
    }

    private string GetUptimeInfo()
    {
        try
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
        }
        catch
        {
            return "Unable to determine uptime";
        }
    }

    private string GetLocalNetworkInfo()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && 
                           n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(n =>
                {
                    var ipv4 = n.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    return $"{n.Name}: {(ipv4?.Address.ToString() ?? "No IPv4")}";
                })
                .ToArray();
            return string.Join("\n", interfaces);
        }
        catch
        {
            return "Unable to determine local network info";
        }
    }

    private string GetInternetInfo()
    {
        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var response = client.GetAsync("https://www.google.com").Result;
            if (response.IsSuccessStatusCode)
                return "✓ Connected";
            return "✗ No connection";
        }
        catch
        {
            return "✗ No connection";
        }
    }

    private string GetScreenInfo()
    {
        try
        {
            var screens = Screens;
            if (screens != null && screens.All.Count > 0)
            {
                var primary = screens.Primary ?? screens.All[0];
                var bounds = primary.Bounds;
                var workArea = primary.WorkingArea;
                return $"Resolution: {bounds.Width} × {bounds.Height}\n" +
                       $"Working Area: {workArea.Width} × {workArea.Height}\n" +
                       $"Scaling: {primary.Scaling * 100:F0}%";
            }
            return "Unable to determine screen info";
        }
        catch
        {
            return "Unable to determine screen info";
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
