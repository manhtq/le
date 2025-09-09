using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

class SimpleInjector
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    const int PROCESS_CREATE_THREAD = 0x0002;
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_WRITE = 0x0020;
    const int PROCESS_VM_READ = 0x0010;
    const uint MEM_COMMIT = 0x00001000;
    const uint MEM_RESERVE = 0x00002000;
    const uint PAGE_READWRITE = 4;

    static void Main(string[] args)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("    SIMPLE DLL INJECTOR");
        Console.WriteLine("=================================\n");

        // Get process name
        Console.Write("Enter process name (without .exe): ");
        string processName = Console.ReadLine();
        
        // Find process
        Process[] processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0)
        {
            Console.WriteLine($"[ERROR] Process '{processName}' not found!");
            Console.WriteLine("\nAvailable processes:");
            foreach (var p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.Length > 0)
                    Console.WriteLine($"  - {p.ProcessName} ({p.MainWindowTitle})");
            }
            Console.ReadKey();
            return;
        }

        Process targetProcess = processes[0];
        Console.WriteLine($"[OK] Found process: {targetProcess.ProcessName} (PID: {targetProcess.Id})\n");

        // Get DLL path
        string dllPath = @"C:\AntiCheatSimple.dll";
        if (File.Exists("AntiCheatSimple.dll"))
        {
            dllPath = Path.GetFullPath("AntiCheatSimple.dll");
        }
        
        Console.Write($"DLL path [{dllPath}]: ");
        string input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input))
            dllPath = input;

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"[ERROR] DLL not found at: {dllPath}");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"[OK] Using DLL: {dllPath}\n");

        // Inject
        Console.WriteLine("[*] Starting injection...");
        bool success = InjectDLL(targetProcess.Id, dllPath);
        
        if (success)
        {
            Console.WriteLine("[SUCCESS] DLL injected successfully!");
            Console.WriteLine("\nAntiCheat is now active. Check C:\\anticheat_log.txt for logs.");
        }
        else
        {
            Console.WriteLine("[FAILED] Injection failed!");
            Console.WriteLine("\nTroubleshooting:");
            Console.WriteLine("1. Run as Administrator");
            Console.WriteLine("2. Make sure the game is Unity/Mono based");
            Console.WriteLine("3. Try injecting right after game starts");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static bool InjectDLL(int processId, string dllPath)
    {
        // Get process handle
        IntPtr hProcess = OpenProcess(
            PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | 
            PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, 
            false, processId);
        
        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] Failed to open process. Run as Administrator!");
            return false;
        }

        // Get LoadLibraryA address
        IntPtr kernel32 = GetModuleHandle("kernel32.dll");
        IntPtr loadLibraryAddr = GetProcAddress(kernel32, "LoadLibraryA");
        
        if (loadLibraryAddr == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] Failed to get LoadLibraryA address");
            CloseHandle(hProcess);
            return false;
        }

        // Allocate memory in target process
        byte[] dllBytes = Encoding.Default.GetBytes(dllPath);
        IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, 
            (uint)((dllBytes.Length + 1) * Marshal.SizeOf(typeof(char))), 
            MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        
        if (allocMemAddress == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] Failed to allocate memory in target process");
            CloseHandle(hProcess);
            return false;
        }

        // Write DLL path to allocated memory
        UIntPtr bytesWritten;
        bool writeSuccess = WriteProcessMemory(hProcess, allocMemAddress, 
            dllBytes, (uint)((dllBytes.Length + 1) * Marshal.SizeOf(typeof(char))), 
            out bytesWritten);
        
        if (!writeSuccess)
        {
            Console.WriteLine("[ERROR] Failed to write DLL path to process memory");
            CloseHandle(hProcess);
            return false;
        }

        // Create remote thread to call LoadLibraryA
        IntPtr threadHandle = CreateRemoteThread(hProcess, IntPtr.Zero, 0, 
            loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
        
        if (threadHandle == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] Failed to create remote thread");
            CloseHandle(hProcess);
            return false;
        }

        // Wait for thread to complete
        WaitForSingleObject(threadHandle, 5000); // 5 second timeout
        
        // Cleanup
        CloseHandle(threadHandle);
        CloseHandle(hProcess);
        
        return true;
    }
}