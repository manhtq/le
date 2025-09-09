using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpMonoInjectorGUI
{
    public class MonoInjector
    {
        // Windows API constants
        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        // Windows API imports
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        // Mono exports structure
        [StructLayout(LayoutKind.Sequential)]
        private struct MonoExports
        {
            public IntPtr mono_get_root_domain;
            public IntPtr mono_thread_attach;
            public IntPtr mono_assembly_open;
            public IntPtr mono_assembly_get_image;
            public IntPtr mono_class_from_name;
            public IntPtr mono_class_get_method_from_name;
            public IntPtr mono_runtime_invoke;
            public IntPtr mono_domain_assembly_open;
            public IntPtr mono_image_get_assembly;
        }

        public bool Inject(Process targetProcess, string dllPath, string namespaceName, string className, string methodName)
        {
            IntPtr hProcess = IntPtr.Zero;
            IntPtr allocatedMem = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                // Open target process
                hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
                if (hProcess == IntPtr.Zero)
                {
                    throw new Exception("Failed to open target process. Make sure you have administrator privileges.");
                }

                // Find mono.dll or Unity's mono module
                ProcessModule monoModule = FindMonoModule(targetProcess);
                if (monoModule == null)
                {
                    throw new Exception("Mono runtime not found in target process.");
                }

                // Prepare injection data
                var injectionData = PrepareInjectionData(dllPath, namespaceName, className, methodName);
                
                // Allocate memory in target process
                allocatedMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)injectionData.Length, 
                    MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (allocatedMem == IntPtr.Zero)
                {
                    throw new Exception("Failed to allocate memory in target process.");
                }

                // Write injection data to target process
                IntPtr bytesWritten;
                if (!WriteProcessMemory(hProcess, allocatedMem, injectionData, (uint)injectionData.Length, out bytesWritten))
                {
                    throw new Exception("Failed to write injection data to target process.");
                }

                // Get address of injection function
                IntPtr injectionFunc = GetInjectionFunctionAddress(monoModule);
                if (injectionFunc == IntPtr.Zero)
                {
                    // Fallback: Use LoadLibrary for simple DLL injection
                    return InjectUsingLoadLibrary(hProcess, dllPath);
                }

                // Create remote thread to execute injection
                IntPtr threadId;
                hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, injectionFunc, allocatedMem, 0, out threadId);
                if (hThread == IntPtr.Zero)
                {
                    throw new Exception("Failed to create remote thread in target process.");
                }

                // Wait for injection to complete
                WaitForSingleObject(hThread, 5000); // 5 second timeout

                // Check if injection succeeded
                uint exitCode;
                GetExitCodeThread(hThread, out exitCode);
                
                return exitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Injection error: {ex.Message}");
                return false;
            }
            finally
            {
                // Cleanup
                if (hThread != IntPtr.Zero) CloseHandle(hThread);
                if (allocatedMem != IntPtr.Zero && hProcess != IntPtr.Zero)
                {
                    VirtualFreeEx(hProcess, allocatedMem, 0, 0x8000); // MEM_RELEASE
                }
                if (hProcess != IntPtr.Zero) CloseHandle(hProcess);
            }
        }

        private ProcessModule FindMonoModule(Process process)
        {
            try
            {
                foreach (ProcessModule module in process.Modules)
                {
                    string moduleName = module.ModuleName.ToLower();
                    if (moduleName.Contains("mono") || 
                        moduleName == "unityplayer.dll" ||
                        moduleName == "gameassembly.dll")
                    {
                        return module;
                    }
                }
            }
            catch { }
            return null;
        }

        private byte[] PrepareInjectionData(string dllPath, string namespaceName, string className, string methodName)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Write strings with null terminators
                writer.Write(Encoding.UTF8.GetBytes(dllPath + "\0"));
                writer.Write(Encoding.UTF8.GetBytes(namespaceName + "\0"));
                writer.Write(Encoding.UTF8.GetBytes(className + "\0"));
                writer.Write(Encoding.UTF8.GetBytes(methodName + "\0"));
                
                return stream.ToArray();
            }
        }

        private IntPtr GetInjectionFunctionAddress(ProcessModule monoModule)
        {
            // Try to get mono injection functions
            IntPtr hModule = GetModuleHandle(monoModule.ModuleName);
            if (hModule != IntPtr.Zero)
            {
                // Look for mono_inject or similar export
                IntPtr funcAddr = GetProcAddress(hModule, "mono_inject");
                if (funcAddr != IntPtr.Zero) return funcAddr;
                
                funcAddr = GetProcAddress(hModule, "mono_runtime_invoke");
                if (funcAddr != IntPtr.Zero) return funcAddr;
            }
            return IntPtr.Zero;
        }

        private bool InjectUsingLoadLibrary(IntPtr hProcess, string dllPath)
        {
            IntPtr allocatedMem = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                // Get LoadLibraryA address
                IntPtr kernel32 = GetModuleHandle("kernel32.dll");
                IntPtr loadLibraryAddr = GetProcAddress(kernel32, "LoadLibraryA");
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    throw new Exception("Failed to get LoadLibraryA address.");
                }

                // Allocate memory for DLL path
                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + "\0");
                allocatedMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllPathBytes.Length, 
                    MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (allocatedMem == IntPtr.Zero)
                {
                    throw new Exception("Failed to allocate memory for DLL path.");
                }

                // Write DLL path to target process
                IntPtr bytesWritten;
                if (!WriteProcessMemory(hProcess, allocatedMem, dllPathBytes, (uint)dllPathBytes.Length, out bytesWritten))
                {
                    throw new Exception("Failed to write DLL path to target process.");
                }

                // Create remote thread to call LoadLibraryA
                IntPtr threadId;
                hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocatedMem, 0, out threadId);
                if (hThread == IntPtr.Zero)
                {
                    throw new Exception("Failed to create remote thread for LoadLibrary.");
                }

                // Wait for DLL to load
                WaitForSingleObject(hThread, 5000);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (hThread != IntPtr.Zero) CloseHandle(hThread);
                if (allocatedMem != IntPtr.Zero)
                {
                    VirtualFreeEx(hProcess, allocatedMem, 0, 0x8000);
                }
            }
        }
    }
}