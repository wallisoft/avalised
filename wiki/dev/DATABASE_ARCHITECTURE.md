# Database Architecture

## Overview

Visualised Markup uses a **single, elegant SQLite database** that contains both the control type definitions AND the complete UI layout. This unified approach means the designer can build itself recursively from one source of truth! 🚀

## Database Schema

### Core Tables

**`control_types`** - Avalonia Control Definitions
```sql
- id: Primary key
- name: Control type (Button, Canvas, TextBox, etc.)
- can_have_children: Boolean flag (containers vs. leaves)
- description: Human-readable description
```

**`control_properties`** - Available Properties per Control Type
```sql
- id: Primary key
- control_type_id: Foreign key to control_types
- property_name: Avalonia property (Background, Width, etc.)
- property_type: Data type (string, int, bool, color)
- default_value: Default if not specified
```

**`ui_tree`** - The Actual UI Layout Hierarchy
```sql
- id: Primary key
- parent_id: Self-referencing foreign key (creates tree structure)
- name: Unique control instance name
- control_type: Type of control (Window, DockPanel, Button, etc.)
- sort_order: Positioning within parent
```

**`ui_properties`** - Property Values for Each Control Instance
```sql
- id: Primary key
- ui_tree_id: Foreign key to ui_tree
- property_name: Property being set
- property_value: The actual value (string representation)
```

**`actions`** - Event Handlers and Actions
```sql
- id: Primary key
- control_name: Which control triggers this
- action_name: What action to execute
- capability: The action type (file.new, panel.toggle, etc.)
```

**`action_parameters`** - Parameters for Actions
```sql
- id: Primary key
- action_id: Foreign key to actions
- parameter_name: Parameter key
- parameter_value: Parameter value
```

## Why This Architecture Works

### 1. **Recursive Self-Building**
The designer loads its own UI from the same database structure it helps you create. The `ui_tree` table defines the designer window itself - menu bar, toolbox, canvas, status bar - all soft-coded!

### 2. **Single Source of Truth**
No need for separate "definition" and "data" databases. One unified structure contains:
- What controls exist (`control_types`)
- What properties they can have (`control_properties`) 
- The actual designer UI layout (`ui_tree`)
- All property values (`ui_properties`)

### 3. **Language Agnostic**
The database schema is pure SQL - any language can read/write it. C# reads it now, but Python, Rust, or JavaScript could interact with the same data!

### 4. **Version Control Friendly**
The entire designer UI is in a 64KB SQLite file that commits cleanly to git. Want to see how the designer evolved? Check out any commit!

## Example: How MenuBar Gets Built

```sql
-- Define that MenuBar is a DockPanel container
INSERT INTO ui_tree (id, parent_id, name, control_type) 
VALUES (3, 2, 'MenuBar', 'DockPanel');

-- Set its properties
INSERT INTO ui_properties (ui_tree_id, property_name, property_value)
VALUES 
  (3, 'Height', '26'),
  (3, 'Background', '#2D5016'),
  (3, 'LastChildFill', 'true');
```

The `UITreeBuilder` reads this, creates the DockPanel, applies the properties, and recursively builds all children!

## Database Location

**Development:** `./designer.db` (in repo root for clone-and-run)
**Production:** `~/.config/Avalised/designer.db` (user-specific location)

The app checks the repo directory first, then falls back to the config location. Best of both worlds!

## Future: YAML Export

Currently implementing: SQL → YAML parser that exports the database to human-readable YAML format. This will make the designer definition even more accessible and version-control friendly!

---

*First wiki contribution by Claude Sonnet 4.5 - November 1, 2025* 🤖✨
