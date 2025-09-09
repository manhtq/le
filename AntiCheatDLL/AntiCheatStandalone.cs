using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AntiCheatSystem
{
    /// <summary>
    /// Standalone AntiCheat class that can work without Unity dependencies
    /// </summary>
    public class AntiCheatStandalone
    {
        public float checkIntervalSeconds = 2.0f;
        public bool logDetectionsToConsole = true;
        public Action<string> OnCheatDetected;

        private DateTime _nextCheckAt;
        private readonly HashSet<string> _onceFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Random _random = new Random();
        private Timer _checkTimer;

        public AntiCheatStandalone()
        {
            _nextCheckAt = DateTime.Now.AddSeconds(_random.NextDouble() * 0.75 + 0.25);
        }

        public void Start()
        {
            _checkTimer = new Timer(
                callback: _ => PerformChecks(),
                state: null,
                dueTime: TimeSpan.FromSeconds(1),
                period: TimeSpan.FromSeconds(Math.Max(0.25f, checkIntervalSeconds))
            );
        }

        public void Stop()
        {
            _checkTimer?.Dispose();
        }

        private void PerformChecks()
        {
            try
            {
                CheckLoadedAssemblies();
                CheckKnownArtifacts();
                CheckNamedPipe();
                CheckDetourPresence();
            }
            catch (Exception)
            {
                // Swallow to avoid impacting application; optionally report internally.
            }
        }

        private void CheckLoadedAssemblies()
        {
            var loaded = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in loaded)
            {
                string name = SafeGetName(asm);
                if (string.IsNullOrEmpty(name)) continue;

                // Signatures from the analyzed cheat
                if (name.IndexOf("BobHSSJJ", StringComparison.OrdinalIgnoreCase) >= 0)
                    FlagOnce("asm:BobHSSJJ", "Loaded suspicious assembly: " + name);
                if (name.IndexOf("MonoMod.RuntimeDetour", StringComparison.OrdinalIgnoreCase) >= 0)
                    FlagOnce("asm:RuntimeDetour", "Detour framework present: " + name);
                if (name.IndexOf("Mono.Cecil", StringComparison.OrdinalIgnoreCase) >= 0)
                    FlagOnce("asm:Mono.Cecil", "IL tooling present: " + name);
            }
        }

        private void CheckKnownArtifacts()
        {
            // Hard-coded artifacts used by the cheat
            TryFlagFile("C:\\UI.dll", "Found injected UI DLL at C:\\UI.dll");
            TryFlagDirectory("D:\\ssjj", "Found cheat log directory D:\\ssjj");
            TryFlagFile("D:\\c\\log.log", "Found cheat log file D:\\c\\log.log");
        }

        private void CheckNamedPipe()
        {
            const string pipeName = "A9CC91EDA92B";
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string fullName = "\\\\.\\pipe\\" + pipeName;
                    if (NativeMethods.WaitNamedPipe(fullName, 1))
                    {
                        FlagOnce("pipe:" + pipeName, "Named pipe server detected: " + pipeName);
                    }
                }
                catch { }
            });
        }

        private void CheckDetourPresence()
        {
            try
            {
                var runtimeDetour = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => SafeGetName(a).IndexOf("MonoMod.RuntimeDetour", StringComparison.OrdinalIgnoreCase) >= 0);
                if (runtimeDetour != null)
                {
                    var hookType = runtimeDetour.GetType("MonoMod.RuntimeDetour.Hook");
                    if (hookType != null)
                    {
                        FlagOnce("type:Hook", "RuntimeDetour Hook type available");
                    }
                }
            }
            catch { }
        }

        private void TryFlagFile(string path, string message)
        {
            try
            {
                if (File.Exists(path))
                {
                    FlagOnce("file:" + path, message);
                }
            }
            catch { }
        }

        private void TryFlagDirectory(string path, string message)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    FlagOnce("dir:" + path, message);
                }
            }
            catch { }
        }

        private void FlagOnce(string key, string message)
        {
            if (_onceFlags.Add(key))
            {
                if (logDetectionsToConsole)
                {
                    Console.WriteLine("[AntiCheat] " + message);
                }
                try { OnCheatDetected?.Invoke(message); } catch { }
                TerminateNow();
            }
        }

        private void TerminateNow()
        {
            try { Environment.Exit(0); } catch { }
            try { Process.GetCurrentProcess()?.Kill(); } catch { }
        }

        private static string SafeGetName(Assembly asm)
        {
            try { return asm.GetName().Name ?? string.Empty; } catch { return string.Empty; }
        }
    }

    internal static class NativeMethods
    {
        private const uint NMPWAIT_NOWAIT = 0x00000001;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool WaitNamedPipe(string lpNamedPipeName, uint nTimeOut);
    }

    /// <summary>
    /// Bootstrap class for initializing AntiCheat
    /// </summary>
    public static class AntiCheatBootstrap
    {
        private static AntiCheatStandalone _instance;

        public static void Initialize()
        {
            if (_instance != null) return;

            try
            {
                _instance = new AntiCheatStandalone();
                _instance.OnCheatDetected += msg => 
                { 
                    Console.WriteLine("[AntiCheat] Detected: " + msg); 
                };
                _instance.Start();
            }
            catch { }
        }

        public static void Shutdown()
        {
            _instance?.Stop();
            _instance = null;
        }
    }
}