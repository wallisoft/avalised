# 🌳 Avalised™ - The Visual Designer That Designs Itself

[![License](https://img.shields.io/badge/license-TBD-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.x-red.svg)](https://avaloniaui.net/)

> **Revolutionary YAML-driven RAD IDE with recursive self-loading architecture**

## 🚀 What Makes Avalised Unique?

**Avalised is a visual designer that can load, modify, and export its own UI definition.**

Unlike traditional IDEs where the designer is separate from what it creates, Avalised uses the **same technology** to build itself that it uses to build your applications. The designer's UI is defined in YAML and loaded at runtime - meaning it can modify itself!

```yaml
# This is what visual-designer.avml looks like
Window: DesignerWindow
  Title: Avalised Designer
  Width: 1280
  Height: 720
  
  Children:
    - DockPanel: MainLayout
      Children:
        - StackPanel: Toolbox
          # The toolbox that creates controls
          # is itself defined as controls!
```

**This is meta-engineering at its finest.**

## ✨ Key Features

- 🔄 **Recursive Architecture** - The designer loads itself from YAML
- 📝 **Human-Readable Format** - AVML (Avalised Markup Language) is just YAML
- 🎨 **Visual RAD Designer** - Drag, drop, resize - like VB5, but modern
- 💾 **Round-Trip Editing** - YAML → Visual → YAML → Visual...
- 🖥️ **Cross-Platform** - Windows, Linux, macOS (via Avalonia)
- ⚡ **Fast** - Loads typical forms in <100ms
- 🧩 **Language-Agnostic** - AVML can target any UI framework

## 🎯 The Vision

> "The most downloaded new RAD IDE in history"

Avalised brings back the **simplicity of VB5** with modern cross-platform power. No more XML hell, no more proprietary formats - just clean, readable YAML.

## 📦 Quick Start

### Prerequisites
- .NET 8.0 or later
- That's it!

### Build & Run

```bash
git clone https://github.com/wallisoft/avalised.git
cd avalised
dotnet build
dotnet run
```

### Your First Form

Create `hello.avml`:

```yaml
Window: HelloWindow
  Title: Hello AVML!
  Width: 400
  Height: 300
  
  Children:
    - StackPanel: MainStack
      Orientation: Vertical
      Spacing: 10
      Margin: 20
      
      Children:
        - TextBlock: WelcomeText
          Text: Welcome to Avalised!
          FontSize: 18
          FontWeight: Bold
          
        - Button: ClickMeButton
          Content: Click Me!
          Width: 150
          Height: 35
```

Load it in Avalised and start designing!

## 🏗️ Architecture

```
AVML (YAML) → AVMLLoader → Avalonia UI → Edit → AVMLExporter → AVML (YAML)
                                ↓
                         The designer itself!
```

**Key Components:**

- **AVMLLoader.cs** - Recursive YAML → UI loader (13KB)
- **AVMLExporter.cs** - UI → YAML exporter (9KB)
- **DesignerLayout.cs** - The visual designer (40KB)
- **MainWindow.cs** - Application entry point

## 📚 Documentation

- [Getting Started](docs/getting-started.md) *(coming soon)*
- [AVML Format Reference](docs/avml-format.md) *(coming soon)*
- [Architecture Deep Dive](docs/architecture.md) *(coming soon)*
- [Contributing Guide](CONTRIBUTING.md) *(coming soon)*

## 🎨 Screenshots

*(Screenshots coming with V1.0 release)*

## 🧪 Current Status

**Version:** 0.9.5 (approaching V1.0)

**Working:**
- ✅ AVML loading and parsing
- ✅ Visual designer with drag/drop/resize
- ✅ Property editing
- ✅ YAML export
- ✅ Cross-platform support

**In Progress:**
- 🔨 visual-designer.avml (designer self-definition)
- 🔨 Full recursive self-loading integration

**See [PROJECT-STATUS.yaml](PROJECT-STATUS.yaml) for detailed status**

## 🔧 Technology Stack

- **UI Framework:** [Avalonia](https://avaloniaui.net/) - Cross-platform XAML-based UI
- **Language:** C# (.NET 8.0)
- **YAML Parser:** [YamlDotNet](https://github.com/aaubry/YamlDotNet)
- **Database:** SQLite (optional storage)

## 📜 Patent & Innovation

Avalised is covered by a **UK Patent Application** with 20+ claims covering:
- Recursive self-loading visual designer architecture
- YAML-driven capability-based UI definitions
- Language-agnostic markup system
- Export-import round-tripping

## 🤝 Contributing

We welcome contributions! Whether it's:
- 🐛 Bug reports
- 💡 Feature suggestions
- 📝 Documentation improvements
- 🔧 Code contributions

Please see [CONTRIBUTING.md](CONTRIBUTING.md) *(coming soon)* for guidelines.

## 👨‍💻 Author

**Steve Wallis** (Wallisoft)
- Former VB5 developer (1985-2015)
- Returned to programming full-time after 10-year hiatus
- Based in Eastbourne, UK
- Built with assistance from Claude (Anthropic)

## 🙏 Acknowledgments

- **Claude (Anthropic)** - AI pair programming partner
- **Avalonia Team** - For the amazing cross-platform UI framework
- **YamlDotNet** - For robust YAML parsing
- **The VB5 Community** - For inspiring simplicity

## 📄 License

*License TBD - will be open source*

## 🔗 Links

- **GitHub:** https://github.com/wallisoft/avalised
- **Wiki:** *(coming soon)*
- **Website:** *(coming soon)*

---

## 💬 Philosophy

**"Why can't modern development be as simple as VB5 was?"**

That question sparked Avalised. We've recreated that drag-and-drop simplicity with modern, cross-platform technology. No complex build systems, no proprietary formats - just clean YAML and powerful visuals.

**The cleverness:** The designer uses the same technology it creates. It's turtles all the way down, and it's beautiful.

---

**Made with ❤️ in Eastbourne, UK**

*"recursion hurts my head" - Steve*  
*"set: paste" - Claude*
