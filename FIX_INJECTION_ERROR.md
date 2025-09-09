# 🔧 SỬA LỖI "mono_class_from_name() returned NULL"

## ❌ LỖI GẶP PHẢI
```
Injection failed: mono_class_from_name() returned NULL
```

## 🎯 NGUYÊN NHÂN
Lỗi này xảy ra khi Mono runtime không thể tìm thấy class trong DLL do:
1. Namespace không đúng hoặc không tồn tại
2. Class name không public hoặc bị obfuscate
3. DLL không tương thích với version Unity/Mono của game
4. Injector gọi sai method của Mono

---

## ✅ GIẢI PHÁP

### 📦 **1. SỬ DỤNG DLL MỚI (RECOMMENDED)**

Tôi đã tạo **AntiCheatSimple.dll** mới với các cải tiến:
- **KHÔNG có namespace** (tránh lỗi namespace)
- **Multiple entry points** (nhiều method để thử)
- **Target .NET 3.5** (tương thích Unity cũ)

**File mới:** `/workspace/AntiCheatDLL/bin/Release/net35/AntiCheatSimple.dll`

---

### 🎮 **2. CÁCH INJECT DLL MỚI**

#### **Option A: Dùng Simple Injector (Dễ nhất)**
```cmd
# Build SimpleInjector
csc SimpleInjector.cs

# Run
SimpleInjector.exe
```

Chỉ cần:
1. Nhập tên process game
2. Nhập path đến AntiCheatSimple.dll
3. Enter để inject

#### **Option B: Dùng SharpMono với thông tin mới**
```
DLL Path:    C:\AntiCheatSimple.dll
Namespace:   (để trống hoặc gõ dấu cách)
Class:       Loader
Method:      Load
```

**Hoặc thử các combinations sau:**
| Namespace | Class | Method |
|-----------|-------|--------|
| (empty) | Loader | Load |
| (empty) | Loader | Init |
| (empty) | Main | Load |
| (empty) | AntiCheat | Load |

---

### 🛠️ **3. INJECT THỦ CÔNG VỚI POWERSHELL**

```powershell
# Simple LoadLibrary injection
$code = @'
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

public static void Inject(int pid, string dll) {
    IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
    IntPtr loadLibraryA = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
    IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dll.Length, 0x3000, 0x40);
    IntPtr bytesWritten;
    WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.Default.GetBytes(dll), (uint)dll.Length, out bytesWritten);
    CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryA, allocMemAddress, 0, IntPtr.Zero);
}
'@

Add-Type -TypeDefinition $code -Name Injector -Namespace Tool
$proc = Get-Process "YourGameName" # Thay tên game
[Tool.Injector]::Inject($proc.Id, "C:\AntiCheatSimple.dll")
Write-Host "Injected!" -ForegroundColor Green
```

---

### 🔍 **4. KIỂM TRA DLL ĐÃ LOAD CHƯA**

#### **Dùng Process Explorer:**
1. Download Process Explorer từ Microsoft
2. Tìm process game
3. View → Lower Pane View → DLLs
4. Check xem AntiCheatSimple.dll có trong list không

#### **Check log file:**
```powershell
# AntiCheat tạo log tại:
Get-Content C:\anticheat_log.txt -Tail 10
```

---

## 📝 **TROUBLESHOOTING CHECKLIST**

### ✅ **Kiểm tra các điều sau:**

1. **[ ] Chạy với quyền Administrator**
   ```cmd
   # Right-click → Run as Administrator
   ```

2. **[ ] Game là Unity/Mono**
   ```powershell
   # Check trong Task Manager → Details
   # Tìm mono.dll hoặc UnityPlayer.dll trong modules
   ```

3. **[ ] Dùng đúng architecture**
   - Game 32-bit → Dùng injector 32-bit
   - Game 64-bit → Dùng injector 64-bit

4. **[ ] Inject timing đúng**
   - Best: Ngay sau khi game start (splash screen)
   - OK: Ở main menu
   - Bad: Trong gameplay

5. **[ ] DLL path chính xác**
   ```powershell
   # Copy to C:\ để đơn giản
   Copy-Item AntiCheatSimple.dll C:\
   ```

---

## 🎯 **PHƯƠNG PHÁP INJECT KHÁC**

### **1. Manual Map Injection**
Tránh được detection nhưng phức tạp hơn

### **2. SetWindowsHookEx Injection**
```csharp
[DllImport("user32.dll")]
static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
```

### **3. Registry Injection (AppInit_DLLs)**
```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows]
"AppInit_DLLs"="C:\\AntiCheatSimple.dll"
"LoadAppInit_DLLs"=dword:00000001
```

---

## 💡 **TIPS CUỐI**

1. **Nếu vẫn lỗi với SharpMono:**
   - Dùng SimpleInjector.exe thay thế
   - Hoặc dùng Cheat Engine để inject

2. **Test với Notepad trước:**
   ```cmd
   SimpleInjector.exe
   > notepad
   > C:\TestDLL.dll
   ```

3. **Nếu game có anti-injection:**
   - Inject vào launcher thay vì game
   - Dùng manual map injection
   - Modify DLL entry point

4. **Debug injection:**
   - Dùng x64dbg attach vào game
   - Set breakpoint tại LoadLibraryA
   - Check xem DLL có được load không

---

## ✅ **TÓM TẮT SOLUTION**

### **Dùng DLL mới + Simple Injector:**
1. Copy `AntiCheatSimple.dll` ra `C:\`
2. Build và chạy `SimpleInjector.exe` với Admin
3. Nhập tên game
4. Path: `C:\AntiCheatSimple.dll`
5. Done!

### **Thông tin inject mới:**
```yaml
DLL: AntiCheatSimple.dll
Namespace: (để trống)
Class: Loader
Method: Load
```

Lỗi `mono_class_from_name() returned NULL` sẽ được fix với DLL mới này!