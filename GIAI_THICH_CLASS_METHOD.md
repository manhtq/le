# 📖 GIẢI THÍCH CLASS NAME & METHOD NAME

## 🎯 TÓM TẮT NHANH

Khi inject **AntiCheat.dll** vào game, bạn cần điền:
- **Namespace**: `AntiCheatSystem`
- **Class**: `AntiCheatBootstrap`  
- **Method**: `Initialize`

---

## 📚 GIẢI THÍCH CHI TIẾT

### 1️⃣ **Namespace là gì?**
Namespace giống như "thư mục" chứa các class trong code C#.

```csharp
namespace AntiCheatSystem  // <-- Đây là Namespace
{
    // Các class nằm trong namespace này
}
```

**Trong AntiCheat.dll**: Namespace là `AntiCheatSystem`

---

### 2️⃣ **Class Name là gì?**
Class là "bản thiết kế" cho một đối tượng trong lập trình.

```csharp
namespace AntiCheatSystem
{
    public class AntiCheatBootstrap  // <-- Đây là Class Name
    {
        // Code của class
    }
    
    public class AntiCheatStandalone  // <-- Một class khác
    {
        // Code khác
    }
}
```

**Trong AntiCheat.dll có 2 class chính:**
- `AntiCheatBootstrap` - Class khởi động (DÙNG CÁI NÀY)
- `AntiCheatStandalone` - Class chính chứa logic

---

### 3️⃣ **Method Name là gì?**
Method (phương thức) là "hành động" mà class có thể thực hiện.

```csharp
public class AntiCheatBootstrap
{
    public static void Initialize()  // <-- Đây là Method Name
    {
        // Code khởi động AntiCheat
    }
    
    public static void Shutdown()    // <-- Method khác
    {
        // Code tắt AntiCheat
    }
}
```

**Methods trong AntiCheatBootstrap:**
- `Initialize` - Khởi động AntiCheat (DÙNG CÁI NÀY)
- `Shutdown` - Tắt AntiCheat

---

## 🔍 XEM CODE THỰC TẾ

### **File: AntiCheatStandalone.cs**
```csharp
namespace AntiCheatSystem              // <-- NAMESPACE
{
    public class AntiCheatBootstrap     // <-- CLASS NAME
    {
        public static void Initialize() // <-- METHOD NAME
        {
            // Khởi tạo AntiCheat
            _instance = new AntiCheatStandalone();
            _instance.OnCheatDetected += msg => 
            { 
                Console.WriteLine("[AntiCheat] Detected: " + msg); 
            };
            _instance.Start();
        }
    }
}
```

---

## 💉 KHI INJECT, ĐIỀN NHƯ SAU:

### **Trong SharpMono Injector GUI:**
```
┌─────────────────────────────────────┐
│ DLL Settings                        │
├─────────────────────────────────────┤
│ DLL Path:    C:\AntiCheat.dll      │
│ Namespace:   AntiCheatSystem       │
│ Class:       AntiCheatBootstrap    │
│ Method:      Initialize            │
└─────────────────────────────────────┘
```

### **Giải thích từng field:**
| Field | Giá trị | Ý nghĩa |
|-------|---------|---------|
| **DLL Path** | `C:\AntiCheat.dll` | Đường dẫn đến file DLL |
| **Namespace** | `AntiCheatSystem` | "Thư mục" chứa class |
| **Class** | `AntiCheatBootstrap` | Class sẽ được gọi |
| **Method** | `Initialize` | Hàm sẽ được chạy |

---

## ❓ TẠI SAO CẦN THÔNG TIN NÀY?

Khi inject DLL vào game, injector cần biết:
1. **File DLL nào** để load vào game
2. **Namespace nào** để tìm class
3. **Class nào** chứa code cần chạy
4. **Method nào** để execute

**Quá trình inject:**
```
1. Load AntiCheat.dll vào memory của game
2. Tìm namespace "AntiCheatSystem"
3. Trong namespace đó, tìm class "AntiCheatBootstrap"
4. Trong class đó, gọi method "Initialize()"
5. AntiCheat bắt đầu hoạt động!
```

---

## 🎮 VÍ DỤ VỚI GAME KHÁC

### **Nếu bạn có DLL khác:**
```csharp
namespace MyCheatTool
{
    public class Loader
    {
        public static void Start() { }
    }
}
```

**Thì sẽ điền:**
- Namespace: `MyCheatTool`
- Class: `Loader`
- Method: `Start`

---

## ⚡ CÁC LỖI THƯỜNG GẶP

### ❌ **Lỗi: "Class not found"**
**Nguyên nhân:** Sai tên class
**Giải pháp:** Kiểm tra chính xác là `AntiCheatBootstrap` (không phải `AntiCheat`)

### ❌ **Lỗi: "Method not found"**
**Nguyên nhân:** Sai tên method
**Giải pháp:** Kiểm tra chính xác là `Initialize` (viết hoa chữ I)

### ❌ **Lỗi: "Namespace not found"**
**Nguyên nhân:** Sai namespace
**Giải pháp:** Kiểm tra chính xác là `AntiCheatSystem`

---

## 🔧 CÁCH KIỂM TRA THÔNG TIN CỦA DLL BẤT KỲ

### **Dùng dnSpy (recommended):**
1. Download dnSpy: https://github.com/dnSpy/dnSpy
2. Mở file DLL bằng dnSpy
3. Xem tree structure bên trái:
   ```
   📁 AntiCheat.dll
   └── 📁 AntiCheatSystem (namespace)
       ├── 📄 AntiCheatBootstrap (class)
       │   ├── Initialize() (method)
       │   └── Shutdown() (method)
       └── 📄 AntiCheatStandalone (class)
   ```

### **Dùng ILSpy:**
1. Download ILSpy
2. Load DLL vào
3. Browse structure tương tự

### **Dùng .NET Reflector:**
1. Mở DLL
2. Xem namespace → class → methods

---

## 📝 CHEAT SHEET

### **Cho AntiCheat.dll của bạn:**
```yaml
DLL Path:    C:\AntiCheat.dll
Namespace:   AntiCheatSystem
Class:       AntiCheatBootstrap
Method:      Initialize
```

### **Copy paste trực tiếp:**
```
AntiCheatSystem
AntiCheatBootstrap
Initialize
```

---

## 💡 TIPS

1. **Luôn dùng AntiCheatBootstrap** thay vì AntiCheatStandalone
   - Bootstrap là entry point được thiết kế để inject
   - Standalone cần khởi tạo phức tạp hơn

2. **Method phải là static**
   - `Initialize()` là static method nên có thể gọi trực tiếp
   - Không cần tạo instance của class

3. **Case-sensitive**
   - C# phân biệt chữ hoa/thường
   - `Initialize` ≠ `initialize` ≠ `INITIALIZE`

4. **Nếu inject fail:**
   - Thử method `Initialize` trước
   - Nếu không được, thử `Init` hoặc `Start`
   - Check lại bằng dnSpy

---

## 🚀 TÓM LẠI

**Khi inject AntiCheat.dll, chỉ cần nhớ:**

| Field | Value |
|-------|-------|
| **Namespace** | `AntiCheatSystem` |
| **Class** | `AntiCheatBootstrap` |
| **Method** | `Initialize` |

**Đây là thông tin CHÍNH XÁC cho DLL AntiCheat mà chúng ta đã tạo!**