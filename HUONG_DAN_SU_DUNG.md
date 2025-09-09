# 📚 HƯỚNG DẪN SỬ DỤNG ANTICHEAT DLL & SHARPMONO INJECTOR

## 🎯 Tổng Quan
Hướng dẫn này sẽ giúp bạn sử dụng AntiCheat.dll để inject vào game Unity/Mono thông qua SharpMono Injector GUI.

---

## 📁 Cấu Trúc Files

```
workspace/
├── AntiCheatDLL/
│   └── bin/Release/netstandard2.0/
│       └── AntiCheat.dll          ← DLL chính để inject
├── SharpMonoInjectorGUI/
│   ├── SharpMonoInjector.exe      ← Tool inject GUI
│   └── [Source files]
└── HUONG_DAN_SU_DUNG.md          ← File này
```

---

## 🚀 CÁCH SỬ DỤNG NHANH

### Bước 1: Chuẩn Bị
1. **Copy AntiCheat.dll** từ `/workspace/AntiCheatDLL/bin/Release/netstandard2.0/` ra một thư mục dễ tìm (ví dụ: `C:\AntiCheat.dll`)
2. **Build SharpMonoInjector GUI** (xem phần Build bên dưới)
3. **Chạy game Unity/Mono** mà bạn muốn inject

### Bước 2: Inject DLL
1. **Chạy SharpMonoInjector.exe với quyền Administrator** (Chuột phải → Run as Administrator)
2. **Chọn process game** từ danh sách (tìm process có dấu ✓ ở cột Mono)
3. **Thiết lập thông tin inject:**
   - DLL Path: `C:\AntiCheat.dll`
   - Namespace: `AntiCheatSystem`
   - Class: `AntiCheatBootstrap`
   - Method: `Initialize`
4. **Click nút "💉 INJECT DLL"**

---

## 📝 HƯỚNG DẪN CHI TIẾT

### 1️⃣ **Build SharpMono Injector GUI**

```bash
# Di chuyển đến thư mục project
cd /workspace/SharpMonoInjectorGUI

# Tạo project file
cat > SharpMonoInjectorGUI.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>
</Project>
EOF

# Build ứng dụng
dotnet build -c Release
```

### 2️⃣ **Cách Inject Thủ Công (Không dùng GUI)**

Nếu bạn muốn inject thủ công, có thể dùng các tool sau:

#### **Option A: Dùng Process Hacker**
1. Mở Process Hacker với quyền Admin
2. Tìm process game Unity
3. Chuột phải → Miscellaneous → Inject DLL
4. Chọn file AntiCheat.dll

#### **Option B: Dùng Cheat Engine**
1. Mở Cheat Engine
2. Attach vào process game
3. Memory View → Tools → Inject DLL
4. Chọn AntiCheat.dll

#### **Option C: Dùng Command Line (PowerShell)**
```powershell
# Script PowerShell để inject
$ProcessName = "GameName"  # Thay bằng tên game
$DllPath = "C:\AntiCheat.dll"

# Lấy process ID
$Process = Get-Process $ProcessName
$ProcessId = $Process.Id

# Inject DLL (cần tool bổ sung như Injector.exe)
.\Injector.exe -p $ProcessId -d $DllPath
```

### 3️⃣ **Kiểm Tra Injection Thành Công**

Sau khi inject, kiểm tra xem DLL đã hoạt động chưa:

#### **Dấu hiệu thành công:**
- ✅ Game không bị crash
- ✅ Không có thông báo lỗi
- ✅ AntiCheat bắt đầu kiểm tra (mỗi 2 giây)

#### **Dấu hiệu thất bại:**
- ❌ Game crash ngay lập tức
- ❌ Thông báo "Access Denied"
- ❌ DLL không xuất hiện trong process modules

### 4️⃣ **Cách AntiCheat Hoạt Động**

Sau khi inject thành công, AntiCheat sẽ:

1. **Kiểm tra định kỳ** (mỗi 2 giây):
   - Quét loaded assemblies
   - Tìm cheat signatures
   - Kiểm tra file/folder đáng ngờ
   - Phát hiện named pipes

2. **Khi phát hiện cheat:**
   - Log cảnh báo ra console
   - Gọi callback OnCheatDetected
   - **TẮT GAME NGAY LẬP TỨC**

---

## ⚠️ LƯU Ý QUAN TRỌNG

### 🔴 **CẢNH BÁO**
1. **DLL này sẽ TẮT GAME nếu phát hiện cheat** - Sử dụng cẩn thận!
2. **Cần quyền Administrator** để inject vào process khác
3. **Chỉ hoạt động trên Windows** (sử dụng Windows API)
4. **Có thể bị antivirus phát hiện** là tool inject (false positive)

### 🟡 **Khuyến Nghị**
1. **Test trên game offline/test environment trước**
2. **Backup game saves** trước khi test
3. **Tắt antivirus tạm thời** nếu bị chặn
4. **Không sử dụng trên game online** (có thể bị ban)

### 🟢 **Tips**
1. Inject ngay sau khi game khởi động (trước main menu)
2. Nếu inject fail, thử chạy với compatibility mode
3. Một số game cần inject vào launcher thay vì game chính

---

## 🛠️ TROUBLESHOOTING

### **Lỗi: "Failed to open target process"**
**Nguyên nhân:** Không có quyền Admin
**Giải pháp:** Chạy injector với quyền Administrator

### **Lỗi: "Mono runtime not found"**
**Nguyên nhân:** Game không phải Unity/Mono
**Giải pháp:** Chỉ sử dụng với game Unity

### **Lỗi: "DLL file not found"**
**Nguyên nhân:** Đường dẫn DLL sai
**Giải pháp:** Kiểm tra lại path, copy DLL ra C:\ để dễ truy cập

### **Lỗi: Game crash sau inject**
**Nguyên nhân:** Version không tương thích
**Giải pháp:** 
- Kiểm tra Unity version của game
- Build lại DLL với target framework phù hợp
- Thử inject sau khi game load xong

---

## 📊 Thông Tin Kỹ Thuật

### **AntiCheat.dll Specifications:**
- **Framework:** .NET Standard 2.0
- **Size:** ~8.7 KB
- **Dependencies:** System libraries only
- **Platform:** Windows x86/x64

### **Detection Methods:**
```csharp
// Các phương thức phát hiện:
1. Assembly Scanning    // Quét DLL injection
2. File System Check   // Kiểm tra file cheat
3. Named Pipe Detection // Phát hiện IPC
4. Hook Detection      // Phát hiện API hooks
```

### **Các Cheat Được Phát Hiện:**
- ✓ BobHSSJJ cheat framework
- ✓ MonoMod.RuntimeDetour
- ✓ Mono.Cecil IL manipulation
- ✓ Known cheat files/folders
- ✓ Named pipe "A9CC91EDA92B"

---

## 💻 CODE MẪU INJECT

### **C# Console App Inject:**
```csharp
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class SimpleInjector
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr addr, 
        uint size, uint allocType, uint protect);
    
    [DllImport("kernel32.dll")]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr addr, 
        byte[] buffer, uint size, out IntPtr written);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr attr, 
        uint stack, IntPtr start, IntPtr param, uint flags, out IntPtr threadId);

    static void Main()
    {
        Console.Write("Enter game process name: ");
        string processName = Console.ReadLine();
        
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0)
        {
            Console.WriteLine("Process not found!");
            return;
        }
        
        var targetProcess = processes[0];
        string dllPath = @"C:\AntiCheat.dll";
        
        // Open process
        IntPtr hProcess = OpenProcess(0x1F0FFF, false, targetProcess.Id);
        
        // Allocate memory for DLL path
        IntPtr allocMem = VirtualAllocEx(hProcess, IntPtr.Zero, 
            (uint)dllPath.Length, 0x1000 | 0x2000, 0x04);
        
        // Write DLL path
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
        IntPtr written;
        WriteProcessMemory(hProcess, allocMem, bytes, (uint)bytes.Length, out written);
        
        // Get LoadLibraryA address
        IntPtr kernel32 = GetModuleHandle("kernel32.dll");
        IntPtr loadLibAddr = GetProcAddress(kernel32, "LoadLibraryA");
        
        // Create remote thread
        IntPtr threadId;
        CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibAddr, 
            allocMem, 0, out threadId);
        
        Console.WriteLine("Injection completed!");
    }
}
```

---

## 📞 SUPPORT

Nếu gặp vấn đề khi sử dụng:
1. Kiểm tra lại các bước trong hướng dẫn
2. Đảm bảo đã chạy với quyền Administrator
3. Kiểm tra game có phải Unity/Mono không
4. Thử với game khác để test

---

## ⚖️ DISCLAIMER

**LƯU Ý PHÁP LÝ:**
- Tool này chỉ dành cho mục đích **học tập và nghiên cứu**
- **KHÔNG sử dụng để gian lận** trong game online
- **KHÔNG sử dụng để phá hoại** hay gây thiệt hại
- Người dùng **tự chịu trách nhiệm** về việc sử dụng

---

## 🎮 GAME TƯƠNG THÍCH

### ✅ **Hoạt động tốt với:**
- Unity 2018.x - 2023.x games
- Mono-based games
- IL2CPP games (với modification)

### ❌ **Không hoạt động với:**
- Unreal Engine games
- Native C++ games
- Games với anti-cheat mạnh (EAC, BattlEye)

---

## 📈 NÂNG CAO

### **Tùy chỉnh AntiCheat:**
Bạn có thể chỉnh sửa file `AntiCheatStandalone.cs` để:
- Thay đổi interval kiểm tra (mặc định 2 giây)
- Thêm/bớt signatures cần kiểm tra
- Tùy chỉnh hành động khi phát hiện cheat
- Thêm logging chi tiết hơn

### **Build với Unity Dependencies:**
Nếu cần version đầy đủ cho Unity:
1. Download Unity DLLs
2. Thêm references trong project
3. Build với Unity-specific code

---

*Cập nhật lần cuối: 2024*
*Version: 1.0.0*