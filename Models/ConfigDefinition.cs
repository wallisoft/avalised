using System.Collections.Generic;

namespace ConfigUI.Models
{
    public class ConfigDefinition
    {
        // ═══════════════════════════════════════
        // CORE IDENTITY
        // ═══════════════════════════════════════
        public string Type { get; set; } = "label";
        public string Name { get; set; } = "";
        public string? Tag { get; set; }
        public int? Index { get; set; }
        
        // ═══════════════════════════════════════
        // CONTENT
        // ═══════════════════════════════════════
        public string? Caption { get; set; }
        public string? Text { get; set; }
        public string? Title { get; set; }
        public string? Hint { get; set; }
        public string? ToolTip { get; set; }
        public string? Placeholder { get; set; }
        public string? Watermark { get; set; }
        
        // ═══════════════════════════════════════
        // POSITION & SIZE
        // ═══════════════════════════════════════
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? MinWidth { get; set; }
        public double? MaxWidth { get; set; }
        public double? MinHeight { get; set; }
        public double? MaxHeight { get; set; }
        
        // ═══════════════════════════════════════
        // LAYOUT & SPACING
        // ═══════════════════════════════════════
        public double? MarginLeft { get; set; }
        public double? MarginTop { get; set; }
        public double? MarginRight { get; set; }
        public double? MarginBottom { get; set; }
        public double? PaddingLeft { get; set; }
        public double? PaddingTop { get; set; }
        public double? PaddingRight { get; set; }
        public double? PaddingBottom { get; set; }
        public double? Spacing { get; set; }
        
        // ═══════════════════════════════════════
        // Z-ORDER & TAB ORDER
        // ═══════════════════════════════════════
        public int? ZIndex { get; set; }
        public int? TabIndex { get; set; }
        public bool? TabStop { get; set; }
        
        // ═══════════════════════════════════════
        // COLORS & APPEARANCE
        // ═══════════════════════════════════════
        public string? BackgroundColor { get; set; }
        public string? BackColor { get; set; }
        public string? ForegroundColor { get; set; }
        public string? ForeColor { get; set; }
        public string? BorderColor { get; set; }
        public double? BorderThickness { get; set; }
        public string? BorderStyle { get; set; }
        public double? Opacity { get; set; }
        public string? Cursor { get; set; }
        public string? CursorType { get; set; }
        
        // ═══════════════════════════════════════
        // FONT PROPERTIES
        // ═══════════════════════════════════════
        public string? FontFamily { get; set; }
        public string? FontName { get; set; }
        public double? FontSize { get; set; }
        public bool FontBold { get; set; }
        public bool? FontItalic { get; set; }
        public bool? FontUnderline { get; set; }
        public bool? FontStrikethrough { get; set; }
        public string? FontWeight { get; set; }
        public string? FontStyle { get; set; }
        
        // ═══════════════════════════════════════
        // ALIGNMENT
        // ═══════════════════════════════════════
        public string? Alignment { get; set; }
        public string? HorizontalAlignment { get; set; }
        public string? VerticalAlignment { get; set; }
        public string? TextAlignment { get; set; }
        public string? ContentAlignment { get; set; }
        
        // ═══════════════════════════════════════
        // BEHAVIOR & STATE
        // ═══════════════════════════════════════
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool? ReadOnly { get; set; }
        public bool? Locked { get; set; }
        public bool? CanFocus { get; set; }
        public bool? AutoSize { get; set; }
        public bool? AutoScale { get; set; }
        public bool? ClipToBounds { get; set; }
        
        // ═══════════════════════════════════════
        // TEXT CONTROL PROPERTIES
        // ═══════════════════════════════════════
        public bool? Multiline { get; set; }
        public bool? WordWrap { get; set; }
        public bool? AcceptsReturn { get; set; }
        public bool? AcceptsTab { get; set; }
        public int? MaxLength { get; set; }
        public string? PasswordChar { get; set; }
        public bool? HideSelection { get; set; }
        public string? ScrollBars { get; set; }
        
        // ═══════════════════════════════════════
        // SELECTION & CHECKBOX
        // ═══════════════════════════════════════
        public bool? Checked { get; set; }
        public int? CheckState { get; set; }
        public bool? ThreeState { get; set; }
        public bool? AutoCheck { get; set; }
        
        // ═══════════════════════════════════════
        // LIST & COMBO PROPERTIES
        // ═══════════════════════════════════════
        public int? SelectedIndex { get; set; }
        public string? SelectedValue { get; set; }
        public string? SelectedText { get; set; }
        public bool? Sorted { get; set; }
        public string? DropDownStyle { get; set; }
        public int? DropDownWidth { get; set; }
        public int? ItemHeight { get; set; }
        public bool? IntegralHeight { get; set; }
        
        // ═══════════════════════════════════════
        // NUMERIC & RANGE CONTROLS
        // ═══════════════════════════════════════
        public int? Minimum { get; set; }
        public int? Maximum { get; set; }
        public int? Value { get; set; }
        public int? SmallChange { get; set; }
        public int? LargeChange { get; set; }
        public int? TickFrequency { get; set; }
        public string? Orientation { get; set; }
        
        // ═══════════════════════════════════════
        // TIMER PROPERTIES
        // ═══════════════════════════════════════
        public int? Interval { get; set; }
        public bool? AutoReset { get; set; }
        
        // ═══════════════════════════════════════
        // IMAGE PROPERTIES
        // ═══════════════════════════════════════
        public string? ImageSource { get; set; }
        public string? Image { get; set; }
        public string? Picture { get; set; }
        public string? Icon { get; set; }
        public string? ImageAlignment { get; set; }
        public string? SizeMode { get; set; }
        public bool? Stretch { get; set; }
        
        // ═══════════════════════════════════════
        // DATA BINDING (VB5/VB6 ADO STYLE)
        // ═══════════════════════════════════════
        public string? DataSource { get; set; }
        public string? DataMember { get; set; }
        public string? ValueMember { get; set; }
        public string? DisplayMember { get; set; }
        public string? DataBindings { get; set; }
        public string? RecordSource { get; set; }
        public string? RecordsetType { get; set; }
        public string? DatabaseName { get; set; }
        public string? Connect { get; set; }
        public bool? DataChanged { get; set; }
        
        // ═══════════════════════════════════════
        // CONTAINER PROPERTIES
        // ═══════════════════════════════════════
        public bool? IsMdiContainer { get; set; }
        public string? DockStyle { get; set; }
        public string? AnchorStyles { get; set; }
        public bool? AutoScroll { get; set; }
        public string? FormBorderStyle { get; set; }
        public bool? ControlBox { get; set; }
        public bool? MinimizeBox { get; set; }
        public bool? MaximizeBox { get; set; }
        public bool? ShowInTaskbar { get; set; }
        public string? StartPosition { get; set; }
        public string? WindowState { get; set; }
        public bool? TopMost { get; set; }
        
        // ═══════════════════════════════════════
        // GRID PROPERTIES
        // ═══════════════════════════════════════
        public int? RowCount { get; set; }
        public int? ColumnCount { get; set; }
        public bool? AllowUserToAddRows { get; set; }
        public bool? AllowUserToDeleteRows { get; set; }
        public bool? AllowUserToResizeRows { get; set; }
        public bool? AllowUserToResizeColumns { get; set; }
        public bool? MultiSelect { get; set; }
        public string? SelectionMode { get; set; }
        public bool? RowHeadersVisible { get; set; }
        public bool? ColumnHeadersVisible { get; set; }
        public int? RowHeadersWidth { get; set; }
        public int? ColumnHeadersHeight { get; set; }
        
        // ═══════════════════════════════════════
        // VALIDATION
        // ═══════════════════════════════════════
        public bool? CausesValidation { get; set; }
        public string? ValidationExpression { get; set; }
        public string? ErrorMessage { get; set; }
        
        // ═══════════════════════════════════════
        // MENU & CONTEXT MENU
        // ═══════════════════════════════════════
        public string? ContextMenu { get; set; }
        public bool? ShowShortcut { get; set; }
        public string? ShortcutKeys { get; set; }
        
        // ═══════════════════════════════════════
        // DRAG & DROP
        // ═══════════════════════════════════════
        public bool? AllowDrop { get; set; }
        public string? DragMode { get; set; }
        
        // ═══════════════════════════════════════
        // MISC VB5 PROPERTIES
        // ═══════════════════════════════════════
        public string? HelpContextID { get; set; }
        public string? WhatsThisHelp { get; set; }
        public bool? RightToLeft { get; set; }
        public bool? UseWaitCursor { get; set; }
        public string? AccessibleName { get; set; }
        public string? AccessibleDescription { get; set; }
        public string? AccessibleRole { get; set; }
        
        // ═══════════════════════════════════════
        // NESTED STRUCTURES
        // ═══════════════════════════════════════
        public List<ConfigDefinition>? Controls { get; set; }
        public List<Dictionary<string, object>>? Items { get; set; }
        public Dictionary<string, string>? Scripts { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
        
        // ═══════════════════════════════════════
        // HELPER METHOD
        // ═══════════════════════════════════════
        public T GetProperty<T>(string name, T defaultValue)
        {
            if (CustomProperties != null && CustomProperties.ContainsKey(name))
            {
                try
                {
                    return (T)CustomProperties[name];
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}