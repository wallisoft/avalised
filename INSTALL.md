# Visualised Markup - Installation Guide

## Requirements

**Visualised Markup requires .NET 9.0 SDK**

Yes, we're on the bleeding edge! This is a modern, revolutionary RAD IDE that leverages the latest .NET capabilities.

## Installing .NET 9.0 SDK

### Linux/macOS (Recommended Method)
```bash
# Download the official installer
wget https://dot.net/v1/dotnet-install.sh

# Make it executable
chmod +x dotnet-install.sh

# Install .NET 9.0 SDK
sudo ./dotnet-install.sh --channel 9.0 --install-dir /usr/lib/dotnet
```

### Alternative: Package Manager (Ubuntu/Debian)
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 9.0 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0
```

### Windows
Download and install from: https://dotnet.microsoft.com/download/dotnet/9.0

### Verify Installation
```bash
dotnet --list-sdks
# Should show: 9.0.xxx [path]
```

## Building & Running

Once .NET 9.0 SDK is installed:

```bash
# Clone the repository
git clone https://github.com/wallisoft/avalised.git
cd avalised

# Build
dotnet build

# Run
dotnet run
```

## Why .NET 9.0?

Modern tools require modern runtimes. Just like:
- Node.js projects specify Node 18+
- Python projects specify Python 3.11+
- PHP projects specify PHP 8.2+

Visualised Markup specifies .NET 9.0 for access to the latest performance improvements and language features.

**No shame in requiring modern tooling for a revolutionary IDE!** 🚀

## Troubleshooting

**Error: "The current .NET SDK does not support targeting .NET 9.0"**
- Solution: Install .NET 9.0 SDK using the instructions above

**Multiple .NET versions installed?**
- That's fine! .NET SDKs coexist happily. The project will automatically use .NET 9.0.

## Support

For issues or questions, please visit: https://github.com/wallisoft/avalised/issues
