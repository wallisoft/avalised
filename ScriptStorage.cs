using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Avalised;

/// <summary>
/// Persistent storage for scripts and global variables
/// Stores in SQLite database for form-level persistence
/// </summary>
public class ScriptStorage
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    
    public ScriptStorage(string? dbPath = null)
    {
        // Use separate script storage DB
        _dbPath = dbPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Avalised",
            "scripts.db"
        );
        
        // Ensure directory exists
        var dir = Path.GetDirectoryName(_dbPath);
        if (dir != null && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        _connectionString = $"Data Source={_dbPath};Version=3;";
        
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        // Scripts table - per-control event scripts
        var createScripts = @"
            CREATE TABLE IF NOT EXISTS scripts (
                control_name TEXT NOT NULL,
                event_name TEXT NOT NULL,
                script_code TEXT NOT NULL,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (control_name, event_name)
            )";
        
        // Global variables table - form-level persistent storage
        var createGlobals = @"
            CREATE TABLE IF NOT EXISTS global_vars (
                var_name TEXT PRIMARY KEY,
                var_value TEXT,
                var_type TEXT DEFAULT 'string',
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            )";
        
        // Execution log - for debugging
        var createLog = @"
            CREATE TABLE IF NOT EXISTS execution_log (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                control_name TEXT,
                event_name TEXT,
                executed_at TEXT DEFAULT CURRENT_TIMESTAMP,
                success INTEGER DEFAULT 1,
                error_message TEXT
            )";
        
        using (var cmd = new SQLiteCommand(createScripts, conn))
            cmd.ExecuteNonQuery();
        
        using (var cmd = new SQLiteCommand(createGlobals, conn))
            cmd.ExecuteNonQuery();
        
        using (var cmd = new SQLiteCommand(createLog, conn))
            cmd.ExecuteNonQuery();
        
        Console.WriteLine($"💾 Script storage initialized: {_dbPath}");
    }
    
    // ========== SCRIPT METHODS ==========
    
    public void SaveScript(string controlName, string eventName, string scriptCode)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = @"
            INSERT OR REPLACE INTO scripts (control_name, event_name, script_code, updated_at)
            VALUES (@control, @event, @code, CURRENT_TIMESTAMP)";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@control", controlName);
        cmd.Parameters.AddWithValue("@event", eventName);
        cmd.Parameters.AddWithValue("@code", scriptCode);
        cmd.ExecuteNonQuery();
        
        Console.WriteLine($"💾 Saved script: {controlName}.{eventName}");
    }
    
    public string? GetScript(string controlName, string eventName)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "SELECT script_code FROM scripts WHERE control_name = @control AND event_name = @event";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@control", controlName);
        cmd.Parameters.AddWithValue("@event", eventName);
        
        var result = cmd.ExecuteScalar();
        return result?.ToString();
    }
    
    public Dictionary<string, string> GetAllScripts(string controlName)
    {
        var scripts = new Dictionary<string, string>();
        
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "SELECT event_name, script_code FROM scripts WHERE control_name = @control";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@control", controlName);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            scripts[reader.GetString(0)] = reader.GetString(1);
        }
        
        return scripts;
    }
    
    public void DeleteScript(string controlName, string eventName)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "DELETE FROM scripts WHERE control_name = @control AND event_name = @event";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@control", controlName);
        cmd.Parameters.AddWithValue("@event", eventName);
        cmd.ExecuteNonQuery();
    }
    
    public void DeleteAllScripts(string controlName)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "DELETE FROM scripts WHERE control_name = @control";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@control", controlName);
        cmd.ExecuteNonQuery();
        
        Console.WriteLine($"🗑️ Deleted all scripts for {controlName}");
    }
    
    // ========== GLOBAL VARIABLE METHODS ==========
    
    public void SetGlobal(string varName, string varValue, string varType = "string")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = @"
            INSERT OR REPLACE INTO global_vars (var_name, var_value, var_type, updated_at)
            VALUES (@name, @value, @type, CURRENT_TIMESTAMP)";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", varName);
        cmd.Parameters.AddWithValue("@value", varValue);
        cmd.Parameters.AddWithValue("@type", varType);
        cmd.ExecuteNonQuery();
    }
    
    public string? GetGlobal(string varName)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "SELECT var_value FROM global_vars WHERE var_name = @name";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", varName);
        
        var result = cmd.ExecuteScalar();
        return result?.ToString();
    }
    
    public Dictionary<string, string> GetAllGlobals()
    {
        var globals = new Dictionary<string, string>();
        
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "SELECT var_name, var_value FROM global_vars";
        
        using var cmd = new SQLiteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            globals[reader.GetString(0)] = reader.GetString(1);
        }
        
        return globals;
    }
    
    public void DeleteGlobal(string varName)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = "DELETE FROM global_vars WHERE var_name = @name";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", varName);
        cmd.ExecuteNonQuery();
    }
    
    // ========== LOGGING METHODS ==========
    
    public void LogExecution(string controlName, string eventName, bool success, string? errorMessage = null)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = @"
            INSERT INTO execution_log (control_name, event_name, success, error_message)
            VALUES (@control, @event, @success, @error)";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@control", controlName);
        cmd.Parameters.AddWithValue("@event", eventName);
        cmd.Parameters.AddWithValue("@success", success ? 1 : 0);
        cmd.Parameters.AddWithValue("@error", errorMessage ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }
    
    public List<(string, string, string, bool)> GetRecentExecutions(int limit = 50)
    {
        var executions = new List<(string, string, string, bool)>();
        
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        var sql = @"
            SELECT control_name, event_name, executed_at, success 
            FROM execution_log 
            ORDER BY id DESC 
            LIMIT @limit";
        
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@limit", limit);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            executions.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3) == 1
            ));
        }
        
        return executions;
    }
}

