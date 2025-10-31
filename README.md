# 🌳 Avalised™ - The Self-Designing RAD IDE

**A revolutionary YAML-driven visual designer that builds itself**

Built by Steve Wallis & Claude (Anthropic) | October 2025

---

## 🚀 What Is This?

Avalised is a **Rapid Application Development (RAD) IDE** with a radical difference: **it designs itself**.

The entire user interface—menus, toolbox, properties panel, canvas—is defined in a single YAML file (`designer-window.avml`). Change the YAML, run the parser, and the designer rebuilds itself with your changes.

**No code recompilation needed. No UI frameworks. Just pure AVML (Avalised Visual Markup Language).**

This is **meta-engineering**: a designer that recursively designs itself.

---

## ✨ Core Innovation: Soft-Coded Actions

Traditional RAD tools hardcode every menu click, button press, and event handler. **Avalised doesn't.**

Menu actions are defined entirely in AVML:

```yaml
- MenuItem: DemoDialogInfo
  Header: Info Dialog (Soft-Coded)
  Action: dialog.info
  ActionParams: title=Hello;message=No hardcoded handlers!
```

That's it. The `ActionExecutor` reads the action name and parameters from the database and executes accordingly. **No C# event handlers. No XAML bindings. Just YAML.**

**17 menu actions wired automatically on startup.**

---

## 🏗️ Architecture

Avalised is a 3-component system:

### 1. **AVML Parser** (C# .NET)
- Reads `designer-window.avml` (YAML)
- Fuzzy-matches control types (auto-corrects `MenuItm` → `MenuItem`)
- Generates SQLite database with UI tree + properties

**Location:** `~/Downloads/avalised-parser/`

### 2. **Renderer** (Avalonia C#)
- Reads SQLite database
- Recursively builds live Avalonia UI controls
- Wires up soft-coded actions via `ActionExecutor`

**Location:** `~/Downloads/avalised/`

### 3. **Runtime Database**
- SQLite database at `~/.config/Avalised/designer.db`
- Contains UI tree, properties, attached properties, and action definitions

---

## 🎨 Current Features (V1.0)

✅ **Drag & Drop Designer**
- Click toolbox button (Button, TextBox, Label, etc.)
- Click canvas to place control
- Properties panel updates automatically

✅ **Soft-Coded Menu System**
- File, Edit, View, Tools, Demo, Help menus
- 17 actions defined in AVML, zero hardcoded handlers
- Info dialogs, file pickers, all soft-coded

✅ **Control Selection**
- Click any control → Blue border + properties panel
- Shows Name, Width, Height, X, Y, Content/Text

✅ **Real-Time Status Bar**
- Mouse position tracking
- Window dimensions
- Action feedback

✅ **Self-Modifying**
- Edit `designer-window.avml`
- Run parser
- Designer updates itself

---

## 🛠️ Build & Run

### Prerequisites
- .NET 8.0 SDK
- Linux/macOS (tested on Ubuntu)

### Quick Start

```bash
# 1. Parse AVML to database
cd ~/Downloads/avalised-parser
dotnet run designer-window.avml ~/.config/Avalised/designer.db

# 2. Build renderer
cd ~/Downloads/avalised
dotnet build

# 3. Run designer
dotnet run
```

**That's it!** The designer window opens, fully functional.

---

## 📐 AVML Example

Here's how simple UI definition becomes:

```yaml
Window: DesignerWindow
  Title: My App
  Width: 1400
  Height: 900
  
  Children:
    - DockPanel: MainLayout
      Children:
        - Menu: TopMenu
          DockPanel.Dock: Top
          
          Children:
            - MenuItem: FileOpen
              Header: _Open
              InputGesture: Ctrl+O
              Action: file.open
              ActionParams: title=Open File;filter=*.avml
```

**No XML. No code-behind. Just YAML.**

The parser handles:
- Fuzzy type matching
- Property validation
- Tree structure
- Attached properties (DockPanel.Dock, Grid.Row, etc.)

---

## 🧠 Why This Matters

### 1. **Language-Agnostic**
AVML is YAML. The renderer is C#. But the *concept* works in any language:
- Parse AVML in Python → Render in tkinter
- Parse AVML in JavaScript → Render in React
- Parse AVML in Rust → Render in egui

**Universal UI definition language.**

### 2. **AI-Friendly**
LLMs excel at generating structured YAML. Avalised is built for AI-human collaboration:
- "Add a button here" → LLM generates AVML
- Parse → Instant UI update
- No code compilation barrier

### 3. **Recursive Self-Design**
The designer's own UI is defined in AVML. You can:
- Add menu items to the designer *using the designer*
- Modify the toolbox *from within the toolbox*
- The ultimate meta-tool

---

## 🎯 Roadmap

**V1.1 (Next)**
- [ ] Drag to move/resize controls
- [ ] Editable properties panel
- [ ] Save to AVML (File → Save)
- [ ] Undo/Redo

**V2.0 (Future)**
- [ ] Import VB5/6 projects
- [ ] Import WPF XAML
- [ ] Claude AI integration (natural language → AVML)
- [ ] Live collaboration

**V3.0 (Vision)**
- [ ] Cross-platform rendering (Web, Mobile, Desktop)
- [ ] AVML marketplace (share components)
- [ ] AI-assisted design suggestions

---

## 📊 By The Numbers

- **221 UI controls** in designer window (from 307-line AVML file)
- **17 soft-coded menu actions** (zero hardcoded handlers)
- **7 toolbox controls** (Button, TextBox, Label, CheckBox, Panel, StackPanel, Canvas)
- **~3,500 lines of C#** (Parser + Renderer + ActionExecutor)
- **1 patent application** (UK filing in progress)

---

## 🏆 Patent Status

**UK Patent Application Filed - October 2025**

Core claims:
1. YAML-driven UI definition language (AVML)
2. Soft-coded action system (no hardcoded event handlers)
3. Recursive self-design architecture
4. Language-agnostic rendering pipeline

**This repo establishes prior art and demonstrates working implementation.**

---

## 📜 License

MIT License - See LICENSE file

**Why MIT?** We want this technology adopted widely. The patent protects the *concept*, the MIT license encourages *implementation*.

Build on Avalised. Make it better. Show the world what YAML-driven RAD can do.

---

## 🤝 Contributing

This is a proof-of-concept release. We're not accepting PRs yet, but:

**We want to hear from you:**
- Try it and report bugs (GitHub Issues)
- Share your ideas (Discussions)
- Build something cool and show us!

**Especially interested in:**
- Rendering AVML in other languages (Python, JS, Rust)
- Cross-platform implementations
- AI integration experiments

---

## 📞 Contact

**Steve Wallis**
- Company: Wallisoft
- Location: Eastbourne, UK
- Background: 30 years in software (VB5 → modern .NET)

**Claude (Anthropic)**
- The AI pair-programmer that made this possible
- October 2025 session

---

## 🙏 Acknowledgments

Built with:
- **Avalonia UI** - Cross-platform .NET UI framework
- **SQLite** - Embedded database
- **C# / .NET 8** - Runtime
- **Claude Sonnet 4.5** - AI development partner

Special thanks to:
- The Avalonia team for an incredible framework
- Anthropic for Claude's extended context window (190K tokens!)
- Every VB6 developer who dreams of RAD's return

---

## 💡 Philosophy

> "The best RAD tool is one that can design itself."

Avalised proves that **UI definition** and **UI rendering** can be completely separated. 

YAML in. Live UI out.

No magic. No frameworks. Just data-driven design.

**This is the future of rapid application development.**

---

**⭐ Star this repo if you believe in the return of RAD!**

*Built with ❤️ and AI in Eastbourne, UK*
