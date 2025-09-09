# SharpMono Quick Injector Script
# Run with Administrator privileges

param(
    [Parameter(Mandatory=$false)]
    [string]$ProcessName = "",
    
    [Parameter(Mandatory=$false)]
    [string]$DllPath = "C:\AntiCheat.dll"
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "     SHARPMONO QUICK INJECTOR SCRIPT" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[ERROR] This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if DLL exists
if (-not (Test-Path $DllPath)) {
    Write-Host "[ERROR] DLL not found at: $DllPath" -ForegroundColor Red
    $DllPath = Read-Host "Enter the full path to AntiCheat.dll"
    if (-not (Test-Path $DllPath)) {
        Write-Host "[ERROR] Invalid DLL path!" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
}

Write-Host "[OK] DLL found: $DllPath" -ForegroundColor Green

# If no process name provided, show list
if ($ProcessName -eq "") {
    Write-Host ""
    Write-Host "Available Unity/Mono processes:" -ForegroundColor Yellow
    Write-Host "--------------------------------"
    
    $unityProcesses = @()
    Get-Process | ForEach-Object {
        try {
            $modules = $_.Modules | Where-Object { 
                $_.ModuleName -like "*mono*" -or 
                $_.ModuleName -like "*unity*" -or
                $_.ModuleName -eq "GameAssembly.dll"
            }
            if ($modules) {
                $unityProcesses += $_
                Write-Host "  [$($_.Id)] $($_.ProcessName)" -ForegroundColor Cyan
                if ($_.MainWindowTitle) {
                    Write-Host "        Window: $($_.MainWindowTitle)" -ForegroundColor Gray
                }
            }
        } catch {}
    }
    
    if ($unityProcesses.Count -eq 0) {
        Write-Host "  No Unity/Mono processes found!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Showing all processes instead:" -ForegroundColor Yellow
        Get-Process | Where-Object { $_.MainWindowTitle -ne "" } | 
            Format-Table Id, ProcessName, MainWindowTitle -AutoSize
    }
    
    Write-Host ""
    $ProcessName = Read-Host "Enter process name or PID to inject into"
}

# Get the target process
$targetProcess = $null
if ($ProcessName -match '^\d+$') {
    # Input is a PID
    $targetProcess = Get-Process -Id $ProcessName -ErrorAction SilentlyContinue
} else {
    # Input is a process name
    $targetProcess = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue | Select-Object -First 1
}

if (-not $targetProcess) {
    Write-Host "[ERROR] Process not found: $ProcessName" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "[OK] Target process: $($targetProcess.ProcessName) (PID: $($targetProcess.Id))" -ForegroundColor Green

# Inject using .NET reflection (simple method)
Write-Host ""
Write-Host "Attempting injection..." -ForegroundColor Yellow

try {
    # Create injector code
    $injectorCode = @'
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class Injector {
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
    
    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
    
    public static bool Inject(int processId, string dllPath) {
        IntPtr hProcess = OpenProcess(0x1F0FFF, false, processId);
        if (hProcess == IntPtr.Zero) return false;
        
        IntPtr allocMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllPath.Length, 0x1000 | 0x2000, 0x04);
        if (allocMem == IntPtr.Zero) return false;
        
        byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
        IntPtr written;
        if (!WriteProcessMemory(hProcess, allocMem, bytes, (uint)bytes.Length, out written)) return false;
        
        IntPtr kernel32 = GetModuleHandle("kernel32.dll");
        IntPtr loadLibAddr = GetProcAddress(kernel32, "LoadLibraryA");
        
        IntPtr thread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibAddr, allocMem, 0, IntPtr.Zero);
        return thread != IntPtr.Zero;
    }
}
'@

    # Compile and run injector
    Add-Type -TypeDefinition $injectorCode
    $result = [Injector]::Inject($targetProcess.Id, $DllPath)
    
    if ($result) {
        Write-Host "[SUCCESS] DLL injected successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "AntiCheat is now active in the target process." -ForegroundColor Cyan
        Write-Host "The game will terminate if any cheats are detected." -ForegroundColor Yellow
    } else {
        Write-Host "[ERROR] Injection failed!" -ForegroundColor Red
        Write-Host "Make sure you have administrator privileges." -ForegroundColor Yellow
    }
} catch {
    Write-Host "[ERROR] Exception during injection: $_" -ForegroundColor Red
}

Write-Host ""
Read-Host "Press Enter to exit"