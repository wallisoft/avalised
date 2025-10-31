AVALISED BUILD PROCESS - COMPLETE OUTLINE

=== SESSION RULES - CRITICAL ===

⚠️ WORKING ON LIVE SERVER - EXTREME CAUTION REQUIRED

1. NO DELETIONS - Never delete files without explicit user approval
2. BASH COMMANDS - Always provide bash commands for user review before execution
3. GZIP EVERYTHING - Compress verbose output to save tokens (| gzip > /tmp/output.gz)
4. PROMPT BEFORE MAJOR CHANGES - Any structural changes require confirmation
5. BACKUP FIRST - Always .bak before modifying working files
6. SERVER AWARENESS - You execute on user's live server via c-helper API, not local
7. TOKEN CONSERVATION - Make session last as long as possible
8. NO ASSUMPTIONS - When unclear, ask - don't guess

=== ARCHITECTURE ===

3-Component System:
1. AVML Source - YAML-based UI definition language
2. Parser - C# dotnet app converts AVML → SQLite
3. Renderer - Avalonia C# app reads SQLite → Live UI

=== FILE LOCATIONS ===

~/Downloads/avalised-parser/
  - designer-window.avml (SOURCE - edit this)
  - designer-schema.sql (DB schema)
  - Program.cs (parser)
  - Avalised.AVMLParser.csproj

~/Downloads/avalised/
  - designer.db (BUILD location - temporary)
  - UITreeBuilder.cs (reads DB, builds Avalonia controls)
  - DesignerLayout.cs (loads UITreeBuilder)
  - MainWindow.cs (entry point)
  - Avalised.csproj

~/.config/Avalised/
  - designer.db (RUNTIME location - app reads from here!)

CRITICAL: Avalised loads from ~/.config/Avalised/designer.db at runtime, NOT from ~/Downloads/avalised/designer.db

=== DATABASE SCHEMA ===

Required tables:
- ui_tree (control hierarchy)
- ui_properties (Width, Height, Background, etc.)
- ui_attached_properties (DockPanel.Dock, Grid.Row, etc.) - CRITICAL, must exist!
- control_types (196 Avalonia control definitions for fuzzy matching)
- control_properties (property definitions)

Missing ui_attached_properties causes SQL errors!

=== BUILD PIPELINE (CORRECTED) ===

1. Edit AVML via API:
   PUT /api/file?project=avalised-parser&path=designer-window.avml
   
2. Sync to filesystem (CRITICAL - don't skip!):
   POST /api/sync?project=avalised-parser
   
3. Parse to build location:
   cd ~/Downloads/avalised-parser
   dotnet run designer-window.avml ~/Downloads/avalised/designer.db
   
4. Copy to runtime location:
   cp ~/Downloads/avalised/designer.db ~/.config/Avalised/designer.db
   
5. Build:
   cd ~/Downloads/avalised && dotnet build
   
6. Run:
   dotnet run
   
7. Verify:
   Check ~/avalised-tree.txt (auto-generated visual tree dump)

ONE-LINER BUILD:
cd ~/Downloads/avalised && rm -f designer.db && sqlite3 designer.db < ~/Downloads/avalised-parser/designer-schema.sql && cd ~/Downloads/avalised-parser && dotnet run designer-window.avml ~/.config/Avalised/designer.db && cd ~/Downloads/avalised && dotnet build && pkill -9 -f avalised ; dotnet run > /dev/null 2>&1 &

=== C-HELPER API SYSTEM ===

Remote build orchestrator eliminates file uploads:

API: http://tmp.avalised.io:8888
Key: dev-1761880081

Endpoints:
- GET  /api/file?project=X&path=Y     (read)
- PUT  /api/file?project=X&path=Y     (write to DB)
- POST /api/sync?project=X            (DB → filesystem) ⚠️ REQUIRED after PUT
- POST /api/build?project=X           (remote build)
- POST /api/exec                      (run commands)

Workflow:
Claude → PUT file to API (DB storage)
      → POST sync (write to disk) ⚠️ CRITICAL STEP
      → POST exec (parse AVML)
      → POST exec (copy to ~/.config/Avalised/)
      → POST build (compile)
      → POST exec (run & capture output)
      → Verify programmatically

=== PARSER INTELLIGENCE ===

196 control type definitions enable fuzzy matching:
- Auto-corrects malformed YAML
- Repositions misplaced elements
- Infers structure from context
- Generates clean SQL from messy input
- "No more YAML errors"

Parser output "Controls: 196" = control definitions, NOT UI element count
Actual UI elements in ui_tree table = ~57-60 controls

=== UITREEBUILDER RENDERING ===

Recursive process:
1. Query ui_tree WHERE is_root=1
2. BuildControl(id):
   - Create control from control_type
   - Apply ui_properties (Width, Height, Background, etc.)
   - Apply ui_attached_properties (DockPanel.Dock, Grid.Row, etc.)
   - Recursively build children
3. Return complete Avalonia control tree

Property mapping (SetProperty switch):
- Width/Height → double.Parse
- Background/Foreground → Brush.Parse
- Margin/Padding → Thickness.Parse
- Orientation → Enum.Parse
- Text/Content/Header → string values
- FontSize → double.Parse

Attached property mapping:
- DockPanel.Dock → Dock enum
- Grid.Row/Column/RowSpan/ColumnSpan → int

=== TOKEN ECONOMICS ===

API vs Upload:
- File upload: Files stay in context, 18K tokens × 10 messages = 180K tokens
- API method: Files in DB, ~2K tokens per iteration = 24K tokens for full build cycle
- 85%+ savings

Current session: 86.5K used / 190K total (45.5%) - good shape!

=== CURRENT STATE ===

Modified:
- Menu [TopMenu] height: 26px (20% reduction from ~32px default)
- Menu FontSize: 14 (20% increase for readability)
- Menu Padding: 2,0,2,0
- Added Tools menu with Live Reload (Ctrl+R) - not yet wired
- Added Preview menu (▶) with HorizontalAlignment: Right

Backups:
- ~/Downloads/avalised-parser/designer-window.avml.bak
- ~/Downloads/avalised-parser/designer-window.avml.bak2
- ~/Downloads/avalised-parser/designer-window.avml.bak3
- ~/Downloads/avalised/designer-window.avml.bak (old/unused file)

Database locations:
- Build: ~/Downloads/avalised/designer.db
- Runtime: ~/.config/Avalised/designer.db (ACTIVE)

=== KEY PRINCIPLES ===

1. AVML is source - Edit YAML, not C#
2. Parser is smart - Fixes errors automatically
3. DB is intermediate - SQLite stores parsed structure
4. UITreeBuilder is dumb - Just reads DB and builds
5. Tree dump is verification - Programmatic validation without visual inspection
6. API eliminates uploads - Remote execution, massive token savings
7. All builds server-side - Zero local dependencies
8. ALWAYS sync after API edits - POST /api/sync before parsing
9. Runtime DB is ~/.config/Avalised/ - NOT build directory
10. ui_attached_properties table is REQUIRED - SQL errors without it

=== PROJECTS REGISTERED ===

avalised (C# Avalonia renderer)
avalised-parser (C# AVML parser)
visualised-markup (related project)
menu-test (test project for menu properties)

=== COMMON ISSUES & SOLUTIONS ===

Problem: Changes don't appear in running app
Solution: Database not copied to ~/.config/Avalised/designer.db

Problem: SQL error "no such table: ui_attached_properties"
Solution: Schema missing table, recreate with full designer-schema.sql

Problem: Parser says 196 controls but DB has 57
Solution: Normal - 196 = control definitions, 57 = actual UI elements

Problem: AVML changes not parsed
Solution: Forgot POST /api/sync after PUT /api/file

Problem: Window not selectable/focusable
Solution: Kill old process before restarting (pkill -9 -f avalised)

Session complete. Ready for next iteration.
