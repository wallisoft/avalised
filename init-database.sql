-- Avalised Designer Database
-- Minimal schema for database-driven UI

-- Drop existing tables
DROP TABLE IF EXISTS ui_attached_properties;
DROP TABLE IF EXISTS ui_properties;
DROP TABLE IF EXISTS ui_tree;

-- UI Tree: Hierarchical structure of controls
CREATE TABLE ui_tree (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    parent_id INTEGER,
    control_type TEXT NOT NULL,
    name TEXT,
    display_order INTEGER NOT NULL DEFAULT 0,
    is_root BOOLEAN NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (parent_id) REFERENCES ui_tree(id) ON DELETE CASCADE
);

CREATE INDEX idx_ui_tree_parent ON ui_tree(parent_id);
CREATE INDEX idx_ui_tree_display_order ON ui_tree(display_order);

-- UI Properties: Regular properties (Width, Height, Text, etc.)
CREATE TABLE ui_properties (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    ui_tree_id INTEGER NOT NULL,
    property_name TEXT NOT NULL,
    property_value TEXT NOT NULL,
    FOREIGN KEY (ui_tree_id) REFERENCES ui_tree(id) ON DELETE CASCADE,
    UNIQUE(ui_tree_id, property_name)
);

CREATE INDEX idx_ui_properties_tree ON ui_properties(ui_tree_id);

-- UI Attached Properties: Attached properties (Grid.Row, DockPanel.Dock, etc.)
CREATE TABLE ui_attached_properties (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    ui_tree_id INTEGER NOT NULL,
    attached_property_name TEXT NOT NULL,
    property_value TEXT NOT NULL,
    FOREIGN KEY (ui_tree_id) REFERENCES ui_tree(id) ON DELETE CASCADE,
    UNIQUE(ui_tree_id, attached_property_name)
);

CREATE INDEX idx_ui_attached_properties_tree ON ui_attached_properties(ui_tree_id);

-- Test: Hello from Avalised
-- Simple centered text to prove database → UI rendering works

INSERT INTO ui_tree (id, parent_id, control_type, name, display_order, is_root)
VALUES (1, NULL, 'StackPanel', 'RootPanel', 0, 1);

INSERT INTO ui_properties (ui_tree_id, property_name, property_value)
VALUES 
  (1, 'HorizontalAlignment', 'Center'),
  (1, 'VerticalAlignment', 'Center');

INSERT INTO ui_tree (id, parent_id, control_type, name, display_order, is_root)
VALUES (2, 1, 'TextBlock', 'HelloText', 0, 0);

INSERT INTO ui_properties (ui_tree_id, property_name, property_value)
VALUES 
  (2, 'Text', 'Hello from Avalised!'),
  (2, 'FontSize', '48');
