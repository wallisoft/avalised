# Database Architecture

## Overview

Visualised Markup uses a single SQLite database containing both control type definitions and UI layout data. The `UITreeBuilder` class reads this database to construct the designer interface at runtime.

## Schema

### control_types
Defines available Avalonia control types.

```
id              INTEGER PRIMARY KEY
name            TEXT UNIQUE NOT NULL
can_have_children BOOLEAN
description     TEXT
```

### control_properties
Maps available properties to control types.

```
id              INTEGER PRIMARY KEY
control_type_id INTEGER FOREIGN KEY
property_name   TEXT
property_type   TEXT
default_value   TEXT
```

### ui_tree
Hierarchical structure of control instances.

```
id           INTEGER PRIMARY KEY
parent_id    INTEGER FOREIGN KEY (self-referencing)
name         TEXT UNIQUE NOT NULL
control_type TEXT
sort_order   INTEGER
```

### ui_properties
Property values for each control instance.

```
id              INTEGER PRIMARY KEY
ui_tree_id      INTEGER FOREIGN KEY
property_name   TEXT
property_value  TEXT
```

### actions
Event handlers and action bindings.

```
id           INTEGER PRIMARY KEY
control_name TEXT
action_name  TEXT
capability   TEXT
```

### action_parameters
Parameters for action execution.

```
id             INTEGER PRIMARY KEY
action_id      INTEGER FOREIGN KEY
parameter_name TEXT
parameter_value TEXT
```

## Data Flow

1. Application starts and calls `UITreeBuilder(dbPath)`
2. `UITreeBuilder` queries `ui_tree` table for root control (parent_id = NULL)
3. For each control, queries `ui_properties` for property values
4. Recursively builds child controls by querying where parent_id matches current control
5. Applies properties using reflection to set Avalonia control properties
6. Wires up actions from `actions` table to control events

## Database Locations

- Development: `./designer.db` (checked first)
- Production: `~/.config/Avalised/designer.db` (fallback)

The `MainWindow.LoadUI()` method checks the current directory before falling back to the user config directory.

## YAML Export

`AVMLExporter` class converts the in-memory control tree to YAML format:
- Traverses control hierarchy
- Exports type and properties
- Writes human-readable YAML structure

`AVMLLoader` performs the inverse operation, loading controls from YAML files.

## Design Rationale

Single database approach provides:
- Atomic version control (entire UI in one file)
- Language-agnostic data format (SQL)
- Recursive self-building capability
- No external dependencies for UI definition
