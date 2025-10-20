using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace ConfigUI.Services;

public class ScriptEngine
{
    private readonly Dictionary<string, Func<string>> _getters = new();
    private readonly Dictionary<string, Action<string>> _setters = new();
    
    public void RegisterControl(string name, Func<string> getter, Action<string> setter)
    {
        _getters[name] = getter;
        _setters[name] = setter;
    }
    
    public async Task<string> ExecuteScriptAsync(string script)
    {
        // Create helper functions script
        var helperFunctions = @"
get_control() {
    echo ""GET_CONTROL:$1""
}

set_control() {
    echo ""SET_CONTROL:$1:$2""
}

enable_control() {
    echo ""ENABLE_CONTROL:$1""
}

disable_control() {
    echo ""DISABLE_CONTROL:$1""
}

show_message() {
    echo ""SHOW_MESSAGE:$1""
}

validate() {
    echo ""VALIDATE:$1:$2""
}
";
        
        var fullScript = helperFunctions + "\n" + script;
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{fullScript.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        // Parse and execute commands
        ProcessScriptOutput(output);
        
        return output;
    }
    
    private void ProcessScriptOutput(string output)
    {
        var lines = output.Split('\n');
        
        foreach (var line in lines)
        {
            if (line.StartsWith("GET_CONTROL:"))
            {
                var controlName = line.Substring("GET_CONTROL:".Length).Trim();
                if (_getters.TryGetValue(controlName, out var getter))
                {
                    Console.WriteLine(getter());
                }
            }
            else if (line.StartsWith("SET_CONTROL:"))
            {
                var parts = line.Substring("SET_CONTROL:".Length).Split(':', 2);
                if (parts.Length == 2 && _setters.TryGetValue(parts[0], out var setter))
                {
                    setter(parts[1]);
                }
            }
            else if (line.StartsWith("SHOW_MESSAGE:"))
            {
                var message = line.Substring("SHOW_MESSAGE:".Length);
                // TODO: Show message dialog
                Console.WriteLine($"MESSAGE: {message}");
            }
        }
    }
}
