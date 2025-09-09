using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

// Simple version without namespace for better compatibility
public class Loader
{
    private static bool _initialized = false;
    
    // Main entry point - this is what the injector will call
    public static void Load()
    {
        if (_initialized) return;
        _initialized = true;
        
        try
        {
            // Start AntiCheat
            AntiCheatCore.Instance.Start();
            LogMessage("[AntiCheat] Loaded successfully!");
        }
        catch (Exception ex)
        {
            LogMessage($"[AntiCheat] Load error: {ex.Message}");
        }
    }
    
    // Alternative entry points for different injectors
    public static void Init() => Load();
    public static void Initialize() => Load();
    public static void Start() => Load();
    public static void Main() => Load();
    public static void Inject() => Load();
    
    // Simple logging
    private static void LogMessage(string message)
    {
        try
        {
            Console.WriteLine(message);
            File.AppendAllText(@"C:\anticheat_log.txt", $"{DateTime.Now}: {message}\n");
        }
        catch { }
    }
}

// Main AntiCheat logic
public class AntiCheatCore
{
    private static AntiCheatCore _instance;
    public static AntiCheatCore Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AntiCheatCore();
            return _instance;
        }
    }
    
    private System.Threading.Timer _checkTimer;
    private HashSet<string> _detectedFlags = new HashSet<string>();
    
    public void Start()
    {
        LogToFile("[AntiCheat] Starting protection...");
        
        // Initial check
        PerformChecks();
        
        // Setup periodic checking (every 2 seconds)
        _checkTimer = new System.Threading.Timer(
            callback: _ => PerformChecks(),
            state: null,
            dueTime: TimeSpan.FromSeconds(2),
            period: TimeSpan.FromSeconds(2)
        );
        
        LogToFile("[AntiCheat] Protection active!");
    }
    
    private void PerformChecks()
    {
        try
        {
            CheckLoadedAssemblies();
            CheckKnownFiles();
            CheckProcesses();
        }
        catch (Exception ex)
        {
            LogToFile($"[AntiCheat] Check error: {ex.Message}");
        }
    }
    
    private void CheckLoadedAssemblies()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                try
                {
                    string name = asm.GetName().Name;
                    if (string.IsNullOrEmpty(name)) continue;
                    
                    // Check for known cheat signatures
                    string[] blacklist = {
                        "BobHSSJJ",
                        "MonoMod",
                        "RuntimeDetour",
                        "Mono.Cecil",
                        "CheatEngine",
                        "Trainer",
                        "Hack",
                        "Cheat"
                    };
                    
                    foreach (var banned in blacklist)
                    {
                        if (name.IndexOf(banned, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            OnCheatDetected($"Suspicious assembly detected: {name}");
                        }
                    }
                }
                catch { }
            }
        }
        catch { }
    }
    
    private void CheckKnownFiles()
    {
        string[] suspiciousFiles = {
            @"C:\UI.dll",
            @"D:\ssjj\",
            @"D:\c\log.log",
            @"C:\Windows\Temp\cheat.dll"
        };
        
        foreach (var path in suspiciousFiles)
        {
            try
            {
                if (File.Exists(path) || Directory.Exists(path))
                {
                    OnCheatDetected($"Suspicious file/folder found: {path}");
                }
            }
            catch { }
        }
    }
    
    private void CheckProcesses()
    {
        string[] bannedProcesses = {
            "cheatengine",
            "x64dbg",
            "ollydbg",
            "ida",
            "processhacker",
            "dnspy"
        };
        
        try
        {
            var processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    string procName = proc.ProcessName.ToLower();
                    foreach (var banned in bannedProcesses)
                    {
                        if (procName.Contains(banned))
                        {
                            OnCheatDetected($"Banned process detected: {proc.ProcessName}");
                        }
                    }
                }
                catch { }
            }
        }
        catch { }
    }
    
    private void OnCheatDetected(string reason)
    {
        if (_detectedFlags.Contains(reason)) return;
        _detectedFlags.Add(reason);
        
        LogToFile($"[DETECTION] {reason}");
        
        // Terminate after 1 second delay
        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            System.Threading.Thread.Sleep(1000);
            TerminateNow();
        });
    }
    
    private void TerminateNow()
    {
        try { Environment.Exit(0); } catch { }
        try { Process.GetCurrentProcess().Kill(); } catch { }
    }
    
    private void LogToFile(string message)
    {
        try
        {
            string logPath = @"C:\anticheat_log.txt";
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            Console.WriteLine(message);
        }
        catch { }
    }
}

// Additional entry class for compatibility
public static class AntiCheat
{
    public static void Load() => Loader.Load();
    public static void Init() => Loader.Load();
    public static void Initialize() => Loader.Load();
    public static void Start() => Loader.Load();
}

// Entry point without namespace
public class Main
{
    public static void Load() => Loader.Load();
    public static void Init() => Loader.Load();
    public static void Entry() => Loader.Load();
}