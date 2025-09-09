# AntiCheat DLL

## Tổng quan
Đây là thư viện AntiCheat được biên dịch thành DLL từ mã nguồn C# của bạn. Thư viện này có khả năng phát hiện các cheat tools và modifications trong game.

## File DLL đã tạo
- **AntiCheat.dll** - File DLL chính nằm trong thư mục `bin/Release/netstandard2.0/`
- Kích thước: ~8.7 KB
- Target Framework: .NET Standard 2.0 (tương thích với Unity và .NET Framework/Core)

## Tính năng chính
1. **Kiểm tra Assembly đáng ngờ**: Phát hiện các DLL injection như BobHSSJJ, MonoMod.RuntimeDetour, Mono.Cecil
2. **Kiểm tra File/Thư mục cheat**: Tìm kiếm các file và thư mục cheat đã biết
3. **Kiểm tra Named Pipe**: Phát hiện named pipe server của cheat tools
4. **Kiểm tra Detour/Hook**: Phát hiện runtime detour và hooking frameworks

## Cách sử dụng

### Trong ứng dụng .NET thông thường:
```csharp
using AntiCheatSystem;

// Khởi tạo AntiCheat
AntiCheatBootstrap.Initialize();

// AntiCheat sẽ tự động chạy kiểm tra định kỳ
// Khi phát hiện cheat, ứng dụng sẽ tự động tắt
```

### Trong Unity:
Nếu bạn muốn sử dụng trong Unity, bạn cần:
1. Copy file `AntiCheat.dll` vào thư mục `Assets/Plugins/` của Unity project
2. Sử dụng file `AntiCheat.cs` gốc (với Unity dependencies) thay vì standalone version
3. Unity sẽ tự động khởi tạo AntiCheat khi game chạy

## Cấu trúc Project
```
AntiCheatDLL/
├── AntiCheatStandalone.cs    # Mã nguồn không phụ thuộc Unity
├── AntiCheatStandalone.csproj # Project file
├── bin/
│   └── Release/
│       └── netstandard2.0/
│           ├── AntiCheat.dll  # DLL đã biên dịch
│           ├── AntiCheat.pdb  # Debug symbols
│           └── AntiCheat.deps.json
└── README.md
```

## Build lại DLL
Nếu bạn muốn build lại hoặc chỉnh sửa:
```bash
cd /workspace/AntiCheatDLL
export PATH=$HOME/.dotnet:$PATH
dotnet build AntiCheatStandalone.csproj -c Release
```

## Lưu ý
- DLL này chỉ hoạt động trên Windows do sử dụng Windows API (kernel32.dll)
- Khi phát hiện cheat, ứng dụng sẽ tự động tắt ngay lập tức
- Kiểm tra được thực hiện định kỳ mỗi 2 giây (có thể điều chỉnh)