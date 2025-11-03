# 🌳 Visualised - Meta-Engineered RAD IDE

**A revolutionary visual designer that builds itself.**

Built through human-AI collaboration between Steve Wallis (Wallisoft) and Claude (Anthropic).

## What Is This?

Visualised is a Rapid Application Development IDE that:
- Loads its own UI from VML (Visualised Markup Language) text files
- Uses SQLite for version control and state management
- Features intuitive drag-and-drop visual design
- Generates and consumes its own definition language
- Recursively self-defines its architecture

**This is meta-engineering in action.**

## The Breakthrough

Traditional designers fight their own controls. Visualised uses "ghost controls" - Border-based dummies that *represent* controls without *being* controls. This eliminates event conflicts and creates native-feeling drag-and-drop.

## Current Status

**V1.0 Alpha - Core Working**
- ✓ VML self-loading architecture
- ✓ Drag-and-drop from toolbox
- ✓ Selection and positioning
- ✓ SQLite-backed versioning
- ✓ Clean event model
- ✓ Smooth native feel

## Quick Start
```bash
dotnet build
dotnet run
```

Drag controls from the toolbox onto the canvas. Click to select. Drag to move. Delete key removes.

## Architecture

- `MainWindow.axaml.cs` - Core rendering engine (C# + Avalonia)
- `DesignerWindow.cs` - VML loader and UI builder
- `designer.vml` - The designer's own UI definition
- `visualised.db` - SQLite version control database
- `DatabaseManager.cs` - DB abstraction layer

## License

**Dual License Model:**

### For Individuals & Non-Profits
Free and open source under MIT License. Use it, modify it, share it.

### For Commercial Use
Businesses generating revenue using Visualised require a commercial license.
Contact: steve@wallisoft.com

## The Story

Built during an intense collaboration session that started with "let's make the toolbox work" and ended with a perfect drag-and-drop system at sunrise on Beachy Head.

Some things can't be planned. They emerge through genuine partnership.

When human vision meets AI capability, when iterative learning becomes genuine understanding, when conversation builds systems that build themselves - that's when the impossible becomes inevitable.

---

**Steve "recursion hurts my head" Wallis** (Wallisoft)  
**Claude "set: paste"** (Anthropic)

*Beachy Head, November 2025*
