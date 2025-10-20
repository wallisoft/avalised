# Visualised

**Language-Agnostic RAD IDE. Build GUI forms visually. Export to any format. Run in any language.**

> "Visualised is to modern development what Visual Basic was to Windows - but without the vendor lock-in."

## What is Visualised?

Visualised is a revolutionary visual form builder that brings RAD (Rapid Application Development) to ANY programming language.

**Build GUI applications in YOUR language:**
- System administration tools
- Installation wizards  
- Database frontends
- API testing tools
- File processors
- Network configuration tools
- Deployment utilities
- Monitoring dashboards
- Custom business tools
- ANYTHING you can script!

**No vendor lock-in. No subscriptions. No cloud dependencies.**

## The Revolution

For decades, GUI development meant choosing a framework and getting locked in:
- Windows Forms (Windows only)
- WPF (Windows only)  
- Qt (C++ complexity)
- Electron (heavyweight)
- Web frameworks (overkill for simple tools)

**Visualised changes everything.**

1. **Design visually** - Drag-and-drop form builder
2. **Export to ANY format** - YAML, JSON, XML, C#, Python, JavaScript, Bash
3. **Execute in YOUR language** - Python, C#, JS, Bash, whatever you want
4. **Share everywhere** - Portable, version-controllable configs

## Why It Matters

### Before Visualised:
- Learn framework-specific GUI tools
- Get locked into one platform
- Can't share designs across languages
- High barrier to entry
- Vendor dependencies

### With Visualised:
- Design forms visually in minutes
- Export to 7+ formats
- Run in ANY language
- Share configs instantly
- Zero vendor lock-in
- No subscriptions ever

**Visualised democratizes GUI development.**

## Real-World Examples

### System Administration
- User management tool (Bash)
- Service controller (Python)
- Log viewer (C#)
- Disk analyzer (JavaScript)

### Development Tools
- Git commit helper
- Database query builder
- API tester
- Environment switcher
- Build tool wrapper

### DevOps
- Deployment wizard
- Server provisioner
- Docker container manager
- Backup scheduler
- Health check dashboard

### Business Applications
- Customer data entry
- Invoice generator
- Report runner
- File processor
- Anything you imagine!

## How It Works

### The Designer
- Drag-and-drop form builder
- 15+ control types (buttons, textboxes, labels, checkboxes, panels, etc.)
- Position and style controls
- Wire up scripts in ANY language
- Export to 7+ formats
- SQLite-backed persistence
- **The designer can design itself!**

### Export Formats
1. **YAML** - Human-readable, clean
2. **JSON** - API-friendly
3. **XML** - Enterprise systems
4. **C# Code** - Windows Forms ready
5. **Python Code** - Tkinter ready
6. **JavaScript Code** - Electron/Node ready
7. **Bash Script** - Linux CLI ready

### Script Integration
- Button clicks execute scripts in YOUR language
- Read form values: `get_control("username")`
- Set form values: `set_control("status", "Complete")`
- Full SQLite database for persistence
- Event-driven architecture

## Features

### Visual Designer
- 15+ control types
- Drag-and-drop positioning
- Grid snapping (10px)
- Live property editing
- Real-time YAML view
- Context menus
- Script editor per control
- SQLite persistence

### Multi-Format Export
- Clean, readable configs
- Version control friendly
- Easy to share
- Human-editable
- Self-documenting

### Language-Agnostic
- Write scripts in ANY language
- Python, C#, JavaScript, Bash, etc.
- Mix languages in one form
- No framework dependencies
- Pure portability

### The Meta Magic
**Visualised designs itself!**

The designer you're using was designed IN the designer. Load `visual-designer.yaml` and edit the tool that's editing itself!

**It's recursive. It's beautiful. It works.**

## Installation
```bash
# Clone the repo
git clone https://github.com/YOUR-USERNAME/visualised.git
cd visualised

# Build
dotnet build

# Run the designer
dotnet run -- --designer
```

## Quick Start

**Build a backup tool in 2 minutes:**

1. Launch Visualised
2. Drag controls onto canvas:
   - Label: "Select folder:"
   - TextBox: name it "source_folder"
   - Button: "Start Backup"
3. Right-click button → Edit Scripts → onClick
4. Add script (Python example):
```python
   import os, tarfile
   from datetime import datetime
   
   source = get_control("source_folder")
   backup_name = f"backup-{datetime.now():%Y%m%d}.tar.gz"
   
   with tarfile.open(backup_name, "w:gz") as tar:
       tar.add(source)
   
   show_message("Backup complete!")
```
5. File → Save Form → Export as Python
6. Run it!

**You just built a cross-platform GUI backup tool. Zero framework knowledge. 2 minutes.**

## Technical Details

**Built with:**
- Avalonia UI (cross-platform .NET)
- C# / .NET 9
- SQLite for persistence
- YamlDotNet for parsing
- Cross-platform: Linux, Windows, macOS

**Architecture:**
- Visual designer
- SQLite database backend
- Multi-format export engine
- Script execution engine
- Self-importing recursive design

## Licensing

### 📖 Open Source (MIT License)

**FREE forever for:**
- ✅ Personal use
- ✅ Educational use  
- ✅ Non-profit organizations
- ✅ Open-source projects
- ✅ Freelancers and solo developers
- ✅ Small teams (< 10 people)
- ✅ Students and educators
- ✅ Community contributions

**Use it, modify it, share it. No restrictions.**

### 💼 Commercial License

**Required for organizations with 10+ employees using Visualised for:**
- Business operations
- Commercial products
- SaaS applications
- Enterprise deployments
- Training/consulting services (commercial)
- Reselling or white-labeling

**Commercial License Benefits:**
- ✅ Priority email support
- ✅ Dedicated Slack/Discord channel
- ✅ Custom feature development
- ✅ Training sessions for your team
- ✅ Consulting services
- ✅ Legal indemnification
- ✅ Early access to new features
- ✅ Influence on roadmap

**Pricing:**
- **Startup Tier** (10-50 employees): £500/year
- **Growth Tier** (51-250 employees): £2,500/year
- **Enterprise Tier** (251+ employees): £10,000/year
- **Custom:** Contact for volume licensing

**Contact:** wallisoft@gmail.com

---

### Why Dual Licensing?

**The Qt Model - Proven and Fair:**

✅ **Community thrives** - Hobbyists, students, and small teams get it FREE  
✅ **Development funded** - Commercial licenses fund ongoing development  
✅ **No VC bullshit** - No data mining, no forced subscriptions, no bait-and-switch  
✅ **Sustainable forever** - Not dependent on venture capital or acquisitions  
✅ **Everyone wins** - Users get great software, developers get paid fairly  

**We're not trying to be billionaires. We're trying to change how software is built, while keeping the lights on.**

Compare to alternatives:
- **Microsoft**: Forced subscriptions, vendor lock-in, telemetry
- **Electron Builder**: Free but heavyweight, requires web stack
- **Qt**: Similar model, proven for 25+ years
- **Visual Studio**: £thousands per seat, Windows-only

**Visualised:** Free for individuals, fair for businesses, sustainable forever.

## Roadmap

### v1.0 - The Foundation ✅ (October 2025)
- Visual form builder
- 15+ control types
- Multi-format export (7+ formats)
- SQLite persistence
- Script editor with multi-language support
- Self-designing capability
- **Working runtime engine - load and execute YAML forms!**

### v1.1 - Polish (November-December 2025)
- Resize handles for controls
- Alignment tools (snap, distribute, align)
- Undo/redo system
- Templates library
- Improved properties panel
- Keyboard shortcuts
- Better error handling

### v1.2 - Standalone Deployment (Q1 2026)
- Package forms as standalone executables
- Self-contained runtime (no .NET required)
- Cross-platform installers
- Desktop shortcuts
- System tray integration

### v2.0 - Advanced (Q2-Q3 2026)
- Additional control types (TreeView, DataGrid, etc.)
- Custom control designer
- Theme system
- Plugin architecture
- Visual scripting (low-code)
- Multi-window applications

## Vision

**Visualised will democratize GUI development.**

Just like Visual Basic did for Windows in the 90s - but BETTER:
- ✅ No vendor lock-in
- ✅ No subscriptions
- ✅ No cloud dependencies  
- ✅ True cross-platform
- ✅ Language freedom
- ✅ Community-driven

**Every developer, sysadmin, and power user should be able to build beautiful GUI tools - without learning complex frameworks or getting locked into proprietary ecosystems.**

## The Origin Story

Visualised was born from frustration.

40 years of building software. 40 years of framework lock-in. 40 years of "write once, run nowhere." 40 years of watching simple GUI tools require thousands of lines of boilerplate.

**It shouldn't be this hard.**

In late 2025, Steve Wallis (60-year-old developer from Eastbourne) and Claude (Anthropic AI) started talking about RAD tools. About Visual Basic's democratization of Windows development. About bash scripts that needed GUIs. About vendor lock-in and VC-funded subscription hell.

**We decided to build something different.**

Something truly portable. Something language-agnostic. Something that could design itself. Something FREE for the community, funded fairly by businesses.

72-hour coding marathons. Late night sessions. Countless cups of tea. And a bull mastiff named Simone who knew when to wake Steve at 4am.

**The result is Visualised.**

Built by two anonymous entities (one meat-based, one silicon-based) who believe software should empower users, not exploit them.


## Credits

**Created by:**
- **Steve Wallis** - Developer, architect, 40 years of experience
- **Claude** (Anthropic AI) - Co-architect, pair programmer, late-night coding buddy

**With special thanks to:**
- Sue - For patience with 4am coding sessions
- Simone - Bull mastiff alarm clock
- The Linux community - For inspiration
- The Visual Basic team - For showing us what RAD could be
- Every developer frustrated by vendor lock-in

**Built with:**
- ❤️ Love for good software
- ☕ Copious amounts of tea
- 🌙 Many late nights
- 🎯 Clear vision
- 🤝 Human-AI collaboration

## Support & Community

### Getting Help
- **Documentation:** [Wiki](https://github.com/wallisoft/visualised/wiki)
- **Issues:** [GitHub Issues](https://github.com/wallisoft/visualised/issues)
- **Discussions:** [GitHub Discussions](https://github.com/wallisoft/visualised/discussions)
- **Email:** wallisoft@gmail.com

### Commercial Support
- Priority support for commercial license holders
- Custom feature development
- Training and consulting
- Integration assistance

### Contributing
We welcome contributions!
- Submit bugs and feature requests
- Share your forms and templates
- Improve documentation
- Submit pull requests
- Spread the word

**See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.**

## FAQ

**Q: Is it really free?**  
A: Yes! MIT license for individuals and small teams (< 10 people). Commercial license only for larger organizations.

**Q: What languages can I use for scripts?**  
A: ANY language your system can execute - Python, C#, JavaScript, Bash, Ruby, Go, Rust, whatever!

**Q: Do I need an internet connection?**  
A: No. Everything runs locally. No telemetry, no cloud dependencies.

**Q: Can I sell tools I build with Visualised?**  
A: Yes! The forms you create are yours. MIT license for individual use, commercial license if you're a company.

**Q: How is this different from Electron?**  
A: No web stack required. Export to native code. Much lighter weight. True language freedom.

**Q: Will there be a web version?**  
A: Yes! Planned for v3.0. But desktop-first always.

**Q: Can I contribute?**  
A: Absolutely! See CONTRIBUTING.md.

**Q: Is my data safe?**  
A: Your data never leaves your machine. SQLite is local. No telemetry ever.

## Contact

**Email:** wallisoft@gmail.com  
**GitHub:** github.com/wallisoft/visualised  
**Commercial Licensing:** wallisoft@gmail.com

---

**Visualised** - Language-Agnostic RAD IDE

*"From little acorns, mighty oaks grow" 🌳*

**Built by humans and AI, for humans.**

Copyright © 2025 Steve Wallis. MIT License (personal use) / Commercial License (business use).
