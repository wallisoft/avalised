#!/bin/bash
cd ~/Downloads/vb/VB

cat > SaveCurrentState.cs << 'SAVESTATE'
using System;
using System.IO;

namespace VB;

class SaveCurrentState
{
    static void Main()
    {
        var db = new DatabaseManager();
        
        Console.WriteLine("[SAVE] Saving current project state...");
        
        // Save MainWindow.axaml.cs
        if (File.Exists("MainWindow.axaml.cs"))
        {
            var content = File.ReadAllText("MainWindow.axaml.cs");
            db.SaveFile("MainWindow.axaml.cs", content, "cs");
            Console.WriteLine("  ✓ MainWindow.axaml.cs");
        }
        
        // Save designer.vml
        if (File.Exists("designer.vml"))
        {
            var content = File.ReadAllText("designer.vml");
            db.SaveFile("designer.vml", content, "vml");
            Console.WriteLine("  ✓ designer.vml");
        }
        
        // Save DesignerWindow.cs
        if (File.Exists("DesignerWindow.cs"))
        {
            var content = File.ReadAllText("DesignerWindow.cs");
            db.SaveFile("DesignerWindow.cs", content, "cs");
            Console.WriteLine("  ✓ DesignerWindow.cs");
        }
        
        // Save DatabaseManager.cs
        if (File.Exists("DatabaseManager.cs"))
        {
            var content = File.ReadAllText("DatabaseManager.cs");
            db.SaveFile("DatabaseManager.cs", content, "cs");
            Console.WriteLine("  ✓ DatabaseManager.cs");
        }
        
        Console.WriteLine($"\n[SAVE] All files saved to: {db.GetDbPath()}");
        db.ListFiles();
    }
}
SAVESTATE

dotnet run --project . SaveCurrentState.cs
